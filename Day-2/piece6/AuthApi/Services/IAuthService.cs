using AuthApi.Models;

namespace AuthApi.Services;

public interface IAuthService
{
    Task<TokenPair> LoginAsync(string userId, CancellationToken cancellationToken);
    Task<RefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed record RefreshResult(TokenPair? Tokens, RefreshFailure Failure)
{
    public bool IsSuccess => Tokens is not null;
}

public enum RefreshFailure
{
    None,
    Invalid,
    ReuseDetected
}
