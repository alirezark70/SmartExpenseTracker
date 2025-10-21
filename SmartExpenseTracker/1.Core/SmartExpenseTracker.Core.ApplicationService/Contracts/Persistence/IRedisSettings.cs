using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface IRedisSettings
    {
       string ConnectionString { get; set; }
       string InstanceName { get; set; }
       int Database { get; set; } 
       int ConnectTimeout { get; set; }
       int SyncTimeout { get; set; }
       bool AbortOnConnectFail { get; set; }
       int ConnectRetry { get; set; }
       int KeepAlive { get; set; }
       string? Password { get; set; }
       bool Ssl { get; set; }
       string? SslHost { get; set; }
       int? DefaultExpirationMinutes { get; set; } 
       bool EnableLogging { get; set; } 
       int MaxDegreeOfParallelism { get; set; } 
       int BatchSize { get; set; }
    }
}
