using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Data;
using QuotesApi.Repositories;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                config.GetConnectionString("DefaultConnection")
                ?? "Data Source=quotes.db"));

        // Dependency Injection
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddSingleton<IClock, SystemClock>();

        // ============================================================
        // Authentication
        // ============================================================

        // Your existing JWT issuer from Day 2
        var internalIssuer = config["Jwt:Issuer"]!;

        // Microsoft Entra ID configuration
        var entraTenantId = config["Entra:TenantId"]!;

        var entraIssuer =
            $"https://login.microsoftonline.com/{entraTenantId}/v2.0";

        services.AddAuthentication(options =>
        {
            // SmartJwt decides which authentication scheme to use
            options.DefaultAuthenticateScheme = "SmartJwt";
            options.DefaultChallengeScheme = "SmartJwt";
        })

        // ============================================================
        // SmartJwt
        // Selects InternalJwt or EntraJwt based on the token issuer
        // ============================================================
        .AddPolicyScheme("SmartJwt", null, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authorization =
                    context.Request.Headers.Authorization.ToString();

                // No bearer token
                if (!authorization.StartsWith("Bearer "))
                {
                    return "InternalJwt";
                }

                var token = authorization["Bearer ".Length..];

                try
                {
                    // Read the token only to determine which
                    // authentication scheme should validate it.
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    // Your own JWT
                    if (jwt.Issuer == internalIssuer)
                    {
                        return "InternalJwt";
                    }

                    // Microsoft Entra JWT
                    if (jwt.Issuer == entraIssuer)
                    {
                        return "EntraJwt";
                    }
                }
                catch
                {
                    // Let the authentication handler reject
                    // an invalid token.
                }

                return "InternalJwt";
            };
        })

        // ============================================================
        // InternalJwt
        // Your Day 2 self-hosted JWT authentication
        // ============================================================
        .AddJwtBearer("InternalJwt", options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                config["Jwt:Secret"]!)),

                    ClockSkew = TimeSpan.Zero
                };
        })

        // ============================================================
        // EntraJwt
        // Microsoft Entra ID authentication
        // ============================================================
        .AddJwtBearer("EntraJwt", options =>
        {
            // Entra ID automatically provides the signing keys
            // and OpenID configuration from this authority.
            options.Authority = entraIssuer;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = entraIssuer,
                    ValidAudience = config["Entra:Audience"],

                    ClockSkew = TimeSpan.Zero
                };
        });

        return services;
    }
}
