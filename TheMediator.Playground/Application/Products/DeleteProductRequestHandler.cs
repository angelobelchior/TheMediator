using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class DeleteProductRequestHandler(IProductsRepository repository) 
    : IRequestHandler<ProductByIdRequest>
{
    public Task HandleAsync(ProductByIdRequest request, CancellationToken cancellationToken)
    {
        return repository.Delete(request.Id, cancellationToken);
    }
}