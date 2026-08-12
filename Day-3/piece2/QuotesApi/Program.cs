using QuotesApi.Extensions;
using QuotesApi.Endpoints;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "can-edit-quotes",
        policy => policy.RequireClaim("scope", "quotes.write"));

    options.AddPolicy(
        "can-delete-own-quotes",
        policy =>
        {
            policy.RequireClaim("scope", "quotes.write");
            policy.AddRequirements(new OwnQuoteRequirement());
        });
});
builder.Services.AddScoped<IAuthorizationHandler, OwnQuoteAuthorizationHandler>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    
    // Seed a test user
    if (!db.Users.Any())
    {
        db.Users.Add(new User 
        { 
            Email = "admin@test.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") 
        });
        db.SaveChanges();
    }
}

// Middleware MUST be in this exact order
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapQuoteEndpoints();

app.Run();
