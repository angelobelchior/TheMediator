using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Filters;

public class SpecificFilter(ILogger<SpecificFilter> logger) : IRequestFilter<ProductRequest>
{
    public Task<TResponse> FilterAsync<TResponse>(ProductRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("Specific filter applied to {Type} with Id: {Id}", request.GetType().Name, request.Id);
        
        return next();
    }
}