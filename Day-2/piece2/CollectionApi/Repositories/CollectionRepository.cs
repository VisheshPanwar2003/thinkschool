using CollectionApi.Data;
using CollectionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionApi.Repositories;

public sealed class CollectionRepository(CollectionsDbContext database, ILogger<CollectionRepository> logger) : ICollectionRepository
{
    public async Task<IReadOnlyList<CollectionItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Loading all collection items");
        return await database.Collections.AsNoTracking().OrderBy(item => item.Id).ToListAsync(cancellationToken);
    }

    public Task<CollectionItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        database.Collections.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<CollectionItem> AddAsync(CollectionItem item, CancellationToken cancellationToken)
    {
        database.Collections.Add(item);
        await database.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created collection item {CollectionItemId}", item.Id);
        return item;
    }
}
