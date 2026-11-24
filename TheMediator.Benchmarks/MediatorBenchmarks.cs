using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TheMediator.Core;
using TheMediator.Core.DependencyInjection;
using TheMediator.Core.Models;

namespace TheMediator.Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private readonly PingHandler _handler = new();
    private ServiceProvider _provider = null!;
    private ISender _sender = null!;
    private IPublisher _publisher = null!;
    private Ping _ping = new("hello");

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        services.AddTheMediator(configuration =>
        {
            configuration.Handlers.Add<PingHandler, Ping, Pong>();
            configuration.Handlers.Add<FilteredPingHandler, FilteredPing, Pong>();
            configuration.Notifiers.Add<PingedHandler, Pinged>();
            configuration.Filters.Add<BenchmarkFilter, FilteredPing>();
            configuration.Notifiers.Settings.DeliveryMode = NotificationDeliveryMode.WaitAll;
        });

        _provider = services.BuildServiceProvider();
        _sender = _provider.GetRequiredService<ISender>();
        _publisher = _provider.GetRequiredService<IPublisher>();
    }

    [GlobalCleanup]
    public void Cleanup() => _provider.Dispose();

    [Benchmark(Baseline = true)]
    public Task<Pong> DirectCall() => _handler.HandleAsync(_ping, CancellationToken.None);

    [Benchmark]
    public Task<Pong> Send() => _sender.SendAsync<Ping, Pong>(_ping, CancellationToken.None);

    [Benchmark]
    public Task<Pong> SendWithFilter() =>
        _sender.SendAsync<FilteredPing, Pong>(new FilteredPing(), CancellationToken.None);

    [Benchmark]
    public Task Publish() => _publisher.PublishAsync(new Pinged("done"), CancellationToken.None);
}
