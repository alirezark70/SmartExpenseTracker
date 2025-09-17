using IdGen.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.IdGenerators.Extentions.DependencyInjection
{
    public  static class SnowflakeIdGeneratorRegistrer
    {
        public static IServiceCollection RegisterSnowflakeIdGeneratorService(this IServiceCollection services,
                                                                         int idGeneratorId)
        {
            services.AddIdGen(idGeneratorId);
            services.AddSingleton<Core.ApplicationService.Contracts.IIdGenerator<long>, SnowflakeIdGenerator>();
            return services;
        }
    }
}
