using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Exceptions
{
    /// <summary>
    /// Exception برای مواقعی که Concurrency Conflict رخ میده
    /// (وقتی دو نفر همزمان روی یک رکورد تغییر می‌دن)
    /// </summary>
    public sealed class ConcurrencyException : ApiException
    {
        public ConcurrencyException()
            : base("A concurrency conflict occurred. The entity has been modified by another user.")
        {
        }

        public ConcurrencyException(string message)
            : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException)
            : base(message)
        {
        }

        public ConcurrencyException(string entityName, object key)
            : base($"The entity '{entityName}' with key '{key}' has been modified by another user.")
        {
            EntityName = entityName;
            Key = key;
        }

        public string? EntityName { get; }
        public object? Key { get; }
    }
}
