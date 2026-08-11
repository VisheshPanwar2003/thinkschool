using CollectionApi.Models;
using CollectionApi.Repositories;

namespace CollectionApi.Services;

public sealed class CollectionService(ICollectionRepository repository) : ICollectionService
{
    public Task<IReadOnlyList<CollectionItem>> GetAllAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<CollectionItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public Task<CollectionItem> CreateAsync(CreateCollectionRequest request, CancellationToken cancellationToken) =>
        repository.AddAsync(new CollectionItem { Name = request.Name, Description = request.Description }, cancellationToken);
}
