using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class UpdateProductHandler(IProductsRepository repository) : IHandler<ProductRequest>
{
    public Task HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        return repository.Update(request, cancellationToken);
    }
}