namespace Collection.Domain;

public sealed class Collection
{
    public const int MaxNameLength = 80;
    public const int MaxItems = 50;

    private readonly List<int> _quoteIds = [];

    public Collection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name is required.", nameof(name));
        }

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Collection name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        Name = name;
    }

    public string Name { get; }

    public IReadOnlyCollection<int> QuoteIds => _quoteIds.AsReadOnly();

    public void AddQuote(int quoteId)
    {
        if (_quoteIds.Contains(quoteId))
        {
            throw new InvalidOperationException("Quote is already in the collection.");
        }

        if (_quoteIds.Count == MaxItems)
        {
            throw new InvalidOperationException($"Collection cannot contain more than {MaxItems} quotes.");
        }

        _quoteIds.Add(quoteId);
    }

    public void RemoveQuote(int quoteId)
    {
        if (!_quoteIds.Remove(quoteId))
        {
            throw new InvalidOperationException("Quote is not in the collection.");
        }
    }
}
