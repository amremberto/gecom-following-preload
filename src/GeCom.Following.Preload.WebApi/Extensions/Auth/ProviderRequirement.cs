using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Authorization requirement for "Following.Preload.Providers" role that validates the CUIT claim exists.
/// </summary>
public sealed class ProviderRequirement : IAuthorizationRequirement
{
}
