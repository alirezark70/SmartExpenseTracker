using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Infra.Redis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Redis.Services
{
    public class RedisCacheService : IDistributedCacheService, IDisposable
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly RedisSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _semaphore;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            IOptions<RedisSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = _redis.GetDatabase(_settings.Database);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _semaphore = new SemaphoreSlim(_settings.MaxDegreeOfParallelism, _settings.MaxDegreeOfParallelism);
        }

        private string GetKey(string key) => $"{_settings.InstanceName}:{key}";

        // Basic Operations
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(GetKey(key));
                if (value.IsNullOrEmpty)
                    return null;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value for key {Key}", key);
                return null;
            }
        }

        public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _database.StringGetAsync(GetKey(key));
                return value.IsNullOrEmpty ? null : value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting string value for key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                var finalExpiry = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes ?? 60);
                await _database.StringSetAsync(GetKey(key), json, finalExpiry);

                if (_settings.EnableLogging)
                    _logger.LogDebug("Set cache for key {Key} with expiry {Expiry}", key, finalExpiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value for key {Key}", key);
                throw;
            }
        }

        public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var finalExpiry = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes ?? 60);
                await _database.StringSetAsync(GetKey(key), value, finalExpiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting string value for key {Key}", key);
                throw;
            }
        }

        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _database.KeyDeleteAsync(GetKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key}", key);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _database.KeyExistsAsync(GetKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key {Key}", key);
                return false;
            }
        }

        // Batch Operations
        public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class
        {
            var result = new Dictionary<string, T?>();

            try
            {
                var redisKeys = keys.Select(k => (RedisKey)GetKey(k)).ToArray();
                var values = await _database.StringGetAsync(redisKeys);

                for (int i = 0; i < redisKeys.Length; i++)
                {
                    var originalKey = keys.ElementAt(i);
                    if (!values[i].IsNullOrEmpty)
                    {
                        result[originalKey] = JsonSerializer.Deserialize<T>(values[i]!, _jsonOptions);
                    }
                    else
                    {
                        result[originalKey] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple values");
            }

            return result;
        }

        public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var tasks = new List<Task>();
                var finalExpiry = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes ?? 60);

                foreach (var batch in items.Chunk(_settings.BatchSize))
                {
                    await _semaphore.WaitAsync(cancellationToken);

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            var transaction = _database.CreateTransaction();

                            foreach (var item in batch)
                            {
                                var json = JsonSerializer.Serialize(item.Value, _jsonOptions);
                                _ = transaction.StringSetAsync(GetKey(item.Key), json, finalExpiry);
                            }

                            await transaction.ExecuteAsync();
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, cancellationToken);

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple values");
                throw;
            }
        }

        public async Task<long> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var keys = server.Keys(_settings.Database, GetKey(pattern)).ToArray();

                if (keys.Length == 0)
                    return 0;

                return await _database.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing keys by pattern {Pattern}", pattern);
                return 0;
            }
        }

        // Advanced Operations
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, expiry, cancellationToken);
            return value;
        }

        public async Task<bool> TryAddAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                var finalExpiry = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes ?? 60);
                return await _database.StringSetAsync(GetKey(key), json, finalExpiry, When.NotExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to add value for key {Key}", key);
                return false;
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _database.StringIncrementAsync(GetKey(key), value);

                if (expiry.HasValue)
                {
                    await _database.KeyExpireAsync(GetKey(key), expiry);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing key {Key}", key);
                throw;
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _database.StringDecrementAsync(GetKey(key), value);

                if (expiry.HasValue)
                {
                    await _database.KeyExpireAsync(GetKey(key), expiry);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing key {Key}", key);
                throw;
            }
        }

        // Hash Operations
        public async Task HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.HashSetAsync(GetKey(key), field, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting hash field {Field} for key {Key}", field, key);
                throw;
            }
        }

        public async Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var value = await _database.HashGetAsync(GetKey(key), field);
                if (value.IsNullOrEmpty)
                    return null;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hash field {Field} for key {Key}", field, key);
                return null;
            }
        }

        public async Task<Dictionary<string, T?>> HashGetAllAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var result = new Dictionary<string, T?>();

            try
            {
                var hashEntries = await _database.HashGetAllAsync(GetKey(key));

                foreach (var entry in hashEntries)
                {
                    if (!entry.Value.IsNullOrEmpty)
                    {
                        result[entry.Name!] = JsonSerializer.Deserialize<T>(entry.Value!, _jsonOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hash fields for key {Key}", key);
            }

            return result;
        }

        public async Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _database.HashDeleteAsync(GetKey(key), field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hash field {Field} for key {Key}", field, key);
                return false;
            }
        }

        // List Operations
        public async Task<long> ListAddAsync<T>(string key, T value, bool addToEnd = true, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);

                if (addToEnd)
                    return await _database.ListRightPushAsync(GetKey(key), json);
                else
                    return await _database.ListLeftPushAsync(GetKey(key), json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to list {Key}", key);
                throw;
            }
        }

        public async Task<List<T?>> ListGetAsync<T>(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default) where T : class
        {
            var result = new List<T?>();

            try
            {
                var values = await _database.ListRangeAsync(GetKey(key), start, stop);

                foreach (var value in values)
                {
                    if (!value.IsNullOrEmpty)
                    {
                        result.Add(JsonSerializer.Deserialize<T>(value!, _jsonOptions));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list {Key}", key);
            }

            return result;
        }

        public async Task<T?> ListPopAsync<T>(string key, bool popFromEnd = true, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                RedisValue value;

                if (popFromEnd)
                    value = await _database.ListRightPopAsync(GetKey(key));
                else
                    value = await _database.ListLeftPopAsync(GetKey(key));

                if (value.IsNullOrEmpty)
                    return null;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error popping from list {Key}", key);
                return null;
            }
        }

        // Set Operations
        public async Task<bool> SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.SetAddAsync(GetKey(key), json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to set {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.SetContainsAsync(GetKey(key), json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking set {Key}", key);
                return false;
            }
        }

        public async Task<HashSet<T?>> SetGetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var result = new HashSet<T?>();

            try
            {
                var values = await _database.SetMembersAsync(GetKey(key));

                foreach (var value in values)
                {
                    if (!value.IsNullOrEmpty)
                    {
                        result.Add(JsonSerializer.Deserialize<T>(value!, _jsonOptions));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting set {Key}", key);
            }

            return result;
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }

}
