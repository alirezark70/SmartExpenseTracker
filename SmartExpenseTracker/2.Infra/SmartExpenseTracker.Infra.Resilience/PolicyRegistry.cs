using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using SmartExpenseTracker.Infra.Resilience.Configuration;
using SmartExpenseTracker.Infra.Resilience.Contracts;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience
{
    public class PolicyRegistry : IPolicyRegistry
    {
        private readonly Dictionary<PolicyType, object> _pipelines = new();
        private readonly IOptions<ResilienceOptions> _options;
        private readonly ILogger<PolicyRegistry> _logger;

        public PolicyRegistry(IOptions<ResilienceOptions> options, ILogger<PolicyRegistry> logger)
        {
            _options = options;
            _logger = logger;
            InitializeDefaultPipelines();
        }

        public ResiliencePipeline<T> GetPipeline<T>(PolicyType policyType)
        {
            if (_pipelines.TryGetValue(policyType, out var pipeline) && pipeline is ResiliencePipeline<T> typedPipeline)
            {
                return typedPipeline;
            }

            throw new InvalidOperationException($"Pipeline '{policyType.ToString()}' not found or type mismatch.");
        }

        public ResiliencePipeline GetPipeline(PolicyType policyType)
        {

            if (_pipelines.TryGetValue(policyType, out var pipeline) && pipeline is ResiliencePipeline untypedPipeline)
            {
                return untypedPipeline;
            }

            throw new InvalidOperationException($"Pipeline '{policyType.ToString()}' not found.");
        }

        public void RegisterPipeline<T>(PolicyType policyType, ResiliencePipeline<T> pipeline)
        {
            _pipelines[policyType] = pipeline;
            _logger.LogInformation("Registered typed pipeline: {PolicyName}", policyType.ToString());
        }

        public void RegisterPipeline(PolicyType policyType, ResiliencePipeline pipeline)
        {
            _pipelines[policyType] = pipeline;
            _logger.LogInformation("Registered pipeline: {PolicyName}", policyType);
        }

        private void InitializeDefaultPipelines()
        {
            // Standard HTTP Pipeline
            var httpPipeline = CreateHttpPipeline();
            RegisterPipeline(PolicyType.HttpStandard, httpPipeline);

            // Database Pipeline
            var dbPipeline = CreateDatabasePipeline();
            RegisterPipeline(PolicyType.Database, dbPipeline);

            // Critical Service Pipeline
            var criticalPipeline = CreateCriticalServicePipeline();
            RegisterPipeline(PolicyType.CriticalService, criticalPipeline);

            //external Service Pipeline

            var externalPipline = CreateExternalServciePipleline();
            RegisterPipeline(PolicyType.ExternalService, criticalPipeline);


        }

        private ResiliencePipeline CreateHttpPipeline()
        {
            var options = _options.Value;

            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                    Delay = options.Retry.InitialDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = options.Retry.UseJitter,
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "Retry attempt {Attempt} after {Delay}ms",
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = options.CircuitBreaker.SamplingDuration,
                    MinimumThroughput = options.CircuitBreaker.FailureThreshold,
                    BreakDuration = options.CircuitBreaker.BreakDuration,
                    OnOpened = args =>
                    {
                        _logger.LogError("Circuit breaker opened!");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("Circuit breaker closed!");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(options.Timeout.RequestTimeout)
                .Build();
        }

        private ResiliencePipeline CreateDatabasePipeline()
        {
            var options = _options.Value;

            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(100),
                    BackoffType = DelayBackoffType.Linear,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<DbUpdateException>()
                        .Handle<TimeoutException>()
                })
                .AddTimeout(TimeSpan.FromSeconds(5))
                .Build();
        }

        private ResiliencePipeline CreateExternalServciePipleline()
        {
            var options = _options.Value;

            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3, // معقول برای external
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true, // مهم برای external services
                    MaxDelay = TimeSpan.FromSeconds(30), // محدودیت delay
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>() // HTTP specifi
                        .Handle<TaskCanceledException>() // Timeout
                        .Handle<SocketException>(), // Network issues
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "External service retry attempt {Attempt} after {Delay}ms",
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.4, // 40% - بین http و critical
                    SamplingDuration = TimeSpan.FromMinutes(2),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(45),
                    OnOpened = args =>
                    {
                        _logger.LogError("External service circuit breaker opened!");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(20)) // مناسب برای external
                .Build();
        }

        private ResiliencePipeline CreateCriticalServicePipeline()
        {
            var options = _options.Value;

            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    MaxDelay = TimeSpan.FromSeconds(60),
                    UseJitter = true
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.3,
                    SamplingDuration = TimeSpan.FromMinutes(1),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromMinutes(2)
                })
                .AddTimeout(TimeSpan.FromSeconds(30))
                .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
                {
                    PermitLimit = options.Bulkhead.MaxParallelization,
                    QueueLimit = options.Bulkhead.MaxQueuingActions
                })
                .Build();
        }


    }
}
