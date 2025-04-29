namespace TheMediator.Core.Tests;

public class RequestResponseRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    public Task<SampleResponse> HandleAsync(SampleRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
    
public class RequestRequestHandler : IRequestHandler<SampleRequest>
{
    public Task HandleAsync(SampleRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class SampleRequestFilter : IRequestFilter
{
    public Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken) where TRequest : notnull
    {
        throw new NotImplementedException();
    }
}

public class GenericHandler : IRequestHandler<string, int>
{
    public Task<int> HandleAsync(string request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class NonGenericHandler : IRequestFilter
{
    public Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken) where TRequest : notnull
    {
        throw new NotImplementedException();
    }
}

public class SampleNotification : INotificationHandler<SampleMessage>
{
    public Task HandleAsync(SampleMessage notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class UnrelatedClass;

public class InvalidType;

public class SampleMessage;
public class SampleRequest;

public class SampleResponse;