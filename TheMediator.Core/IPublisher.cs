using TheMediator.Core.Executors;

namespace TheMediator.Core;

/// <summary>
/// Publishes messages to the registered subscribers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a message to the registered subscribers.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="TMessage">Message Type</typeparam>
    /// <returns></returns>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull;
}

internal class Publisher(NotifierExecutor notifierExecutor) : IPublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull
        => notifierExecutor.PublishAsync(message, cancellationToken);
}