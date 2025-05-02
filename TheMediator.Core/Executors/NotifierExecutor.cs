using System.Threading.Channels;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal class NotifierExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    NotifierRegistry notifierRegistry) : IDisposable
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull
    {
        var services = notifierRegistry.ListNotifiersByMessageType<TMessage>();
        if (services.Count == 0) return;

        var channel = Channel.CreateBounded<Func<Task>>(new BoundedChannelOptions(services.Count));

        var sender = Task.Run(() => SendNotificationsAsync(message, services, channel, cancellationToken),
            cancellationToken);
        var receiver = Task.Run(() => ExecuteHandlersAsync(channel, cancellationToken), cancellationToken);

        await Task.WhenAll(sender, receiver);
    }

    private async Task SendNotificationsAsync<TMessage>(
        TMessage message,
        IReadOnlyCollection<Models.ServiceDescriptor> services,
        Channel<Func<Task>> channel,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        logger.LogTrace("Publishing message {MessageType} to {NotificationCount} notifications",
            typeof(TMessage).Name, services.Count);

        try
        {
            foreach (var serviceDescriptor in services)
            {
                var handler =
                    (INotificationHandler<TMessage>)serviceProvider.GetRequiredService(serviceDescriptor.MainType);
                await channel.Writer.WriteAsync(() =>
                {
                    logger.LogTrace("Sending message {MessageType} to {HandlerType}",
                        typeof(TMessage).Name, serviceDescriptor.MainType.Name);

                    return handler.HandleAsync(message, cancellationToken);
                }, cancellationToken);
            }
        }
        finally
        {
            channel.Writer.Complete();
        }
    }

    private async Task ExecuteHandlersAsync(
        Channel<Func<Task>> channel,
        CancellationToken cancellationToken)
    {
        while (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (channel.Reader.TryRead(out var handlerTask))
            {
                if (notifierRegistry.Configurations.DeliveryMode == NotificationDeliveryMode.FireAndForget)
                    _ = Task.Run(handlerTask, cancellationToken).ConfigureAwait(false);
                else
                    await handlerTask();
            }
        }
    }

    public void Dispose()
        => loggerFactory.Dispose();
}