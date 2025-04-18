using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using TheMediator.Core;
using TheMediator.Core.DependencyInjection;
using TheMediator.Playground.Application;
using TheMediator.Playground.Contracts.Products;
using TheMediator.Playground.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTheMediator(configuration =>
{
    configuration.AddServicesFromAssemblies(typeof(Program).Assembly);
    configuration.AddFilter<MeasureTimeRequestFilter>();
    configuration.AddFilter<LoggerRequestFilter>();
});

builder.Services.AddSingleton<IProductsRepository, ProductsRepository>();

builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();

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

app.MapPut("/products/{id:guid}",
        async (
            [FromServices] ISender sender,
            [FromRoute] Guid id,
            [FromBody] ProductRequest request,
            CancellationToken cancellationToken) =>
        {
            await sender.SendAsync(request with { Id = id }, cancellationToken);
            return Results.Ok();
        })
    .WithName("Products.Update.ProductRequest");

app.MapDelete("/products/{id:guid}",
        async (
            [FromServices] ISender sender,
            [FromRoute] Guid id,
            CancellationToken cancellationToken) =>
        {
            var query = new ProductByIdRequest(id);
            await sender.SendAsync(query, cancellationToken);
            return Results.Ok();
        })
    .WithName("Products.Delete.ProductByIdQuery");

app.MapGet("/products/{id:guid}",
        async (
            [FromServices] ISender sender,
            [FromRoute] Guid id,
            CancellationToken cancellationToken) =>
        {
            var query = new ProductByIdQuery(id);
            var response =
                await sender.SendAsync<ProductByIdQuery, ProductResponse?>(query,
                    cancellationToken);
            return response is null
                ? Results.NotFound()
                : Results.Ok(response);
        })
    .WithName("Products.Get.ProductByIdQuery");

app.MapGet("/products/search",
        async (
            [FromServices] ISender sender,
            [FromQuery] string? query,
            CancellationToken cancellationToken) =>
        {
            var response =
                await sender.SendAsync<string, IReadOnlyCollection<ProductResponse>?>(query ?? string.Empty,
                    cancellationToken);
            return response is { Count: > 0 }
                ? Results.Ok(response)
                : Results.NoContent();
        })
    .WithName("Products.Get.ProductSearchQuery");

app.Run();