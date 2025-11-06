using MediatR;

namespace GeCom.Following.Preload.WebApi.Controllers;

/// <summary>
/// Base controller class that provides common functionality for all API controllers.
/// </summary>
/// <remarks>
/// This controller provides access to the MediatR sender for handling commands and queries
/// through the CQRS pattern, and includes common API controller attributes.
/// </remarks>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private ISender _mediator = null!;

    /// <summary>
    /// Gets the MediatR sender instance for handling commands and queries.
    /// </summary>
    /// <value>The MediatR sender instance, lazily initialized from the service provider.</value>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
