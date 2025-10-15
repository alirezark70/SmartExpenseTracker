using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities
{
    public sealed partial class ErrorResponse : BaseResponse
    {
        public List<ErrorDetail> Errors { get; set; }

        public ErrorResponse()
        {
            Errors = new List<ErrorDetail>();
            IsSuccess = false;
        }

        public ErrorResponse(string message, ResponseStatus statusCode = ResponseStatus.BadRequest) : this()
        {
            Message = message;
            StatusCode = statusCode;
        }

        public void AddError(string field, string message)
        {
            Errors.Add(new ErrorDetail { Field = field, Message = message });
        }
    }

    public sealed partial class ErrorResponse
    {
        public static ErrorResponse Create(string message, ResponseStatus statusCode = ResponseStatus.BadRequest)
        {
            return new ErrorResponse(message, statusCode);
        }

        public static ErrorResponse CreateValidation(Dictionary<string, string[]> errors)
        {
            var response = new ErrorResponse("خطای اعتبارسنجی", ResponseStatus.UnprocessableEntity);

            foreach (var error in errors)
            {
                foreach (var message in error.Value)
                {
                    response.AddError(error.Key, message);
                }
            }

            return response;
        }
    }
}
