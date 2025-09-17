using SmartExpenseTracker.Core.ApplicationService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.SimpleDateTime
{
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetDateTimeNow()=> DateTime.UtcNow;

        public DateTime GetDateTimeUtcNow()=> DateTime.Now;
    }
}
