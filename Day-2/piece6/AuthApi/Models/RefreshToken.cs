namespace AuthApi.Models;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FamilyId { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
}

public sealed record LoginRequest(string UserId);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);
