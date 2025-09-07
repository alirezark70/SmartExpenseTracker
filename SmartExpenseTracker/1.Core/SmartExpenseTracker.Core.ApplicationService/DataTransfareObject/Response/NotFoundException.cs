using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Response
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string entityName, object key)
        : base($"{entityName} با شناسه {key} یافت نشد", ResponseStatus.NotFound)
        {
        }
    }
}
