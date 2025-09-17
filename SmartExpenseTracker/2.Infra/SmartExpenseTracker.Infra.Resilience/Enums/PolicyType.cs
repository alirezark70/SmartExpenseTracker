using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Enums
{
    public enum PolicyType
    {
        HttpStandard,
        Database,
        CriticalService,
        ExternalService
    }
}
