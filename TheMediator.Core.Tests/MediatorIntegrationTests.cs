using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TheMediator.Core;
using TheMediator.Core.DependencyInjection;
using TheMediator.Core.Models;
using TheMediator.Core.Tests.External;
using TheMediator.Generated;

namespace TheMediator.Core.Tests;

public class MediatorIntegrationTests
{
    [Fact]
    public async Task SendAndPublish_ShouldWork_UsingSourceGeneratedRegistration()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(log);

        services.AddTheMediator(configuration =>
        {
            configuration.RegisterFromAssembly<Ping>();
            configuration.Filters.Add<IntegrationFilter>();
            configuration.Notifiers.Settings.DeliveryMode = NotificationDeliveryMode.WaitAll;
        });

        await using var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        var response = await sender.SendAsync<Ping, Pong>(new Ping("hello"), CancellationToken.None);

        Assert.Equal("pong:hello", response.Message);

        var publisher = provider.GetRequiredService<IPublisher>();
        await publisher.PublishAsync(new Pinged("done"), CancellationToken.None);

        Assert.Equal(
            ["filter:before", "filter:after", "notified:done"],
            log);
    }

    [Fact]
    public async Task SendAndPublish_ShouldWork_UsingCrossAssemblyRegistration()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(log);

        services.AddTheMediator(configuration =>
        {
            configuration.RegisterFromAssembly<ExternalMarker>();
        });

        await using var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        var response =
            await sender.SendAsync<ExternalPing, ExternalPong>(new ExternalPing("hello"), CancellationToken.None);

        Assert.Equal("external-pong:hello", response.Message);

        var publisher = provider.GetRequiredService<IPublisher>();
        await publisher.PublishAsync(new ExternalPinged("done"), CancellationToken.None);

        Assert.Equal(["external-notified:done"], log);
    }
}

public sealed record Ping(string Message);

public sealed record Pong(string Message);

public sealed record Pinged(string Message);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> HandleAsync(Ping request, CancellationToken cancellationToken)
        => Task.FromResult(new Pong($"pong:{request.Message}"));
}

public sealed class PingNotificationHandler(List<string> log) : INotificationHandler<Pinged>
{
    public Task HandleAsync(Pinged notification, CancellationToken cancellationToken)
    {
        log.Add($"notified:{notification.Message}");
        return Task.CompletedTask;
    }
}

public sealed class IntegrationFilter(List<string> log) : IRequestFilter
{
    public async Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request,
        Func<Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        log.Add("filter:before");
        var response = await next();
        log.Add("filter:after");
        return response;
    }
}
