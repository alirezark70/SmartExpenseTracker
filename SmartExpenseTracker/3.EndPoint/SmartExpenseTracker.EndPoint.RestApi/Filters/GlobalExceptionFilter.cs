using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using System.ComponentModel.DataAnnotations;
using System.Net;
using validaationEx = SmartExpenseTracker.Core.ApplicationService.Exceptions.ValidationException;

namespace SmartExpenseTracker.EndPoint.RestApi.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (context.Exception)
            {
                case validaationEx validationException:
                    errorResponse.StatusCode = ResponseStatus.UnprocessableEntity;
                    errorResponse.Message = validationException.Message;
                    foreach (var error in validationException.Errors)
                    {
                        foreach (var message in error.Value)
                        {
                            errorResponse.AddError(error.Key, message);
                        }
                    }
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    break;

                case NotFoundException notFoundException:
                    errorResponse.StatusCode = ResponseStatus.NotFound;
                    errorResponse.Message = notFoundException.Message;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedException unauthorizedException:
                    errorResponse.StatusCode = ResponseStatus.Unauthorized;
                    errorResponse.Message = unauthorizedException.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ForbiddenException forbiddenException:
                    errorResponse.StatusCode = ResponseStatus.Forbidden;
                    errorResponse.Message = forbiddenException.Message;
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;

                case ApiException apiException:
                    errorResponse.StatusCode = apiException.StatusCode;
                    errorResponse.Message = apiException.Message;
                    response.StatusCode = (int)apiException.StatusCode;
                    if (apiException.Details != null)
                    {
                        errorResponse.AddError("details", apiException.Details.ToString() ?? string.Empty);
                    }
                    break;

                default:
                    _logger.LogError(context.Exception, "An unhandled exception occurred");
                    errorResponse.StatusCode = ResponseStatus.InternalServerError;
                    errorResponse.Message = _environment.IsDevelopment()
                        ? context.Exception.Message
                        : "خطای سرور داخلی رخ داده است";

                    if (_environment.IsDevelopment())
                    {
                        errorResponse.AddError("stackTrace", context.Exception.StackTrace ?? string.Empty);
                    }

                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Result = new ObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
    }
}

