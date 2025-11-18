using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApp.Extensions.Auth;

/// <summary>
/// Authorization requirement for SingleSociety role that validates the CUIT claim exists.
/// </summary>
public sealed class ProviderRequirement : IAuthorizationRequirement
{
}
