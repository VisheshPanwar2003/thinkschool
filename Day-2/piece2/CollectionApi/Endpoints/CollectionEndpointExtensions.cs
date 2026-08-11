using CollectionApi.Models;
using CollectionApi.Services;

namespace CollectionApi.Endpoints;

public static class CollectionEndpointExtensions
{
    public static IEndpointRouteBuilder MapCollectionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var collections = endpoints.MapGroup("/api/collections");
        collections.MapGet("", GetAllAsync);
        collections.MapGet("/{id:int}", GetByIdAsync);
        collections.MapPost("", CreateAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(ICollectionService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.GetAllAsync(cancellationToken)); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return Results.StatusCode(499); }
    }

    private static async Task<IResult> GetByIdAsync(int id, ICollectionService service, CancellationToken cancellationToken)
    {
        try
        {
            return await service.GetByIdAsync(id, cancellationToken) is { } item ? Results.Ok(item) : Results.NotFound();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return Results.StatusCode(499); }
    }

    private static async Task<IResult> CreateAsync(CreateCollectionRequest request, ICollectionService service, CancellationToken cancellationToken)
    {
        try
        {
            var item = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/collections/{item.Id}", item);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return Results.StatusCode(499); }
    }
}
