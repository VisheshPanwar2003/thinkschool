using Microsoft.EntityFrameworkCore;
using QuotesApi.Entities;
using QuotesApi.Infrastructure;

namespace QuotesApi.Repositories;

public interface IQuoteRepository
{
    Task<(IReadOnlyList<Quote> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken ct);
    Task<Quote?> GetByIdAsync(int id, CancellationToken ct);
    Task<Quote> AddAsync(Quote quote, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public class QuoteRepository : IQuoteRepository
{
    private readonly AppDbContext _db;
    public QuoteRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Quote> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken ct)
    {
        var total = await _db.Quotes.CountAsync(ct);
        var items = await _db.Quotes
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Quote?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<Quote> AddAsync(Quote quote, CancellationToken ct)
    {
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(ct);
        return quote;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var rows = await _db.Quotes.Where(q => q.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}