using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts
{
    public interface IDateTimeProvider
    {
        DateTime GetDateTimeUtcNow();
        DateTime GetDateTimeNow();
    }
}
