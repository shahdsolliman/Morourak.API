using MediatR;
using Microsoft.Extensions.Logging;
using Morourak.Application.Common.Interfaces;
using Morourak.Application.Interfaces.Services;

namespace Morourak.Application.Common.Behaviours;

public class InvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IInvalidateCacheRequest
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<InvalidationBehavior<TRequest, TResponse>> _logger;

    public InvalidationBehavior(ICacheService cacheService, ILogger<InvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Only invalidate if the request was successful (assuming no exception was thrown by next())
        foreach (var key in request.CacheKeysToInvalidate)
        {
            if (key.EndsWith("*"))
            {
                _logger.LogInformation("Invalidating cache pattern: {CachePattern}", key);
                await _cacheService.RemoveByPatternAsync(key, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Invalidating cache key: {CacheKey}", key);
                await _cacheService.RemoveAsync(key, cancellationToken);
            }
        }

        return response;
    }
}
