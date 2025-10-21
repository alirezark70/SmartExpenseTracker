using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Infra.Redis.Configuration;
using SmartExpenseTracker.Infra.Redis.Services;
using StackExchange.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SmartExpenseTracker.Infra.Redis.Extensions.DependencyInjection
{
    public static class RedisServiceRegistrer
    {
        public static IServiceCollection RegisterRedisService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register settings
            services.Configure<RedisSettings>(options =>
            {
                configuration.GetSection(RedisSettings.SectionName).Bind(options);
            });

            var redisSettings = configuration
                                        .GetSection(RedisSettings.SectionName)
                                        .Get<RedisSettings>() ?? new RedisSettings();


            // Register Redis connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();

                var configOptions = ConfigurationOptions.Parse(redisSettings.ConnectionString);
                configOptions.Password = redisSettings.Password;
                configOptions.Ssl = redisSettings.Ssl;
                configOptions.SslHost = redisSettings.SslHost;
                configOptions.ConnectTimeout = redisSettings.ConnectTimeout;
                configOptions.SyncTimeout = redisSettings.SyncTimeout;
                configOptions.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
                configOptions.ConnectRetry = redisSettings.ConnectRetry;
                configOptions.KeepAlive = redisSettings.KeepAlive;
                configOptions.DefaultDatabase = redisSettings.Database;

                // Add retry policy
                configOptions.ReconnectRetryPolicy = new LinearRetry(5000);

                var connection = ConnectionMultiplexer.Connect(configOptions);

                // Log connection events
                connection.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError("Redis connection failed: {FailureType}", args.FailureType);
                };

                connection.ConnectionRestored += (sender, args) =>
                {
                    logger.LogInformation("Redis connection restored");
                };

                connection.ErrorMessage += (sender, args) =>
                {
                    logger.LogError("Redis error: {Message}", args.Message);
                };

                return connection;
            });

            // Register cache service
            services.AddSingleton<IDistributedCacheService, RedisCacheService>();

            // Register legacy cache service adapter if needed
            services.AddSingleton<ICacheService>(provider =>
                new RedisCacheAdapter(provider.GetRequiredService<IDistributedCacheService>()));

            // Add health checks
            services.AddHealthChecks()
                .AddCheck<RedisHealthCheck>("redis", tags: new[] { "redis", "cache" });

            return services;
        }
    }
}
