using TheMediator.Playground.Application;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Infrastructure;

public class ProductsRepository : IProductsRepository
{
    private static readonly List<ProductResponse> _products =
    [
        new(Guid.NewGuid(), "Playstation 5", 10.0m),
        new(Guid.NewGuid(), "xbox", 20.0m),
        new(Guid.NewGuid(), "Nintendo Switch", 30.0m)
    ];

    public Task<ProductResponse> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var response = new ProductResponse(Id: Guid.NewGuid(), Name: request.Name, Price: request.Price);
        _products.Add(response);
        return Task.FromResult(response);
    }

    public Task<ProductResponse?> Update(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = _products.FirstOrDefault(p => p.Id == request.Id);
        if (product == null) return Task.FromResult<ProductResponse?>(null);
        _products.Remove(product);
        var response = new ProductResponse(Id: request.Id.GetValueOrDefault(), Name: request.Name,
            Price: request.Price);
        _products.Add(response);

        return Task.FromResult<ProductResponse?>(response);
    }

    public Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
            _products.Remove(product);
        return Task.CompletedTask;
    }

    public Task<ProductResponse?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<ProductResponse>?> Search(string? filter, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return Task.FromResult<IReadOnlyCollection<ProductResponse>?>(_products);

        var products = _products
            .Where(p => p.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<ProductResponse>?>(products);
    }
}