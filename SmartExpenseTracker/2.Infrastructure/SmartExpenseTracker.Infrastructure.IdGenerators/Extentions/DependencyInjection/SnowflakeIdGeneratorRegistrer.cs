using IdGen.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SmartExpenseTracker.Infrastructure.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infrastructure.Extentions.DependencyInjection
{
    public  static class SnowflakeIdGeneratorRegistrer
    {
        public static IServiceCollection RegisterSnowflakeIdGeneratorService(this IServiceCollection services,
                                                                         int idGeneratorId)
        {
            services.AddIdGen(idGeneratorId);
            services.AddSingleton<SmartExpenseTracker.Core.ApplicationService.Contracts.IIdGenerator<long>, SnowflakeIdGenerator>();
            return services;
        }
    }
}
