using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts;
using SmartExpenseTracker.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.ExternalServices.Posts;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using SmartExpenseTracker.Infra.Resilience.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Extensions.DependencyInjection
{
    public static class ExternalServiceRegistrer
    {
        public static IServiceCollection RegisterExternalService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPostInquiryService>(provider =>
            {
                var innerService = new PostInquiryService(
                    provider.GetRequiredService<HttpClient>(),
                    provider.GetRequiredService<IMappingService>(),
                    provider.GetRequiredService<ILogger<PostInquiryService>>());

                return new ResilientPostInquiryService(
                    provider.GetRequiredService<ILogger<ResilientPostInquiryService>>(),
                    provider.GetRequiredService<IPolicyRegistry>(),
                    innerService,
                    provider.GetRequiredService<IResilienceTelemetry>());
            });

            services.AddHttpClient<IPostInquiryService, PostInquiryService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ExternalServices:PersonApi:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .InjectStandardResilienceHandler();


            return services;
        }   
    }
}
