using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;

namespace SmartExpenseTracker.EndPoint.RestApi.Filters
{
    public class ApiResponseActionFilter : ActionFilterAttribute
    {
        


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                var resultType = objectResult.Value.GetType();

                // Check if already wrapped
                if (resultType.IsGenericType &&
                    (resultType.GetGenericTypeDefinition() == typeof(ApiResponse<>) ||
                     resultType.GetGenericTypeDefinition() == typeof(PagedResponse<>)))
                {
                    return;
                }

                // Check for NoContent
                if (objectResult.StatusCode == 204)
                {
                    context.Result = new ObjectResult(ApiResponse.NoContent())
                    {
                        StatusCode = 200
                    };
                    return;
                }

                // Wrap the response
                var apiResponseType = typeof(ApiResponse<>).MakeGenericType(resultType);
                var successMethod = apiResponseType.GetMethod("Success", new[] { resultType, typeof(string) });
                var wrappedResponse = successMethod!.Invoke(null, new[] { objectResult.Value, "عملیات با موفقیت انجام شد" });

                context.Result = new ObjectResult(wrappedResponse)
                {
                    StatusCode = objectResult.StatusCode ?? 200
                };
            }
            else if (context.Result is EmptyResult || (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode == 204))
            {
                context.Result = new ObjectResult(ApiResponse.NoContent())
                {
                    StatusCode = 200
                };
            }

            base.OnActionExecuted(context);
        }
    }
}
