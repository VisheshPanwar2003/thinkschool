using AuthApi.Data;
using AuthApi.Endpoints;
using AuthApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Auth") ?? "Data Source=auth.db"));
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await database.Database.EnsureCreatedAsync();
}

app.MapAuthEndpoints();
app.Run();

public partial class Program;
