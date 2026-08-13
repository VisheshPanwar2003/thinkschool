using FluentAssertions;
using QuotesApi.Models;

namespace QuotesApi.Tests.Integration;

public class QuoteDomainTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = Quote.Create(
            "Albert Einstein",
            "Life is like riding a bicycle.",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Author.Should()
            .Be("Albert Einstein");

        result.Value.Text.Should()
            .Be("Life is like riding a bicycle.");

        result.Value.CreatedAt.Should()
            .Be(CreatedAt);

        result.Value.UserId.Should()
            .Be(1);

        result.Value.IsDeleted.Should()
            .BeFalse();
    }

    [Fact]
    public void Create_WithEmptyText_ReturnsFailure()
    {
        var result = Quote.Create(
            "Author",
            "",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should()
            .Be("Text must be between 1 and 1000 characters.");
    }

    [Fact]
    public void Create_WithWhitespaceText_ReturnsFailure()
    {
        var result = Quote.Create(
            "Author",
            "   ",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithTextOver1000Characters_ReturnsFailure()
    {
        var result = Quote.Create(
            "Author",
            new string('a', 1001),
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithEmptyAuthor_ReturnsFailure()
    {
        var result = Quote.Create(
            "",
            "Valid quote",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should()
            .Be("Author must be between 1 and 200 characters.");
    }

    [Fact]
    public void Create_WithWhitespaceAuthor_ReturnsFailure()
    {
        var result = Quote.Create(
            "   ",
            "Valid quote",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithAuthorOver200Characters_ReturnsFailure()
    {
        var result = Quote.Create(
            new string('a', 201),
            "Valid quote",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ChangeAuthor_WithValidAuthor_ReturnsSuccess()
    {
        var quote = CreateValidQuote();

        var result = quote.ChangeAuthor("New Author");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        quote.Author.Should().Be("New Author");
    }

    [Fact]
    public void ChangeAuthor_WithEmptyAuthor_ReturnsFailure()
    {
        var quote = CreateValidQuote();

        var result = quote.ChangeAuthor("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        quote.Author.Should()
            .Be("Original Author");
    }

    [Fact]
    public void ChangeAuthor_WithWhitespaceAuthor_ReturnsFailure()
    {
        var quote = CreateValidQuote();

        var result = quote.ChangeAuthor("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ChangeAuthor_WithAuthorOver200Characters_ReturnsFailure()
    {
        var quote = CreateValidQuote();

        var result = quote.ChangeAuthor(
            new string('a', 201));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        quote.Author.Should()
            .Be("Original Author");
    }

    [Fact]
    public void ChangeAuthor_WhenQuoteIsDeleted_ReturnsFailure()
    {
        var quote = CreateValidQuote();

        quote.Delete();

        var result = quote.ChangeAuthor("New Author");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        result.Error!.Message.Should()
            .Be("Cannot modify a deleted quote.");

        quote.Author.Should()
            .Be("Original Author");
    }

    [Fact]
    public void Delete_SetsIsDeletedToTrue()
    {
        var quote = CreateValidQuote();

        quote.IsDeleted.Should().BeFalse();

        quote.Delete();

        quote.IsDeleted.Should().BeTrue();
    }

    private static Quote CreateValidQuote()
    {
        var result = Quote.Create(
            "Original Author",
            "Original quote",
            CreatedAt,
            1);

        result.IsSuccess.Should().BeTrue();

        return result.Value!;
    }
}