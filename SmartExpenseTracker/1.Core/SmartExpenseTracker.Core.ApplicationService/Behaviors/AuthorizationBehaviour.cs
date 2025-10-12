using MediatR;
using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.ApplicationService.Common.Security;
using SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Response;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Behaviors
{
    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthorizationBehaviour<TRequest, TResponse>> _logger;


        public AuthorizationBehaviour(ICurrentUserService currentUserService, ILogger<AuthorizationBehaviour<TRequest, TResponse>> logger)
        {
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();

            if (!authorizeAttributes.Any())
            {
                return await next();
            }

            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt for {RequestType}", typeof(TRequest).Name);

                throw new UnauthorizedAccessException();
            }

            foreach (var attribute in authorizeAttributes)
            {
                var authorized = await IsAuthorizedAsync(attribute);

                if (authorized)
                {
                    _logger.LogWarning(
                    "User {UserId} failed authorization for {RequestType}",
                    _currentUserService.UserId,
                    typeof(TRequest).Name);

                    throw new ForbiddenException();
                }
            }

            _logger.LogInformation(
            "User {UserId} authorized for {RequestType}",
             _currentUserService.UserId,
            typeof(TRequest).Name);

            return await next();
        }
        private Task<bool> IsAuthorizedAsync(AuthorizeAttribute attribute)
        {
            var isAuthorized = true;

            //check roles
            if (!string.IsNullOrWhiteSpace(attribute.Roles))
            {
                var roles = attribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim());

                isAuthorized = roles.Any(role => _currentUserService.IsInRole(role));
            }

            //check permissions

            if (isAuthorized && !string.IsNullOrWhiteSpace(attribute.Roles))
            {
                var permissions = attribute?.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim());

                isAuthorized = permissions!= null? permissions.Any(persmission => _currentUserService.HasPermission(persmission)):false;
            }
            // Check permissions
            if (isAuthorized && !string.IsNullOrWhiteSpace(attribute?.Permissions))
            {
                var permissions = attribute.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim());

                isAuthorized = permissions.Any(permission => _currentUserService.HasPermission(permission));
            }

            return Task.FromResult(isAuthorized);
        }
    }

}
