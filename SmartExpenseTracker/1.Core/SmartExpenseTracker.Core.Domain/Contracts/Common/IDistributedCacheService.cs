using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Contracts.Common
{
    public interface IDistributedCacheService
    {
        // Basic Operations
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
        Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        // Batch Operations
        Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;
        Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
        Task<long> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

        // Advanced Operations
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
        Task<bool> TryAddAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
        Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

        // Hash Operations
        Task HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default) where T : class;
        Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default) where T : class;
        Task<Dictionary<string, T?>> HashGetAllAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default);

        // List Operations
        Task<long> ListAddAsync<T>(string key, T value, bool addToEnd = true, CancellationToken cancellationToken = default) where T : class;
        Task<List<T?>> ListGetAsync<T>(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default) where T : class;
        Task<T?> ListPopAsync<T>(string key, bool popFromEnd = true, CancellationToken cancellationToken = default) where T : class;

        // Set Operations
        Task<bool> SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;
        Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;
        Task<HashSet<T?>> SetGetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    }
}
