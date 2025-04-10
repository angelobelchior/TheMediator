using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class SearchProductHandler(IProductsRepository repository)
    : IHandler<string?, IReadOnlyCollection<ProductResponse>?>
{
    public Task<IReadOnlyCollection<ProductResponse>?> HandleAsync(
        string? query,
        CancellationToken cancellationToken)
    {
        return repository.Search(query, cancellationToken);
    }
}