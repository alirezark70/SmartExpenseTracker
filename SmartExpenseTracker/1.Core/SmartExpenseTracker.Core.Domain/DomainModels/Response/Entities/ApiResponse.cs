using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities
{
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Success(string message = "عملیات با موفقیت انجام شد")
        {
            return new ApiResponse
            {
                IsSuccess = true,
                StatusCode = ResponseStatus.Success,
                Message = message
            };
        }

        public static ApiResponse NoContent()
        {
            return new ApiResponse
            {
                IsSuccess = true,
                StatusCode = ResponseStatus.NoContent,
                Message = "بدون محتوا"
            };
        }
    }
    public class ApiResponse<T> : BaseResponse
    {
        public T? Data { get; set; }
        public Dictionary<string, object>? Meta { get; set; }

        public ApiResponse()
        {
            Meta = new Dictionary<string, object>();
        }

        public static ApiResponse<T> Success(T data, string message = "عملیات با موفقیت انجام شد")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = ResponseStatus.Success,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Created(T data, string message = "با موفقیت ایجاد شد")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = ResponseStatus.Created,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Failure(string message, ResponseStatus statusCode = ResponseStatus.BadRequest)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}

