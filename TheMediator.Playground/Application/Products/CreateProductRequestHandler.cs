using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class CreateProductRequestHandler(IProductsRepository repository)
    : IRequestHandler<ProductRequest, ProductResponse>
{
    public Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        return repository.Create(request, cancellationToken);
    }
}