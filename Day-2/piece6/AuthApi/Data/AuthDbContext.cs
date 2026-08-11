using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => token.Token).IsUnique();
            entity.Property(token => token.Token).HasMaxLength(128).IsRequired();
            entity.Property(token => token.UserId).HasMaxLength(100).IsRequired();
            entity.Property(token => token.FamilyId).HasMaxLength(36).IsRequired();
            entity.Property(token => token.ReplacedByToken).HasMaxLength(128);
            entity.Property(token => token.ExpiresAt).IsRequired();
        });
    }
}
