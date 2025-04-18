using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class UpdateProductRequestHandler(IProductsRepository repository) : IRequestHandler<ProductRequest>
{
    public Task HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        return repository.Update(request, cancellationToken);
    }
}