using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Data;
using QuotesApi.Dtos;
using QuotesApi.Models;

namespace QuotesApi.Tests.Integration;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

    protected TestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Email = "admin@test.com",
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("password123")
            });

            await db.SaveChangesAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected int GetAdminUserId()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        return db.Users
            .Single(u => u.Email == "admin@test.com")
            .Id;
    }

    protected string CreateToken(
        int userId,
        bool includeEditScope = true)
    {
        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                "admin@test.com")
        };

        if (includeEditScope)
        {
            claims.Add(
                new Claim("scope", "quotes.write"));
        }

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

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    protected void AuthenticateAs(int userId)
    {
        var token = CreateToken(userId);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<LoginResponse> LoginAsync()
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

        return login!;
    }

    protected async Task<int> CreateQuoteAsync(
        string author = "Test Author",
        string text = "Test quote")
    {
        AuthenticateAs(GetAdminUserId());

        var response = await _client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                Author = author,
                Text = text
            });

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);

        var json = await response.Content
            .ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        return document.RootElement
            .GetProperty("id")
            .GetInt32();
    }
}