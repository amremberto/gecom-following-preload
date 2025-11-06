namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Constants for authorization roles and permissions.
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Claim type for roles in JWT tokens.
    /// </summary>
    public const string RoleClaimType = "role";

    /// <summary>
    /// Claim type for permissions in JWT tokens.
    /// </summary>
    public const string PermissionClaimType = "permission";

    /// <summary>
    /// Available roles in the system.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Administrator role with full access.
        /// </summary>
        public const string Administrator = "Administrator";

        /// <summary>
        /// Manager role with elevated permissions.
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        /// User role with standard permissions.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// Viewer role with read-only permissions.
        /// </summary>
        public const string Viewer = "Viewer";
    }

    /// <summary>
    /// Available permissions in the system.
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// Permission to read societies.
        /// </summary>
        public const string SocietiesRead = "societies:read";

        /// <summary>
        /// Permission to create societies.
        /// </summary>
        public const string SocietiesCreate = "societies:create";

        /// <summary>
        /// Permission to update societies.
        /// </summary>
        public const string SocietiesUpdate = "societies:update";

        /// <summary>
        /// Permission to delete societies.
        /// </summary>
        public const string SocietiesDelete = "societies:delete";

        /// <summary>
        /// Permission to read all preload data.
        /// </summary>
        public const string PreloadRead = "preload:read";

        /// <summary>
        /// Permission to manage all preload data.
        /// </summary>
        public const string PreloadManage = "preload:manage";
    }

    /// <summary>
    /// Authorization policy names.
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy requiring any authenticated user.
        /// </summary>
        public const string RequireAuthenticated = "RequireAuthenticated";

        /// <summary>
        /// Policy requiring administrator role.
        /// </summary>
        public const string RequireAdministrator = "RequireAdministrator";

        /// <summary>
        /// Policy requiring manager or administrator role.
        /// </summary>
        public const string RequireManagerOrAdministrator = "RequireManagerOrAdministrator";

        /// <summary>
        /// Policy requiring societies read permission.
        /// </summary>
        public const string RequireSocietiesRead = "RequireSocietiesRead";

        /// <summary>
        /// Policy requiring societies create permission.
        /// </summary>
        public const string RequireSocietiesCreate = "RequireSocietiesCreate";

        /// <summary>
        /// Policy requiring societies update permission.
        /// </summary>
        public const string RequireSocietiesUpdate = "RequireSocietiesUpdate";

        /// <summary>
        /// Policy requiring societies delete permission.
        /// </summary>
        public const string RequireSocietiesDelete = "RequireSocietiesDelete";

        /// <summary>
        /// Policy requiring preload read permission.
        /// </summary>
        public const string RequirePreloadRead = "RequirePreloadRead";

        /// <summary>
        /// Policy requiring preload manage permission.
        /// </summary>
        public const string RequirePreloadManage = "RequirePreloadManage";
    }
}

