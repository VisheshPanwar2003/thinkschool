using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuotesApi.Dtos;

namespace QuotesApi.Tests.Integration;

public class RefreshTokenTests : TestBase
{
    public RefreshTokenTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewTokens()
    {
        var login = await LoginAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                RefreshToken = login.RefreshToken
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var refreshed = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        refreshed.Should().NotBeNull();
        refreshed!.AccessToken.Should().NotBeNullOrEmpty();
        refreshed.RefreshToken.Should().NotBeNullOrEmpty();

        refreshed.RefreshToken
            .Should()
            .NotBe(login.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithUnknownRefreshToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                RefreshToken = "invalid-refresh-token"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithReusedRefreshToken_ReturnsUnauthorized()
    {
        var login = await LoginAsync();

        // First use should succeed and rotate the token.
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                RefreshToken = login.RefreshToken
            });

        firstResponse.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        // Reusing the old token should fail.
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                RefreshToken = login.RefreshToken
            });

        secondResponse.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}