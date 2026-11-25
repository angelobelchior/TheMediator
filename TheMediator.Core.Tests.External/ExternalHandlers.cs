using TheMediator.Core;

namespace TheMediator.Core.Tests.External;

public sealed record ExternalPing(string Message);

public sealed record ExternalPong(string Message);

public sealed record ExternalPinged(string Message);

public sealed class ExternalPingHandler : IRequestHandler<ExternalPing, ExternalPong>
{
    public Task<ExternalPong> HandleAsync(ExternalPing request, CancellationToken cancellationToken)
        => Task.FromResult(new ExternalPong($"external-pong:{request.Message}"));
}

public sealed class ExternalPingedNotification(List<string> log) : INotificationHandler<ExternalPinged>
{
    public Task HandleAsync(ExternalPinged notification, CancellationToken cancellationToken)
    {
        log.Add($"external-notified:{notification.Message}");
        return Task.CompletedTask;
    }
}

public sealed class ExternalMarker
{
}
