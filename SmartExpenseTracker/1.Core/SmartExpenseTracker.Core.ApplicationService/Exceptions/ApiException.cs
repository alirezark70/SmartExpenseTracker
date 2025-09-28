using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Exceptions
{
    public class ApiException : Exception
    {
        public ResponseStatus StatusCode { get; }
        public object? Details { get; }

        public ApiException(string message, ResponseStatus statusCode = ResponseStatus.BadRequest, object? details = null)
            : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }
}
