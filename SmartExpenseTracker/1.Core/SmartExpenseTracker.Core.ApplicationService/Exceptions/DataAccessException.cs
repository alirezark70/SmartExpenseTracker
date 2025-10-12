using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Exceptions
{
    /// <summary>
    /// Exception برای مشکلات مرتبط با دیتابیس و data access
    /// </summary>
    public sealed class DataAccessException : ApiException
    {
        public DataAccessException()
            : base("A data access error occurred.")
        {
        }

        public DataAccessException(string message)
            : base(message)
        {
        }

        public DataAccessException(string message, Exception innerException)
            : base(message)
        {
           
        }

        public int? SqlErrorNumber { get; }
        public string? SqlErrorMessage { get; }
    }
}
