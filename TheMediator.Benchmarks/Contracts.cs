using TheMediator.Core;

namespace TheMediator.Benchmarks;

public sealed record Ping(string Message);

public sealed record Pong(string Message);

public sealed record Pinged(string Message);

public sealed record FilteredPing;

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> HandleAsync(Ping request, CancellationToken cancellationToken)
        => Task.FromResult(new Pong($"pong:{request.Message}"));
}

public sealed class FilteredPingHandler : IRequestHandler<FilteredPing, Pong>
{
    public Task<Pong> HandleAsync(FilteredPing request, CancellationToken cancellationToken)
        => Task.FromResult(new Pong("filtered"));
}

public sealed class PingedHandler : INotificationHandler<Pinged>
{
    public Task HandleAsync(Pinged notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class BenchmarkFilter : IRequestFilter<FilteredPing>
{
    public Task<TResponse> FilterAsync<TResponse>(FilteredPing request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        => next();
}
