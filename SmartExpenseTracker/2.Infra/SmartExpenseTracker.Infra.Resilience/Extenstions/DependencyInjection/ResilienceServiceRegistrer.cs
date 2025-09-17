using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using SmartExpenseTracker.Infra.Resilience.Configuration;
using SmartExpenseTracker.Infra.Resilience.Contracts;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Extenstions.DependencyInjection
{
    public static class ResilienceServiceRegistrer
    {
        public static IServiceCollection RegisterExternalService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Options
            services.Configure<ResilienceOptions>(
                configuration.GetSection(ResilienceOptions.SectionName));

            // Register Policy Registry
            services.AddSingleton<IPolicyRegistry, PolicyRegistry>();


            //must be move to external service
            //services.AddScoped<IPostInquiryService>(provider =>
            //{
            //    var innerService = new PostInquiryService(
            //        provider.GetRequiredService<HttpClient>(),
            //        provider.GetRequiredService<IMappingService>(),
            //        provider.GetRequiredService<ILogger<PostInquiryService>>());

            //    return new ResilientPostInquiryService(
            //        provider.GetRequiredService<ILogger<ResilientPostInquiryService>>(),
            //        provider.GetRequiredService<IPolicyRegistry>(),
            //        innerService,
            //        provider.GetRequiredService<IResilienceTelemetry>());
            //});


            // Register HTTP Clients with Resilience
            //services.AddHttpClient<IPostInquiryService, PostInquiryService>(client =>
            //{
            //    client.BaseAddress = new Uri(configuration["ExternalServices:PersonApi:BaseUrl"]!);
            //    client.DefaultRequestHeaders.Add("Accept", "application/json");
            //    client.Timeout = TimeSpan.FromSeconds(30);
            //})
            //.InjectStandardResilienceHandler();

            services.AddSingleton<IResilienceTelemetry, ResilienceTelemetry>();

            services.AddHttpContextAccessor();

            //services.AddScoped<ICurrentUserService, ICurrentUserService>();

            // Register Decorated Service
            //services.Decorate<IPostInquiryService, ResilientPostInquiryService>();




            // Register Custom Resilience Pipelines
            services.AddResiliencePipeline(PolicyType.ExternalService, (builder, context) =>
            {

                var telemetry = context.ServiceProvider.GetRequiredService<IResilienceTelemetry>();


                var options = context.ServiceProvider
                    .GetRequiredService<IOptions<ResilienceOptions>>().Value;

                builder
                    .AddRetry(new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutRejectedException>(),
                        MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                        Delay = options.Retry.InitialDelay,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = options.Retry.UseJitter,
                        OnRetry = args =>
                        {
                            var logger = context.ServiceProvider.GetRequiredService<ILogger<PolicyRegistry>>();
                            telemetry.RecordRetryAttempt(
                            PolicyType.ExternalService,
                            args.AttemptNumber,
                            args.RetryDelay);

                            logger.LogWarning(
                                "PostInquiry Retry {Attempt} after {Delay}ms",
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
                        ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutRejectedException>(),
                        OnOpened = args =>
                        {
                            telemetry.RecordCircuitBreakerStateChange(
                                PolicyType.ExternalService,
                                CircuitState.Open);

                            return ValueTask.CompletedTask;
                        },

                        OnClosed = args =>
                        {
                            telemetry.RecordCircuitBreakerStateChange(
                                PolicyType.ExternalService,
                                CircuitState.Closed);

                            return ValueTask.CompletedTask;
                        },
                        OnHalfOpened = args =>
                        {
                            telemetry.RecordCircuitBreakerStateChange(
                                PolicyType.ExternalService,
                                CircuitState.HalfOpen);

                            return ValueTask.CompletedTask;
                        }

                    })
                    .AddTimeout(new TimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromSeconds(5),
                        OnTimeout = args =>
                        {
                            telemetry.RecordTimeout(
                                PolicyType.ExternalService,
                                args.Timeout);

                            return ValueTask.CompletedTask;
                        }
                    });
            });




            return services;


        }

        public static IHttpClientBuilder InjectStandardResilienceHandler(
       this IHttpClientBuilder builder)
        {

            builder.AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

            return builder;
        }
    }
}
