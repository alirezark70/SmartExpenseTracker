using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Exceptions
{
    public class ValidationException : ApiException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("یک یا چند خطای اعتبارسنجی رخ داده است", ResponseStatus.UnprocessableEntity)
        {
            Errors = errors;
        }

       
    }
}
