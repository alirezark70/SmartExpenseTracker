using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "دسترسی غیرمجاز")
        : base(message, ResponseStatus.Unauthorized)
        {
        }
    }
}
