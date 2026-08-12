using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using QuotesApi.Models;

namespace QuotesApi.Authorization;

public sealed class OwnQuoteAuthorizationHandler
    : AuthorizationHandler<OwnQuoteRequirement, Quote>
{
    private readonly ILogger<OwnQuoteAuthorizationHandler> _logger;

    public OwnQuoteAuthorizationHandler(
        ILogger<OwnQuoteAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnQuoteRequirement requirement,
        Quote resource)
    {
        var subject = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (int.TryParse(subject, out var userId) && resource.OwnerId == userId)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} was denied permission to delete quote {QuoteId} owned by {OwnerId}.",
                subject ?? "unknown",
                resource.Id,
                resource.OwnerId);
        }

        return Task.CompletedTask;
    }
}
