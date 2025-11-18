using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Authorization handler for ProviderRequirement.
/// Validates that users with "Following.Preload.Providers" role have the CUIT claim.
/// </summary>
public sealed class ProviderHandler : AuthorizationHandler<ProviderRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderRequirement requirement)
    {
        if (context.User is null)
        {
            return Task.CompletedTask;
        }

        // Check if user has "Following.Preload.Providers" role
        bool hasProviderRole = context.User.IsInRole(AuthorizationConstants.Roles.FollowingPreloadProviders) ||
            context.User.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadProviders) ||
            context.User.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadProviders);

        // If user has "Following.Preload.Providers" role, they must have the CUIT claim
        if (hasProviderRole)
        {
            bool hasCuitClaim = context.User.FindFirst(AuthorizationConstants.SocietyCuitClaimType) is not null;

            if (hasCuitClaim)
            {
                context.Succeed(requirement);
            }
        }
        else
        {
            // If user doesn't have "Following.Preload.Providers" role, requirement is not applicable
            // Other roles (Administrator, AllSocieties, ReadOnly) don't need this validation
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
