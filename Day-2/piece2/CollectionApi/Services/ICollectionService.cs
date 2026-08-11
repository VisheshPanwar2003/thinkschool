using CollectionApi.Models;

namespace CollectionApi.Services;

public interface ICollectionService
{
    Task<IReadOnlyList<CollectionItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<CollectionItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CollectionItem> CreateAsync(CreateCollectionRequest request, CancellationToken cancellationToken);
}
