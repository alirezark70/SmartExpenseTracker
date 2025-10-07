using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Base
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan CacheDuration { get; }
    }
}
