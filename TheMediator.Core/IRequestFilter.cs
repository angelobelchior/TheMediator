namespace TheMediator.Core;

public interface IRequestFilter
{
    Task FilterAsync<TRequest>(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        where TRequest : notnull;
}