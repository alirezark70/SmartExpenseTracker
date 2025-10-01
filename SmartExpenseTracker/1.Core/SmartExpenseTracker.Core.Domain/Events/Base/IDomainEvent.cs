using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Events.Base
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }

    }
}
