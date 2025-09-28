using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Services.Response
{
    public class ResponseService : IResponseService
    {
        public ApiResponse<T> CreateSuccessResponse<T>(T data, string message = "عملیات با موفقیت انجام شد")
        {
            return ApiResponse<T>.Success(data, message);
        }

        public ApiResponse<T> CreateErrorResponse<T>(string message, ResponseStatus statusCode = ResponseStatus.BadRequest)
        {
            return ApiResponse<T>.Failure(message, statusCode);
        }

        public PagedResponse<T> CreatePagedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            return new PagedResponse<T>(data, pageNumber, pageSize, totalRecords);
        }

        public ErrorResponse CreateValidationErrorResponse(Dictionary<string, string[]> errors)
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
