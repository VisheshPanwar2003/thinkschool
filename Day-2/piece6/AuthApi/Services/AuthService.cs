using System.Security.Cryptography;
using System.Text;
using AuthApi.Data;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services;

public sealed class AuthService(AuthDbContext database, ILogger<AuthService> logger) : IAuthService
{
    public async Task<TokenPair> LoginAsync(string userId, CancellationToken cancellationToken)
    {
        var rawRefreshToken = CreateSecureToken();
        database.RefreshTokens.Add(new RefreshToken
        {
            Token = HashToken(rawRefreshToken),
            UserId = userId,
            FamilyId = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });
        await database.SaveChangesAsync(cancellationToken);
        return CreateTokenPair(rawRefreshToken);
    }

    public async Task<RefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var tokenHash = HashToken(refreshToken);
        var existing = await database.RefreshTokens.SingleOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);

        if (existing is null || existing.ExpiresAt <= now)
        {
            return new RefreshResult(null, RefreshFailure.Invalid);
        }

        if (existing.RevokedAt is not null)
        {
            if (existing.ReplacedByToken is not null)
            {
                await RevokeFamilyAsync(existing.FamilyId, now, cancellationToken);
                logger.LogWarning("Security event: refresh token reuse detected for user {UserId}", existing.UserId);
                return new RefreshResult(null, RefreshFailure.ReuseDetected);
            }

            return new RefreshResult(null, RefreshFailure.Invalid);
        }

        var newRawRefreshToken = CreateSecureToken();
        existing.RevokedAt = now;
        existing.ReplacedByToken = HashToken(newRawRefreshToken);
        database.RefreshTokens.Add(new RefreshToken
        {
            Token = existing.ReplacedByToken,
            UserId = existing.UserId,
            FamilyId = existing.FamilyId,
            ExpiresAt = now.AddDays(7)
        });

        await database.SaveChangesAsync(cancellationToken);
        return new RefreshResult(CreateTokenPair(newRawRefreshToken), RefreshFailure.None);
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var existing = await database.RefreshTokens.SingleOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
        if (existing is null || existing.RevokedAt is not null) return false;

        existing.RevokedAt = DateTimeOffset.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task RevokeFamilyAsync(string familyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var family = await database.RefreshTokens.Where(t => t.FamilyId == familyId).ToListAsync(cancellationToken);
        foreach (var token in family)
        {
            token.RevokedAt ??= now;
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    private static TokenPair CreateTokenPair(string refreshToken)
    {
        var accessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var accessToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"access:{Guid.NewGuid():N}:{accessExpiresAt:O}"));
        return new TokenPair(accessToken, refreshToken, accessExpiresAt);
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string CreateSecureToken()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
