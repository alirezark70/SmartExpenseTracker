using App.Metrics;
using App.Metrics.Counter;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using SmartExpenseTracker.Infra.Resilience.Contracts;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience
{
    public class ResilienceTelemetry : IResilienceTelemetry
    {
        private readonly ILogger<ResilienceTelemetry> _logger;
        private readonly IMetrics _metrics;

        public ResilienceTelemetry(ILogger<ResilienceTelemetry> logger, IMetrics metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        public void RecordRetryAttempt(PolicyType policyType, int attemptNumber, TimeSpan delay)
        {
            _logger.LogInformation(
                "Retry attempt {AttemptNumber} for policy {PolicyName} after {Delay}ms",
                attemptNumber, policyType.ToString(), delay.TotalMilliseconds);

            _metrics.Measure.Counter.Increment(
                new CounterOptions { Name = "resilience_retry_attempts", Tags = new MetricTags("policy", policyType.ToString()) });
        }

        public void RecordCircuitBreakerStateChange(PolicyType policyType, CircuitState newState)
        {
            _logger.LogWarning(
                "Circuit breaker {PolicyName} changed state to {NewState}",
                policyType.ToString(), newState);

            _metrics.Measure.Counter.Increment(
                new CounterOptions
                {
                    Name = "resilience_circuit_breaker_state_changes",
                    Tags = new MetricTags(new[] { "policy", "state" }, new[] { policyType.ToString(), newState.ToString() })
                });
        }

        public void RecordTimeout(PolicyType policyType, TimeSpan elapsed)
        {
            _logger.LogWarning(
                "Timeout occurred for policy {PolicyName} after {Elapsed}ms",
                 policyType.ToString(), elapsed.TotalMilliseconds);

            _metrics.Measure.Counter.Increment(
                new CounterOptions { Name = "resilience_timeouts", Tags = new MetricTags("policy", policyType.ToString()) });
        }

        public void RecordFallback(PolicyType policyType, string reason)
        {
            _logger.LogInformation(
                "Fallback executed for policy {PolicyName}. Reason: {Reason}",
                 policyType.ToString(), reason);

            _metrics.Measure.Counter.Increment(
                new CounterOptions
                {
                    Name = "resilience_fallbacks",
                    Tags = new MetricTags(new[] { "policy", "reason" }, new[] { policyType.ToString(), reason })
                });
        }
    }
   
}
