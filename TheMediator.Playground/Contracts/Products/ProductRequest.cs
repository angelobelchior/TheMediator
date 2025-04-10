namespace TheMediator.Playground.Contracts.Products;

public record ProductRequest(string Name, decimal Price, Guid? Id = null);