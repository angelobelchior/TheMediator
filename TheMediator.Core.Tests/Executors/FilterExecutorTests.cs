using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TheMediator.Core;
using TheMediator.Core.Executors;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Tests.Executors;

public class FilterExecutorTests
{
    [Fact]
    public async Task Execute_ShouldRunFiltersInRegistrationOrderAroundHandler()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        var registry = new FilterRegistry();
        registry.Add<FirstFilter>();
        registry.Add<SecondFilter>();
        services.AddScoped<FirstFilter>();
        services.AddScoped<SecondFilter>();
        var serviceProvider = services.BuildServiceProvider();

        var executor = new FilterExecutor(NullLoggerFactory.Instance, serviceProvider, registry);

        var response = await executor.Execute<SampleRequest, string>(
            new TestResponseHandler(log),
            new SampleRequest(),
            CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal(
            ["first:before", "second:before", "handler", "second:after", "first:after"],
            log);
    }

    [Fact]
    public async Task Execute_WithoutResponse_ShouldRunFiltersAroundVoidHandler()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        var registry = new FilterRegistry();
        registry.Add<FirstFilter>();
        registry.Add<SecondFilter>();
        services.AddScoped<FirstFilter>();
        services.AddScoped<SecondFilter>();
        var serviceProvider = services.BuildServiceProvider();

        var executor = new FilterExecutor(NullLoggerFactory.Instance, serviceProvider, registry);

        await executor.Execute<SampleRequest>(
            new TestVoidHandler(log),
            new SampleRequest(),
            CancellationToken.None);

        Assert.Equal(
            ["first:before", "second:before", "handler", "second:after", "first:after"],
            log);
    }

    private sealed class TestResponseHandler(List<string> log) : IRequestHandler<SampleRequest, string>
    {
        public Task<string> HandleAsync(SampleRequest request, CancellationToken cancellationToken)
        {
            log.Add("handler");
            return Task.FromResult("ok");
        }
    }

    private sealed class TestVoidHandler(List<string> log) : IRequestHandler<SampleRequest>
    {
        public Task HandleAsync(SampleRequest request, CancellationToken cancellationToken)
        {
            log.Add("handler");
            return Task.CompletedTask;
        }
    }

    private sealed class FirstFilter(List<string> log) : IRequestFilter
    {
        public async Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request,
            Func<Task<TResponse>> next, CancellationToken cancellationToken)
            where TRequest : notnull
        {
            log.Add("first:before");
            var response = await next();
            log.Add("first:after");
            return response;
        }
    }

    private sealed class SecondFilter(List<string> log) : IRequestFilter
    {
        public async Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request,
            Func<Task<TResponse>> next, CancellationToken cancellationToken)
            where TRequest : notnull
        {
            log.Add("second:before");
            var response = await next();
            log.Add("second:after");
            return response;
        }
    }
}
