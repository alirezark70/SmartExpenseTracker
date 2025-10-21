using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Redis.Services
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var database = _redis.GetDatabase();

                // Test basic operations
                var key = $"health_check_{Guid.NewGuid()}";
                var testValue = DateTime.UtcNow.ToString();

                await database.StringSetAsync(key, testValue, TimeSpan.FromSeconds(5));
                var result = await database.StringGetAsync(key);
                await database.KeyDeleteAsync(key);

                if (result != testValue)
                {
                    return HealthCheckResult.Unhealthy("Redis read/write test failed");
                }

                // Check connection status
                if (!_redis.IsConnected)
                {
                    return HealthCheckResult.Unhealthy("Redis is not connected");
                }

                // Get server stats
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var ping = await database.PingAsync();

                return HealthCheckResult.Healthy($"Redis is healthy. Ping: {ping.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed");
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
}
