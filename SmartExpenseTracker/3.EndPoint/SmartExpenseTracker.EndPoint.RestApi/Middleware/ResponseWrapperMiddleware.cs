using Microsoft.AspNetCore.Http.Features;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using System.Net;

namespace SmartExpenseTracker.EndPoint.RestApi.Middleware
{
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseWrapperMiddleware> _logger;

        public ResponseWrapperMiddleware(RequestDelegate next, ILogger<ResponseWrapperMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip wrapping for non-API endpoints
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                if (context.Request.Path.StartsWithSegments("/metrics"))
                {
                    var syncIOFeature = context.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                    }
                    await _next(context);
                }

                else { await _next(context); }
                return;
            }

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                if (context.Response.StatusCode == (int)HttpStatusCode.OK && context.Response.ContentType?.Contains("application/json") == true)
                {
                    var body = await FormatResponse(context.Response);
                    await WriteFormattedResponse(context.Response, originalBodyStream, body);
                }
                else
                {
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in ResponseWrapperMiddleware");
                await HandleExceptionAsync(context, ex, originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }

        private async Task WriteFormattedResponse(HttpResponse response, Stream originalBodyStream, string body)
        {
            // Implementation depends on your specific needs
            // You can wrap the response here if needed
            var buffer = System.Text.Encoding.UTF8.GetBytes(body);
            await originalBodyStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, Stream originalBodyStream)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ErrorResponse("خطای سرور داخلی", ResponseStatus.InternalServerError);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);

            await originalBodyStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
