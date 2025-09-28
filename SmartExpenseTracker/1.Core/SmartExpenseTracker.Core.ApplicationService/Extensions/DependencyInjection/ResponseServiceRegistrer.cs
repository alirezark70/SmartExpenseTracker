using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartExpenseTracker.Core.ApplicationService.Behaviors;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Services.Response;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using SmartExpenseTracker.Infra.Mapping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Extensions.DependencyInjection
{
    public static class ResponseServiceRegistrer
    {
        public static IServiceCollection RegisterResponseService(this IServiceCollection services)
        {
            services.AddTransient<IResponseService, ResponseService>();


            // Mapster Configuration
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(Assembly.GetAssembly(typeof(IMappingConfig))!);
            typeAdapterConfig.Default.PreserveReference(true);

            // Add Mapster
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped<IMappingService, MappingService>();


            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // ثبت رفتارها به ترتیب اجرا
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            });


            return services;
        }
    }
}
