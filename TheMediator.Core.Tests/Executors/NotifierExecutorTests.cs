using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TheMediator.Core.Executors;
using TheMediator.Core.Models;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Tests.Executors;

public class NotifierExecutorTests
{
    [Fact]
    public async Task PublishAsync_WithWaitAll_ShouldAwaitAllHandlers()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        var registry = new NotifierRegistry();
        registry.Add<RecordingNotificationHandler, SampleMessage>();
        services.AddScoped<RecordingNotificationHandler>();
        registry.Settings.DeliveryMode = NotificationDeliveryMode.WaitAll;
        var serviceProvider = services.BuildServiceProvider();

        var executor = new NotifierExecutor(NullLoggerFactory.Instance, serviceProvider, registry);

        // Act
        await executor.PublishAsync(new SampleMessage(), CancellationToken.None);

        // Assert
        Assert.Equal(["handled"], log);
    }

    [Fact]
    public async Task PublishAsync_WithFireAndForget_ShouldReturnBeforeHandlersComplete()
    {
        // Arrange
        var state = new FireAndForgetState();
        var services = new ServiceCollection();
        services.AddSingleton(state);
        var registry = new NotifierRegistry();
        registry.Add<BlockingNotificationHandler, SampleMessage>();
        services.AddScoped<BlockingNotificationHandler>();
        registry.Settings.DeliveryMode = NotificationDeliveryMode.FireAndForget;
        var serviceProvider = services.BuildServiceProvider();

        var executor = new NotifierExecutor(NullLoggerFactory.Instance, serviceProvider, registry);

        // Act
        await executor.PublishAsync(new SampleMessage(), CancellationToken.None);

        // Assert
        await state.Started.Task;
        Assert.Equal(["started"], state.Log);

        state.Release.TrySetResult(true);
    }

    private sealed class FireAndForgetState
    {
        public List<string> Log { get; } = [];

        public TaskCompletionSource<bool> Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class RecordingNotificationHandler(List<string> log) : INotificationHandler<SampleMessage>
    {
        public Task HandleAsync(SampleMessage notification, CancellationToken cancellationToken)
        {
            log.Add("handled");
            return Task.CompletedTask;
        }
    }

    private sealed class BlockingNotificationHandler(FireAndForgetState state)
        : INotificationHandler<SampleMessage>
    {
        public async Task HandleAsync(SampleMessage notification, CancellationToken cancellationToken)
        {
            state.Log.Add("started");
            state.Started.TrySetResult(true);
            await state.Release.Task;
            state.Log.Add("completed");
        }
    }
}
