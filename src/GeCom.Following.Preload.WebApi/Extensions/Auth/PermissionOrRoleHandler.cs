using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Authorization handler for PermissionOrRoleRequirement.
/// </summary>
public sealed class PermissionOrRoleHandler : AuthorizationHandler<PermissionOrRoleRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionOrRoleRequirement requirement)
    {
        if (context.User is null)
        {
            return Task.CompletedTask;
        }

        // Check if user has any of the required permissions
        bool hasPermission = requirement.RequiredPermissions.Length > 0 &&
            requirement.RequiredPermissions.Any(permission =>
                context.User.HasClaim(requirement.PermissionClaimType, permission));

        // Check if user has any of the required roles
        // Check both IsInRole (which uses RoleClaimType) and direct role claims
        bool hasRole = requirement.RequiredRoles.Length > 0 &&
            requirement.RequiredRoles.Any(role =>
                context.User.IsInRole(role) ||
                context.User.HasClaim(ClaimTypes.Role, role) ||
                context.User.HasClaim(AuthorizationConstants.RoleClaimType, role));

        // Grant access if user has permission OR role
        if (hasPermission || hasRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

