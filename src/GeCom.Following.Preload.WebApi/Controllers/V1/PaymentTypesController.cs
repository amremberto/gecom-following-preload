using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.CreatePaymentType;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.DeletePaymentType;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetAllPaymentTypes;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeByDescripcion;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeById;
using GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.UpdatePaymentType;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes.Create;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing payment types.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class PaymentTypesController : VersionedApiController
{
    /// <summary>
    /// Gets all payment types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all payment types.</returns>
    /// <response code="200">Returns the list of payment types.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<PaymentTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllPaymentTypes", "Gets all payment types.")]
    public async Task<ActionResult<IEnumerable<PaymentTypeResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllPaymentTypesQuery query = new();

        Result<IEnumerable<PaymentTypeResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a payment type by its ID.
    /// </summary>
    /// <param name="id">Payment type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment type if found.</returns>
    /// <response code="200">Returns the payment type.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the payment type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(PaymentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetPaymentTypeById", "Gets a payment type by its ID.")]
    public async Task<ActionResult<PaymentTypeResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        GetPaymentTypeByIdQuery query = new(id);

        Result<PaymentTypeResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a payment type by its description.
    /// </summary>
    /// <param name="descripcion">Payment type description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment type if found.</returns>
    /// <response code="200">Returns the payment type.</response>
    /// <response code="400">If the descripcion parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the payment type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-descripcion/{descripcion}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(PaymentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetPaymentTypeByDescripcion", "Gets a payment type by its description.")]
    public async Task<ActionResult<PaymentTypeResponse>> GetByDescripcionAsync(string descripcion, CancellationToken cancellationToken)
    {
        GetPaymentTypeByDescripcionQuery query = new(descripcion);

        Result<PaymentTypeResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new payment type.
    /// </summary>
    /// <param name="request">The payment type data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created payment type.</returns>
    /// <response code="201">Returns the created payment type.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a payment type with the same description already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(PaymentTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreatePaymentType", "Creates a new payment type.")]
    public async Task<ActionResult<PaymentTypeResponse>> CreateAsync(
        [FromBody] CreatePaymentTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreatePaymentTypeCommand command = new(request.Descripcion);

        Result<PaymentTypeResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByDescripcionAsync), new { descripcion = request.Descripcion });
    }

    /// <summary>
    /// Updates an existing payment type.
    /// </summary>
    /// <param name="id">Payment type ID.</param>
    /// <param name="request">The payment type data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated payment type.</returns>
    /// <response code="200">Returns the updated payment type.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the payment type was not found.</response>
    /// <response code="409">If a payment type with the same description already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(PaymentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdatePaymentType", "Updates an existing payment type.")]
    public async Task<ActionResult<PaymentTypeResponse>> UpdateAsync(
        int id,
        [FromBody] UpdatePaymentTypeRequest request,
        CancellationToken cancellationToken)
    {
        UpdatePaymentTypeCommand command = new(id, request.Descripcion);

        Result<PaymentTypeResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a payment type by its ID.
    /// </summary>
    /// <param name="id">Payment type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Payment type deleted successfully.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the payment type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeletePaymentType", "Deletes a payment type by its ID.")]
    public async Task<ActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        DeletePaymentTypeCommand command = new(id);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

