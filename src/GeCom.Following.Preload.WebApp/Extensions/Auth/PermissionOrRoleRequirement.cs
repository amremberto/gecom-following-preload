using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApp.Extensions.Auth;

/// <summary>
/// Authorization requirement that allows access if the user has a specific permission OR a specific role.
/// </summary>
public sealed class PermissionOrRoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the permission claim type.
    /// </summary>
    public string PermissionClaimType { get; }

    /// <summary>
    /// Gets the required permissions.
    /// </summary>
    public string[] RequiredPermissions { get; }

    /// <summary>
    /// Gets the required roles.
    /// </summary>
    public string[] RequiredRoles { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionOrRoleRequirement"/> class.
    /// </summary>
    /// <param name="permissionClaimType">The permission claim type.</param>
    /// <param name="requiredPermissions">The required permissions.</param>
    /// <param name="requiredRoles">The required roles.</param>
    public PermissionOrRoleRequirement(
        string permissionClaimType,
        string[] requiredPermissions,
        string[] requiredRoles)
    {
        PermissionClaimType = permissionClaimType;
        RequiredPermissions = requiredPermissions;
        RequiredRoles = requiredRoles;
    }
}

