using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Response
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "شما اجازه دسترسی به این منبع را ندارید")
        : base(message, ResponseStatus.Forbidden)
        {
        }
    }
}
