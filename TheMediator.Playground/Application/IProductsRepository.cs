using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Application;

public interface IProductsRepository
{
    Task<ProductResponse> Create(ProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse?> Update(ProductRequest request, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
    Task<ProductResponse?> GetById(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProductResponse>?> Search(string? filter, CancellationToken cancellationToken);
}