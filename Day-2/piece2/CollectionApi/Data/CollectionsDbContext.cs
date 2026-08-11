using CollectionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionApi.Data;

public sealed class CollectionsDbContext(DbContextOptions<CollectionsDbContext> options) : DbContext(options)
{
    public DbSet<CollectionItem> Collections => Set<CollectionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CollectionItem>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(2_000).IsRequired();
        });
    }
}
