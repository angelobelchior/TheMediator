using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application.Products;

public class CreateProductRequestHandler(IPublisher publisher, IProductsRepository repository)
    : IRequestHandler<ProductRequest, ProductResponse>
{
    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var created = await repository.Create(request, cancellationToken);
        await publisher.PublishAsync(created, cancellationToken);
        return created;
    }
}