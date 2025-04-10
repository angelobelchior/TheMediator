using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class DeleteProductHandler(IProductsRepository repository) 
    : IHandler<ProductByIdRequest>
{
    public Task HandleAsync(ProductByIdRequest request, CancellationToken cancellationToken)
    {
        return repository.Delete(request.Id, cancellationToken);
    }
}