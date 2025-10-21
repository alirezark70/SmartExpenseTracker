using SmartExpenseTracker.Core.Domain.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Redis.Services
{
    public class RedisCacheAdapter : ICacheService
    {
        private readonly IDistributedCacheService _distributedCache;

        public RedisCacheAdapter(IDistributedCacheService distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class 
        {
            return await _distributedCache.GetAsync<T>(key, cancellationToken);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
            where T : class
        {
            await _distributedCache.SetAsync(key, value, expiry, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }

        public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveByPatternAsync($"{prefixKey}*", cancellationToken);
        }
    }
}
