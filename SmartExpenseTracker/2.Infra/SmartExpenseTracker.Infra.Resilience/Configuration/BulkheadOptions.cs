using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Configuration
{
    public class BulkheadOptions
    {
        public int MaxParallelization { get; set; } = 10;
        public int MaxQueuingActions { get; set; } = 20;
    }
}
