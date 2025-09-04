using Microsoft.Extensions.DependencyInjection;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Infrastructure.SimpleDateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infrastructure.Extenstions.DependencyInjection
{
    public static class SimpleDateTimeRegistrer
    {
        public static IServiceCollection RegisterSimpleDateTimeService(this IServiceCollection services)
        {
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();

            return services;
        }
    }
}
