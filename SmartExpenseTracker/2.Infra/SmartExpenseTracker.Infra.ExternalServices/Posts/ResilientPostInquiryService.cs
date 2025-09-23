using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Posts;
using SmartExpenseTracker.Infra.Resilience.Contracts;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.ExternalServices.Posts
{
    public class ResilientPostInquiryService : IPostInquiryService
    {
        private readonly IPostInquiryService _innerService;
        private readonly IPolicyRegistry _policyRegistry;
        private readonly ILogger<ResilientPostInquiryService> _logger;
        private readonly ResiliencePipeline _pipeline;
        private readonly IResilienceTelemetry _telemetry;

        public ResilientPostInquiryService(ILogger<ResilientPostInquiryService> logger, IPolicyRegistry policyRegistry, IPostInquiryService innerService, IResilienceTelemetry telemetry)
        {
            _logger = logger;
            _policyRegistry = policyRegistry;
            _innerService = innerService;
            _pipeline = _policyRegistry.GetPipeline(PolicyType.ExternalService);
            _telemetry = telemetry;
            Id = _innerService.Id;
        }

        public Guid Id { get; init; }

        public async Task<IEnumerable<PostDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var context = ResilienceContextPool.Shared.Get(cancellationToken);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                context.Properties.Set(new ResiliencePropertyKey<string>("operation"), "GetAll");

                context.Properties.Set(new ResiliencePropertyKey<Action<int, TimeSpan>>("OnRetry"),
                (attemptNumber, delay) => _telemetry.RecordRetryAttempt(PolicyType.ExternalService, attemptNumber, delay));

                context.Properties.Set(new ResiliencePropertyKey<Action<CircuitState>>("OnCircuitStateChange"),
                (state) => _telemetry.RecordCircuitBreakerStateChange(PolicyType.ExternalService, state));

                return await _pipeline.ExecuteAsync(
                    async (ctx) =>
                    {
                        var cancellation = ctx.CancellationToken;
                        _logger.LogDebug($"Executing GetAll with resilience for trackId {Id}");
                        //_telemetry.RecordFallback(PolicyType.ExternalService, "Timeout");
                        return await _innerService.GetAllAsync(cancellation);
                    },
                    context);
            }
            catch (TimeoutRejectedException)
            {
                _logger.LogWarning("Request timed out for track Id: {Id}", Id);
                _telemetry.RecordTimeout(PolicyType.ExternalService, stopwatch.Elapsed);
                _telemetry.RecordFallback(PolicyType.ExternalService, "Timeout");
                return GetFallbackPerson(Id);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open for Post inquiry service");
                _telemetry.RecordFallback(PolicyType.ExternalService, "CircuitBreakerOpen");
                return GetFallbackPerson(Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in resilient Post inquiry");
                _telemetry.RecordFallback(PolicyType.ExternalService, $"UnhandledException_{ex.GetType().Name}");
                return GetFallbackPerson(Id);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        public async Task<PostDto?> GetPostByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var context = ResilienceContextPool.Shared.Get(cancellationToken);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();


            try
            {
                context.Properties.Set(new ResiliencePropertyKey<string>("operation"), "GetPostById");
                context.Properties.Set(new ResiliencePropertyKey<int>("postId"), id);

                context.Properties.Set(new ResiliencePropertyKey<Action<int, TimeSpan>>("OnRetry"),
                (attemptNumber, delay) => _telemetry.RecordRetryAttempt(PolicyType.ExternalService, attemptNumber, delay));

                context.Properties.Set(new ResiliencePropertyKey<Action<CircuitState>>("OnCircuitStateChange"),
                (state) => _telemetry.RecordCircuitBreakerStateChange(PolicyType.ExternalService, state));

                return await _pipeline.ExecuteAsync(
                    async (ctx) =>
                    {
                        var cancellation = ctx.CancellationToken;
                        _logger.LogDebug($"Executing GetPostById with resilience for id {id}");
                        //_telemetry.RecordFallback(PolicyType.ExternalService, "Timeout");
                        return await _innerService.GetPostByIdAsync(id, cancellation);
                    },
                    context);
            }
            catch (TimeoutRejectedException)
            {
                _logger.LogWarning("Request timed out for Id: {Id}", id);
                _telemetry.RecordTimeout(PolicyType.ExternalService, stopwatch.Elapsed);
                _telemetry.RecordFallback(PolicyType.ExternalService, "Timeout");
                return GetFallbackPerson(id);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open for Post inquiry service");
                _telemetry.RecordFallback(PolicyType.ExternalService, "CircuitBreakerOpen");
                return GetFallbackPerson(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in resilient Post inquiry");
                _telemetry.RecordFallback(PolicyType.ExternalService, $"UnhandledException_{ex.GetType().Name}");
                return GetFallbackPerson(id);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        private PostDto? GetFallbackPerson(int id)
        {
            _logger.LogInformation("Returning fallback Post for Id: {Id}", id);

            return new PostDto(
                Id: 0,
                Title: "نامشخص",
                Body: "نامشخص",
                UserId: 0
            );
        }

        private IEnumerable<PostDto>? GetFallbackPerson(Guid id)
        {
            _logger.LogInformation("Returning fallback Post for Id: {Id}", id);

            return Enumerable.Empty<PostDto>();
        }
    }
}
