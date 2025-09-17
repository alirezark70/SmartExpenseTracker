using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Configuration
{
    public class TimeoutOptions
    {
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
