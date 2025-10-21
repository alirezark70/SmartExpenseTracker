using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Redis.Configuration
{
    public class RedisSettings : IRedisSettings
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "SmartExpenseTracker";
        public int Database { get; set; } = 0;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3;
        public int KeepAlive { get; set; } = 60;
        public string? Password { get; set; }
        public bool Ssl { get; set; } = false;
        public string? SslHost { get; set; }
        public int? DefaultExpirationMinutes { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public int MaxDegreeOfParallelism { get; set; } = 10;
        public int BatchSize { get; set; } = 100;
    }
}
