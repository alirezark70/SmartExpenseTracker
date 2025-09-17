using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Configuration
{
    public class ResilienceOptions
    {
        public const string SectionName = "Resilience";

        public RetryOptions Retry { get; set; } = new();
        public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
        public TimeoutOptions Timeout { get; set; } = new();
        public BulkheadOptions Bulkhead { get; set; } = new();
    }
}
