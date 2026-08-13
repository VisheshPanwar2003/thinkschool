using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace QuotesApi.Tests.Integration;

public class QuoteDeleteTests : TestBase
{
    public QuoteDeleteTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteQuote_WhenUserIsOwner_ReturnsNoContent()
    {
        var quoteId = await CreateQuoteAsync();

        AuthenticateAs(GetAdminUserId());

        var response = await _client.DeleteAsync(
            $"/api/quotes/{quoteId}");

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(
            $"/api/quotes/{quoteId}");

        getResponse.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuote_WhenQuoteDoesNotExist_ReturnsNotFound()
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.DeleteAsync(
            "/api/quotes/999");

        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuote_WhenUserIsNotOwner_ReturnsForbidden()
    {
        var quoteId = await CreateQuoteAsync();

        int otherUserId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<QuotesApi.Data.AppDbContext>();

            db.Users.Add(new QuotesApi.Models.User
            {
                Email = "other@test.com",
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("password123")
            });

            await db.SaveChangesAsync();

            otherUserId = db.Users
                .Single(u => u.Email == "other@test.com")
                .Id;
        }

        AuthenticateAs(otherUserId);

        var response = await _client.DeleteAsync(
            $"/api/quotes/{quoteId}");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteQuote_WithoutUserIdClaim_ReturnsForbidden()
    {
        var quoteId = await CreateQuoteAsync();

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Email,
                "admin@test.com")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "super-secret-jwt-signing-key-must-be-32-bytes-min!!"));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "QuotesApi",
            audience: "QuotesApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                new JwtSecurityTokenHandler()
                    .WriteToken(token));

        var response = await _client.DeleteAsync(
            $"/api/quotes/{quoteId}");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }
}