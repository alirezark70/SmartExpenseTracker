using MediatR;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }


}
