using AuthApi.Models;
using AuthApi.Services;

namespace AuthApi.Endpoints;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth");
        auth.MapPost("/login", LoginAsync);
        auth.MapPost("/refresh", RefreshAsync);
        auth.MapPost("/logout", LogoutAsync);
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, IAuthService auth, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["userId"] = ["UserId is required."] });
        }

        return Results.Ok(await auth.LoginAsync(request.UserId.Trim(), cancellationToken));
    }

    private static async Task<IResult> RefreshAsync(RefreshRequest request, IAuthService auth, CancellationToken cancellationToken)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, cancellationToken);
        if (result.IsSuccess) return Results.Ok(result.Tokens);
        return result.Failure == RefreshFailure.ReuseDetected
            ? Results.Problem("Refresh token reuse detected. Please sign in again.", statusCode: StatusCodes.Status401Unauthorized)
            : Results.Unauthorized();
    }

    private static async Task<IResult> LogoutAsync(LogoutRequest request, IAuthService auth, CancellationToken cancellationToken) =>
        await auth.LogoutAsync(request.RefreshToken, cancellationToken) ? Results.NoContent() : Results.Unauthorized();
}
