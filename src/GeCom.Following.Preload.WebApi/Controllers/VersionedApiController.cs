namespace GeCom.Following.Preload.WebApi.Controllers;

/// <summary>
/// Base controller class for versioned API endpoints.
/// </summary>
/// <remarks>
/// This controller provides the standard routing pattern for versioned API endpoints,
/// inheriting common functionality from BaseApiController.
/// </remarks>
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class VersionedApiController : BaseApiController;
