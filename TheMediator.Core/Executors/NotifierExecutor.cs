using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal class NotifierExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    NotifierRegistry notifierRegistry)
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull
    {
        var handlers = ResolveHandlers<TMessage>();
        if (handlers.Count == 0)
            return Task.CompletedTask;

        logger.LogTrace("Publishing message {MessageType} to {NotificationCount} notifications",
            typeof(TMessage).Name, handlers.Count);

        return notifierRegistry.Settings.DeliveryMode == NotificationDeliveryMode.WaitAll
            ? PublishAndWaitAsync(message, handlers, cancellationToken)
            : PublishFireAndForget(message, handlers, cancellationToken);
    }

    private IReadOnlyList<INotificationHandler<TMessage>> ResolveHandlers<TMessage>()
        where TMessage : notnull
        => notifierRegistry.ListNotifiersByMessageType<TMessage>()
            .Select(d => (INotificationHandler<TMessage>)serviceProvider.GetRequiredService(d.MainType))
            .ToList();

    private async Task PublishAndWaitAsync<TMessage>(
        TMessage message,
        IReadOnlyCollection<INotificationHandler<TMessage>> handlers,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        var tasks = handlers.Select(handler => HandleAsync(handler, message, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private Task PublishFireAndForget<TMessage>(
        TMessage message,
        IReadOnlyCollection<INotificationHandler<TMessage>> handlers,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        foreach (var handler in handlers)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleAsync(handler, message, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,
                        "Error publishing message {MessageType} to {HandlerType}",
                        typeof(TMessage).Name, handler.GetType().Name);
                }
            });
        }

        return Task.CompletedTask;
    }

    private async Task HandleAsync<TMessage>(
        INotificationHandler<TMessage> handler,
        TMessage message,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        logger.LogTrace("Sending message {MessageType} to {HandlerType}",
            typeof(TMessage).Name, handler.GetType().Name);

        await handler.HandleAsync(message, cancellationToken);
    }
}
