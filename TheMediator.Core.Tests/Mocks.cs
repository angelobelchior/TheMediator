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
    public Task FilterAsync<TRequest>(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        where TRequest : notnull
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
    public Task FilterAsync<TRequest>(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        where TRequest : notnull => Task.CompletedTask;
}

public class UnrelatedClass
{
}

public class InvalidType;

public class SampleRequest;

public class SampleResponse;