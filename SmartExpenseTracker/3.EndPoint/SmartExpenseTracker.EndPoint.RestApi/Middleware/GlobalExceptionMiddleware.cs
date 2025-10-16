using SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Response;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using  validationEx = SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Response.ValidationException;

namespace SmartExpenseTracker.EndPoint.RestApi.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var response = exception switch
            {
                validationEx Ex => ErrorResponse.CreateValidation(Ex.Errors),
                UnauthorizedException _ => ErrorResponse.Create("احراز هویت الزامی است", ResponseStatus.Unauthorized),
                ForbiddenException _ => ErrorResponse.Create("دسترسی غیرمجاز", ResponseStatus.Forbidden),
                NotFoundException notFoundEx => ErrorResponse.Create(notFoundEx.Message, ResponseStatus.NotFound),
                ApiException apiEx => ErrorResponse.Create(apiEx.Message, apiEx.StatusCode),
                _ => ErrorResponse.Create("خطایی رخ داده است", ResponseStatus.InternalServerError)
            };

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
