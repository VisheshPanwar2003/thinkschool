using FluentAssertions;
using CollectionAggregate = Collection.Domain.Collection;

namespace Tests.Domain;

public class CollectionTests
{
    [Fact]
    public void Empty_name_throws()
    {
        var act = () => new CollectionAggregate("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Name_over_80_characters_throws()
    {
        var act = () => new CollectionAggregate(new string('a', CollectionAggregate.MaxNameLength + 1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fifty_first_item_throws()
    {
        var collection = new CollectionAggregate("Favorites");
        foreach (var quoteId in Enumerable.Range(1, CollectionAggregate.MaxItems)) collection.AddQuote(quoteId);

        var act = () => collection.AddQuote(CollectionAggregate.MaxItems + 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Duplicate_quote_id_throws()
    {
        var collection = new CollectionAggregate("Favorites");
        collection.AddQuote(42);

        var act = () => collection.AddQuote(42);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Removing_non_existent_item_throws()
    {
        var collection = new CollectionAggregate("Favorites");

        var act = () => collection.RemoveQuote(42);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adding_then_removing_leaves_zero_items()
    {
        var collection = new CollectionAggregate("Favorites");
        collection.AddQuote(42);

        collection.RemoveQuote(42);

        collection.QuoteIds.Should().BeEmpty();
    }
}
