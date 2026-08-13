using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace QuotesApi.Tests.Integration;

public class QuoteWriteTests : TestBase
{
    public QuoteWriteTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    // ============================================================
    // POST
    // ============================================================

    [Fact]
    public async Task PostQuote_WithoutAuth_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = "Test",
                Text = "Test Text"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostQuote_WithAuthentication_CreatesQuote()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = "Albert Einstein",
                Text = "Life is like riding a bicycle."
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Albert Einstein");
        content.Should().Contain(
            "Life is like riding a bicycle.");
    }

    [Fact]
    public async Task PostQuote_WithEmptyText_ReturnsBadRequest()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = "Test Author",
                Text = ""
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostQuote_WithEmptyAuthor_ReturnsBadRequest()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = "",
                Text = "Valid quote text"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostQuote_WithTextOver1000Characters_ReturnsBadRequest()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = "Test Author",
                Text = new string('a', 1001)
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostQuote_WithAuthorOver200Characters_ReturnsBadRequest()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = new string('a', 201),
                Text = "Valid quote"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    // ============================================================
    // PUT
    // ============================================================

    [Fact]
    public async Task UpdateAuthor_WhenQuoteExists_ReturnsNoContent()
    {
        var quoteId = await CreateQuoteAsync(
            "Original Author",
            "Test quote");

        AuthenticateAs(GetAdminUserId());

        var response = await _client.PutAsJsonAsync(
            $"/api/quotes/{quoteId}/author",
            new
            {
                Author = "New Author"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(
            $"/api/quotes/{quoteId}");

        getResponse.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();

        content.Should().Contain("New Author");
    }

    [Fact]
    public async Task UpdateAuthor_WhenQuoteDoesNotExist_ReturnsNotFound()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PutAsJsonAsync(
            "/api/quotes/999/author",
            new
            {
                Author = "New Author"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAuthor_WithEmptyAuthor_ReturnsBadRequest()
    {
        var quoteId = await CreateQuoteAsync();

        AuthenticateAs(GetAdminUserId());

        var response = await _client.PutAsJsonAsync(
            $"/api/quotes/{quoteId}/author",
            new
            {
                Author = ""
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthor_WithAuthorOver200Characters_ReturnsBadRequest()
    {
        var quoteId = await CreateQuoteAsync();

        AuthenticateAs(GetAdminUserId());

        var response = await _client.PutAsJsonAsync(
            $"/api/quotes/{quoteId}/author",
            new
            {
                Author = new string('a', 201)
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }
}