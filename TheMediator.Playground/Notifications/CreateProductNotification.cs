using TheMediator.Core;
using TheMediator.Playground.Contracts.Products;

namespace TheMediator.Playground.Notifications;

public class CreateProductNotification(ILogger<CreateProductNotification> logger)
    : INotificationHandler<ProductResponse>
{
    public async Task HandleAsync(ProductResponse notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("{Class} Product {ProductName} created", nameof(CreateProductNotification),
            notification.Name);
        await Task.Delay(1500, cancellationToken);
    }
}

public class UpdateProductNotification(
    ILogger<UpdateProductNotification> logger)
    : INotificationHandler<ProductResponse>
{
    public async Task HandleAsync(ProductResponse notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("{Class} Product {ProductName} created", nameof(UpdateProductNotification),
            notification.Name);

        await Task.Delay(2500, cancellationToken);
    }
}