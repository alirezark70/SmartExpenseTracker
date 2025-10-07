using MediatR;
using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;
        private readonly IPostInquiryService _postInquiryService;

        public LoggingBehavior(ILogger<TRequest> logger, IPostInquiryService postInquiryService)
        {
            _logger = logger;
            _postInquiryService = postInquiryService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            var requestServiceId = _postInquiryService.Id;

            _logger.LogInformation(
            "Handling {RequestName} for service Id {requestServiceId}  with Request: {@Request}",
            requestName, requestServiceId, request);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();

                // Log Response (be careful with sensitive data)
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms for User {UserId})",
                    requestName, stopwatch.ElapsedMilliseconds, requestServiceId);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "Request {RequestName} failed after {ElapsedMilliseconds}ms for User {UserId})",
                    requestName, stopwatch.ElapsedMilliseconds, requestServiceId);

                throw;
            }

        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation("درخواست {Name} با داده {@Request} شروع شد",
                requestName, request);

            return Task.CompletedTask;
        }
    }
}
