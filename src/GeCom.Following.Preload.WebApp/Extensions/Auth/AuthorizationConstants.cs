namespace GeCom.Following.Preload.WebApp.Extensions.Auth;

/// <summary>
/// Constants for authorization roles.
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Claim type for roles in OIDC tokens.
    /// </summary>
    public const string RoleClaimType = "role";

    /// <summary>
    /// Claim type for society CUIT in JWT tokens.
    /// </summary>
    public const string SocietyCuitClaimType = "following.society.cuit";

    /// <summary>
    /// Available roles in the system.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Administrator role with full access to all module options.
        /// </summary>
        public const string FollowingAdministrator = "Following.Administrator";

        /// <summary>
        /// Read-only role with access to view documents from all societies.
        /// </summary>
        public const string FollowingPreloadReadOnly = "Following.Preload.ReadOnly";

        /// <summary>
        /// Role with CRUD access to all documents from all societies.
        /// </summary>
        public const string FollowingPreloadAllSocieties = "Following.Preload.AllSocieties";

        /// <summary>
        /// Role with CRUD access to documents from a single society (identified by CUIT claim).
        /// </summary>
        public const string FollowingPreloadSingleSociety = "Following.Preload.SingleSociety";
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
        /// Policy requiring preload read access (ReadOnly, AllSocieties, SingleSociety, or Administrator).
        /// </summary>
        public const string RequirePreloadRead = "RequirePreloadRead";

        /// <summary>
        /// Policy requiring preload write access (AllSocieties, SingleSociety, or Administrator).
        /// </summary>
        public const string RequirePreloadWrite = "RequirePreloadWrite";
    }
}

