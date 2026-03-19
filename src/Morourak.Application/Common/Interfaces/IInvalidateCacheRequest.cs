namespace Morourak.Application.Common.Interfaces;

public interface IInvalidateCacheRequest
{
    string[] CacheKeysToInvalidate { get; }
}
