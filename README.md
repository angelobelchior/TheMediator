# TheMediator

<img src="./TheMediator.Core/icon.png" alt="drawing" style="width:200px;"/>

[![.NET](https://github.com/angelobelchior/TheMediator/actions/workflows/dotnet-cicd-publish.yml/badge.svg)](https://github.com/angelobelchior/TheMediator/actions/workflows/dotnet-cicd-publish.yml)

[![NuGet](https://img.shields.io/nuget/v/TheMediator.svg)](https://www.nuget.org/packages/TheMediator)

**TheMediator** é uma implementação de alto desempenho do padrão Mediator para .NET, com suporte a **Filters** e **Notifications**. 
É leve, extensível e rápido, ideal para desacoplar componentes em sistemas.

Esse projeto faz parte da saga "Reinventando a Roda: Criando seu próprio MediatR", onde eu crio uma implementação própria.

Foram 3 posts no total:

- https://dev.to/angelobelchior/reinventando-a-roda-criando-seu-proprio-mediatr-parte-1-3f1o
- https://dev.to/angelobelchior/reinventando-a-roda-criando-seu-proprio-mediatr-parte-2-1c8e
- https://dev.to/angelobelchior/reinventando-a-roda-criando-seu-proprio-mediatr-parte-3-3gp1

## Instalação

Você pode instalar o pacote via NuGet usando o seguinte comando:

```bash
dotnet add package TheMediator --version 1.0.1
```

Feito isso, você pode registrar o Mediator no seu projeto:

```csharp
builder.Services.AddTheMediator(configuration =>
{
    configuration.AddServicesFromAssemblies(typeof(Program).Assembly);
    
    configuration.Filters.Add<LoggingFilter>();
    
    configuration.Notifiers.Configurations.DeliveryMode = NotificationDeliveryMode.FireAndForget;
}); 
```

# Funcionalidades

## Handlers
Os `Handlers` são responsáveis por processar as requisições. 

Eles implementam a interface `IRequestHandler<TRequest, TResponse>`, onde `TRequest` é o tipo da requisição e `TResponse` é o tipo da resposta.

`TResponse` não é obrigatório, e o `TRequest` pode ser qualquer tipo, incluindo `string`, `int`, `Guid`, etc.

É possível ter mais de um `Handler` para o mesmo tipo de requisição (`TRequest`) contanto que o `TResponse` seja diferente.

Os `Handlers` são registrados automaticamente pelo método `AddServicesFromAssemblies` no `AddTheMediator`.

Exemplo de uso:

```csharp
public record ProductRequest(string Name, decimal Price);

public record ProductResponse(Guid Id, string Name, decimal Price);

public class CreateProductRequestHandler(IProductsRepository repository)
    : IRequestHandler<ProductRequest, ProductResponse>
{
    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var created = await repository.Create(request, cancellationToken);
        return created;
    }
}

public class SearchProductRequestHandler(IProductsRepository repository)
    : IRequestHandler<string, IReadOnlyCollection<ProductResponse>?>
{
    public Task<IReadOnlyCollection<ProductResponse>?> HandleAsync(
        string? query,
        CancellationToken cancellationToken)
    {
        return repository.Search(query, cancellationToken);
    }
}

// Exemplo de uso

app.MapPost("/products",
        async (
            [FromServices] ISender sender,
            [FromBody] ProductRequest request,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.SendAsync<ProductRequest, ProductResponse?>(request, cancellationToken);
            return response is null
                ? Results.BadRequest("Cannot create product")
                : Results.Created($"/products/{response.Id}", response);
        })
    .WithName("Products.Post.ProductRequest");
```

## Filters

Os Filters permitem adicionar lógica antes ou depois da execução de um handler, como validações, logging ou manipulação de exceções.

Exemplo de uso:


```csharp
public class LoggerRequestFilter(ILogger<LoggerRequestFilter> logger)
    : IRequestFilter
{
    public Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        logger.LogInformation("{Type} execution started: {Time}", request.GetType().Name, DateTime.Now);
        var response = next();
        logger.LogInformation("{Type} execution finished: {Time}", request.GetType().Name, DateTime.Now);
        return response;
    }
}
```
Os `Filters` devem ser registrados manualmente no `AddTheMediator` e a ordem de execução é a ordem em que foram registrados.


## Notifications

As `Notifications` permitem enviar mensagens para múltiplos `handlers` de forma assíncrona (quando o delivery mode é `FireAndForget`) ou síncrona (quando o delivery mode é `WaitForAll`).

O DeliveryMode padrão é `FireAndForget`, e ele pode ser configurado no `AddTheMediator`.

As `Notifications` são registrados automaticamente pelo método `AddServicesFromAssemblies` no `AddTheMediator`.

Exemplo de uso:

```csharp
public class LogInfoProductCreatedNotification(ILogger<LogInfoProductCreatedNotification> logger)
    : INotificationHandler<ProductResponse>
{
    public async Task HandleAsync(ProductResponse notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("{Class} Product {ProductName} created", nameof(CreateProductNotification),
            notification.Name);
    }
}

public class SendEmailProductCreatedNotification(ILogger<SendEmailProductCreatedNotification> logger, IEmailService emailService)
    : INotificationHandler<ProductResponse>
{
    public async Task HandleAsync(ProductResponse notification, CancellationToken cancellationToken)
    {
        await emailService.SendAsync($"Product {notification.Name} created...", notification.Dump(), cancellationToken);
    }
}

// Exemplo de uso

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
```

## Logging

O `TheMediator` utiliza o `ILogger` para registrar informações sobre as requisições e respostas via método `Trace`.

Por default, essa opção vem desabilitada.

Para mudar essa configuração é necessário mudar o level do logger no `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "TheMediator.Core": "Trace"
    }
  }
}
```
