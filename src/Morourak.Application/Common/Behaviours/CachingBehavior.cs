using MediatR;
using Microsoft.Extensions.Logging;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.Interfaces.Services;

namespace Morourak.Application.Common.Behaviours;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableRequest
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for {RequestName} with key: {CacheKey}", typeof(TRequest).Name, cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for {RequestName} with key: {CacheKey}. Fetching from database.", typeof(TRequest).Name, cacheKey);
        var response = await next();

        if (response != null)
        {
            await _cacheService.SetAsync(cacheKey, response, request.Expiration, cancellationToken);
        }

        return response;
    }
}
