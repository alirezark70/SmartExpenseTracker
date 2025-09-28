using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Base
{
    public interface IResponseService
    {
        ApiResponse<T> CreateSuccessResponse<T>(T data, string message = "عملیات با موفقیت انجام شد");
        ApiResponse<T> CreateErrorResponse<T>(string message, ResponseStatus statusCode = ResponseStatus.BadRequest);
        PagedResponse<T> CreatePagedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords);
        ErrorResponse CreateValidationErrorResponse(Dictionary<string, string[]> errors);
    }
}
