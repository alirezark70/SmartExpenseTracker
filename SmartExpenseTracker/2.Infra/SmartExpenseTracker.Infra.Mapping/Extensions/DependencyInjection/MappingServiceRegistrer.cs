using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using SmartExpenseTracker.Infra.Mapping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Extensions.DependencyInjection
{
    public static class MappingServiceRegistrer
    {
        public static IServiceCollection RegisterMappingService(this IServiceCollection services)
        {
            // Mapster Configuration
            //var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            //typeAdapterConfig.Scan(Assembly.GetAssembly(typeof(IMappingConfig))!);
            //typeAdapterConfig.Default.PreserveReference(true);
            //services.AddSingleton(typeAdapterConfig);

            // Add Mapster
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped<IMappingService, MappingService>();

            return services;
        }
    }
}
