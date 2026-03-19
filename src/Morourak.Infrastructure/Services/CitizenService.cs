using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Services
{
    /// <summary>
    /// Cached implementation of citizen lookup using IDistributedCache.
    /// Uses the CitizenRegistry entity as the current source of citizen data.
    /// </summary>
    public sealed class CitizenService : ICitizenService
    {
        private const string CacheKeyPrefix = "citizen:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public CitizenService(IUnitOfWork unitOfWork, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<CitizenRegistry?> GetByIdAsync(int id)
        {
            var cacheKey = BuildCacheKey(id);

            // Try cache first
            var cachedBytes = await _cache.GetAsync(cacheKey);
            if (cachedBytes is { Length: > 0 })
            {
                var cachedCitizen = JsonSerializer.Deserialize<CitizenRegistry>(cachedBytes);
                if (cachedCitizen is not null)
                    return cachedCitizen;
            }

            // Fallback to repository
            var citizenRepo = _unitOfWork.Repository<CitizenRegistry>();
            var citizen = await citizenRepo.GetByIdAsync(id);
            if (citizen is null)
                return null;

            // Store in cache with absolute expiration
            var bytes = JsonSerializer.SerializeToUtf8Bytes(citizen);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };

            await _cache.SetAsync(cacheKey, bytes, options);

            return citizen;
        }

        private static string BuildCacheKey(int id) => $"{CacheKeyPrefix}{id}";
    }
}

