using App.Metrics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartExpenseTracker.Core.ApplicationService.Behaviors;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using SmartExpenseTracker.EndPoint.RestApi.Filters;
using SmartExpenseTracker.EndPoint.RestApi.Middleware;

namespace SmartExpenseTracker.EndPoint.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddResponseFramework(this IServiceCollection services)
        {
            // Add Response Service

            // Add MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Add Filters
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseActionFilter>();
                options.Filters.Add<GlobalExceptionFilter>();
            });

            // Configure API Behavior Options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    var response = new ErrorResponse("خطای اعتبارسنجی", ResponseStatus.UnprocessableEntity);
                    foreach (var error in errors)
                    {
                        foreach (var message in error.Value)
                        {
                            response.AddError(error.Key, message);
                        }
                    }

                    return new UnprocessableEntityObjectResult(response);
                };
            });



            return services;
        }


        public static IServiceCollection AddMetericsToDI(this IServiceCollection services)
        {
            var metrics = new MetricsBuilder().Build();

            services.AddMetrics(metrics);

            services.AddMetricsEndpoints();
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();

            return services;
        }

        public static IApplicationBuilder UseResponseFramework(this IApplicationBuilder app)
        {

            app.UseMiddleware<ResponseWrapperMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseMetericsMiddleware(this IApplicationBuilder app)
        {

            app.UseMetricsAllMiddleware();
            app.UseMetricsAllEndpoints();

            return app;
        }
    }
}
