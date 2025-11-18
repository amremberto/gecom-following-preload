using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApp.Extensions.Auth;

/// <summary>
/// Authorization handler for SingleSocietyRequirement.
/// Validates that users with SingleSociety role have the CUIT claim.
/// </summary>
public sealed class SingleSocietyHandler : AuthorizationHandler<SingleSocietyRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SingleSocietyRequirement requirement)
    {
        if (context.User is null)
        {
            return Task.CompletedTask;
        }

        // Check if user has SingleSociety role
        bool hasSingleSocietyRole = context.User.IsInRole(AuthorizationConstants.Roles.FollowingPreloadSingleSociety) ||
            context.User.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadSingleSociety) ||
            context.User.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadSingleSociety);

        // If user has SingleSociety role, they must have the CUIT claim
        if (hasSingleSocietyRole)
        {
            bool hasCuitClaim = context.User.FindFirst(AuthorizationConstants.SocietyCuitClaimType) is not null;

            if (hasCuitClaim)
            {
                context.Succeed(requirement);
            }
        }
        else
        {
            // If user doesn't have SingleSociety role, requirement is not applicable
            // Other roles (Administrator, AllSocieties, ReadOnly) don't need this validation
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
