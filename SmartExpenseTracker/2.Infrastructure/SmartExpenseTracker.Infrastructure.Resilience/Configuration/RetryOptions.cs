using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Configuration
{
    public class RetryOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public double BackoffMultiplier { get; set; } = 2.0;
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
        public bool UseJitter { get; set; } = true;
    }
}
