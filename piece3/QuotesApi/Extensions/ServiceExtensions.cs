using Microsoft.EntityFrameworkCore;
using QuotesApi.Entities;
using QuotesApi.Infrastructure;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public record CreateQuoteRequest(string Author, string Text);

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection") ?? "Data Source=quotes.db"));

        services.AddScoped<IQuoteRepository, QuoteRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes");

        group.MapGet("/", async (int? page, int? size, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            int p = page is null or < 1 ? 1 : page.Value;
            int s = size is null or < 1 ? 10 : Math.Min(size.Value, 100);

            logger.LogInformation("Fetching quotes page {Page} with size {Size}", p, s);
            var (items, total) = await repo.GetPagedAsync(p, s, ct);

            return Results.Ok(new { Page = p, Size = s, TotalCount = total, Data = items });
        });

        group.MapGet("/{id:int}", async (int id, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            logger.LogInformation("Fetching quote {Id}", id);
            var quote = await repo.GetByIdAsync(id, ct);
            return quote is not null ? Results.Ok(quote) : Results.NotFound();
        });

        group.MapPost("/", async (CreateQuoteRequest req, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(req.Author)) errors.Add(nameof(req.Author), ["Author is required."]);
            if (string.IsNullOrWhiteSpace(req.Text)) errors.Add(nameof(req.Text), ["Text is required."]);

            if (errors.Count > 0)
            {
                logger.LogWarning("Validation failed for quote creation.");
                return Results.ValidationProblem(errors);
            }

            var quote = new Quote { Author = req.Author.Trim(), Text = req.Text.Trim() };
            var created = await repo.AddAsync(quote, ct);

            logger.LogInformation("Created quote {Id}", created.Id);
            return Results.Created($"/api/quotes/{created.Id}", created);
        });

        group.MapDelete("/{id:int}", async (int id, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            logger.LogInformation("Deleting quote {Id}", id);
            var deleted = await repo.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}