namespace Morourak.Application.Common.Interfaces;

public interface ICacheableRequest
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
