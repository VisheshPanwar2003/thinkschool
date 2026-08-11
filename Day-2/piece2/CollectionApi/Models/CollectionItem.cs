namespace CollectionApi.Models;

public sealed class CollectionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed record CreateCollectionRequest(string Name, string Description);
