using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Dtos;
using QuotesApi.Repositories;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class QuoteEndpointExtensions
{
    public static void MapQuoteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/quotes");

        // GET remains open
        group.MapGet("/", async (IQuoteRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetAllAsync(ct)));

        group.MapGet("/{id:int}", async (int id, IQuoteRepository repo, CancellationToken ct) =>
            await repo.GetByIdAsync(id, ct) is Quote quote ? Results.Ok(quote) : Results.NotFound());

        // POST, PUT, DELETE require Authorization
        group.MapPost("/", async (CreateQuoteRequest request, ClaimsPrincipal user, IQuoteRepository repo, IClock clock, CancellationToken ct) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            var result = Quote.Create(request.Author, request.Text, clock.UtcNow, userId);
            if (!result.IsSuccess) return Results.BadRequest(result.Error);
            
            await repo.AddAsync(result.Value!, ct);
            return Results.Created($"/api/quotes/{result.Value!.Id}", result.Value);
        }).RequireAuthorization("can-edit-quotes");

        group.MapPut("/{id:int}/author", async (int id, UpdateAuthorRequest request, IQuoteRepository repo, CancellationToken ct) =>
        {
            var quote = await repo.GetByIdAsync(id, ct);
            if (quote is null) return Results.NotFound();

            var result = quote.ChangeAuthor(request.Author);
            if (!result.IsSuccess) return Results.BadRequest(result.Error);

            await repo.UpdateAsync(quote, ct);
            return Results.NoContent();
        }).RequireAuthorization("can-edit-quotes");

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, IQuoteRepository repo, IAuthorizationService authorization, CancellationToken ct) =>
        {
            var quote = await repo.GetByIdAsync(id, ct);
            if (quote is null) return Results.NotFound();

            var authorizationResult = await authorization.AuthorizeAsync(
                user,
                quote,
                "can-delete-own-quotes");
            if (!authorizationResult.Succeeded) return Results.Forbid();

            quote.Delete();
            await repo.UpdateAsync(quote, ct);
            return Results.NoContent();
        }).RequireAuthorization();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return int.TryParse(subject, out userId);
    }
}
