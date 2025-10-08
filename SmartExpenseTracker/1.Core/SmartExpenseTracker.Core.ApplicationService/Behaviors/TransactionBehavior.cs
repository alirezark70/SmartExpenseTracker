using MediatR;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Skip transaction for queries
            if (request is IQuery<TResponse>)
                return await next();

            if (_unitOfWork.HasActiveTransaction)
                return await next();


            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
