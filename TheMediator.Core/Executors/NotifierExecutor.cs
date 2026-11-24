using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal partial class NotifierExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    NotifierRegistry notifierRegistry)
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    [LoggerMessage(LogLevel.Trace, "Publishing message {MessageType} to {NotificationCount} notifications")]
    private static partial void LogPublishing(ILogger logger, string messageType, int notificationCount);

    [LoggerMessage(LogLevel.Trace, "Sending message {MessageType} to {HandlerType}")]
    private static partial void LogSending(ILogger logger, string messageType, string handlerType);

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull
    {
        var handlers = ResolveHandlers<TMessage>();
        if (handlers.Count == 0)
            return Task.CompletedTask;

        LogPublishing(logger, typeof(TMessage).Name, handlers.Count);

        return notifierRegistry.Settings.DeliveryMode == NotificationDeliveryMode.WaitAll
            ? PublishAndWaitAsync(message, handlers, cancellationToken)
            : PublishFireAndForget(message, handlers, cancellationToken);
    }

    private IReadOnlyList<INotificationHandler<TMessage>> ResolveHandlers<TMessage>()
        where TMessage : notnull
    {
        var descriptors = notifierRegistry.ListNotifiersByMessageType<TMessage>();
        if (descriptors.Count == 0)
            return Array.Empty<INotificationHandler<TMessage>>();

        var handlers = new INotificationHandler<TMessage>[descriptors.Count];
        for (var i = 0; i < descriptors.Count; i++)
            handlers[i] = (INotificationHandler<TMessage>)serviceProvider.GetRequiredService(descriptors[i].MainType);

        return handlers;
    }

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
        LogSending(logger, typeof(TMessage).Name, handler.GetType().Name);

        await handler.HandleAsync(message, cancellationToken);
    }
}
