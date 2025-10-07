using MediatR;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    {
    }


}
