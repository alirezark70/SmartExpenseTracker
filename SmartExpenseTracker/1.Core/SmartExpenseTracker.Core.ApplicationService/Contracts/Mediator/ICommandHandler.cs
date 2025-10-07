using MediatR;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator
{
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
    {
    }

    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    {
    }
}
