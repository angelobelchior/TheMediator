using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class GetByIdProductRequestHandler(IProductsRepository repository) : IRequestHandler<ProductByIdQuery, ProductResponse?>
{
    public Task<ProductResponse?> HandleAsync(ProductByIdQuery request, CancellationToken cancellationToken)
    {
        return repository.GetById(request.Id, cancellationToken);
    }
}