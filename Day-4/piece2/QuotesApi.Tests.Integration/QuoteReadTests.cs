using System.Net;
using FluentAssertions;

namespace QuotesApi.Tests.Integration;

public class QuoteReadTests : TestBase
{
    public QuoteReadTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetQuotes_WhenCalled_ReturnsOkAndEmptyList()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/quotes");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("[]");
    }

    [Fact]
    public async Task GetQuotes_WhenQuotesExist_ReturnsQuotes()
    {
        await CreateQuoteAsync(
            "Albert Einstein",
            "Life is like riding a bicycle.");

        var response = await _client.GetAsync("/api/quotes");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Albert Einstein");
        content.Should().Contain(
            "Life is like riding a bicycle.");
    }

    [Fact]
    public async Task GetQuoteById_WhenQuoteDoesNotExist_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/quotes/999");

        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuoteById_WhenQuoteExists_ReturnsOk()
    {
        var quoteId = await CreateQuoteAsync(
            "Albert Einstein",
            "Life is like riding a bicycle.");

        var response = await _client.GetAsync(
            $"/api/quotes/{quoteId}");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Albert Einstein");
        content.Should().Contain(
            "Life is like riding a bicycle.");
    }
}