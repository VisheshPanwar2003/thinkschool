using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuotesApi.Dtos;

namespace QuotesApi.Tests.Integration;

public class AuthenticationTests : TestBase
{
    public AuthenticationTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = "admin@test.com",
                Password = "password123"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var login = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        login.Should().NotBeNull();
        login!.AccessToken.Should().NotBeNullOrEmpty();
        login.RefreshToken.Should().NotBeNullOrEmpty();
        login.ExpiresIn.Should().Be(900);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = "admin@test.com",
                Password = "wrong-password"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = "unknown@test.com",
                Password = "password123"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync(
            "/api/auth/logout",
            new
            {
                RefreshToken = "some-token"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsNoContent()
    {
        var login = await LoginAsync();

        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/auth/logout",
            new
            {
                RefreshToken = login.RefreshToken
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithUnknownRefreshToken_ReturnsNoContent()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/auth/logout",
            new
            {
                RefreshToken = "unknown-token"
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);
    }
}