using Polly.CircuitBreaker;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Contracts
{
    public interface IResilienceTelemetry
    {
        void RecordRetryAttempt(PolicyType policyType, int attemptNumber, TimeSpan delay);
        void RecordCircuitBreakerStateChange(PolicyType policyType, CircuitState newState);
        void RecordTimeout(PolicyType policyType, TimeSpan elapsed);
        void RecordFallback(PolicyType policyType, string reason);
    }
}
