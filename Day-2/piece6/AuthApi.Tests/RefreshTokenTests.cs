using System.Net;
using System.Net.Http.Json;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AuthApi.Tests;

public sealed class RefreshTokenTests
{
    [Fact]
    public async Task Reusing_replaced_refresh_token_revokes_the_entire_chain()
    {
        await using var factory = new AuthApiFactory();
        var client = factory.CreateClient();

        var login = await PostAsync<LoginRequest, TokenPair>(client, "/api/auth/login", new("user-1"));
        var firstRefresh = await PostAsync<RefreshRequest, TokenPair>(client, "/api/auth/refresh", new(login.RefreshToken));
        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(login.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var hashes = new[] { AuthService.HashToken(login.RefreshToken), AuthService.HashToken(firstRefresh.RefreshToken) };
        var tokens = await scope.ServiceProvider.GetRequiredService<AuthDbContext>().RefreshTokens
            .Where(token => hashes.Contains(token.Token)).ToListAsync();
        Assert.All(tokens, token => Assert.NotNull(token.RevokedAt));
    }

    private static async Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient client, string url, TRequest request)
    {
        var response = await client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }
}

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public AuthApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.AddDbContext<AuthDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
