using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Redis.Extensions.DependencyInjection
{
    public static class RedisServiceRegistrer
    {
        public static IServiceCollection RegisterRedisService(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
