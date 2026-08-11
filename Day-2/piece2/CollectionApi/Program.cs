using Microsoft.EntityFrameworkCore;
using CollectionApi.Data;
using CollectionApi.Endpoints;
using CollectionApi.Repositories;
using CollectionApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CollectionsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Collections") ?? "Data Source=collections.db"));
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<ICollectionService, CollectionService>();

var app = builder.Build();
app.MapCollectionEndpoints();
app.Run();

public partial class Program;
