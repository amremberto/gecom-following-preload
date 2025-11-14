using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.CreateSapAccount;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.DeleteSapAccount;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccounts;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccountsPaged;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByAccountNumber;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByCuit;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.UpdateSapAccount;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts.Create;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts.GetAll;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing SAP accounts.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class SapAccountsController : VersionedApiController
{
    /// <summary>
    /// Gets all SAP accounts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all SAP accounts.</returns>
    /// <response code="200">Returns the list of SAP accounts.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<SapAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllSapAccounts", "Gets all SAP accounts.")]
    public async Task<ActionResult<IEnumerable<SapAccountResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllSapAccountsQuery query = new();

        Result<IEnumerable<SapAccountResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets SAP accounts with pagination.
    /// </summary>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result with SAP accounts.</returns>
    /// <response code="200">Returns paged SAP accounts.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paged")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(PagedResponse<SapAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllSapAccountsPaged", "Gets SAP accounts with pagination.")]
    public async Task<ActionResult<PagedResponse<SapAccountResponse>>> GetAllPagedAsync(
        [FromQuery] GetAllSapAccountsRequest request,
        CancellationToken cancellationToken)
    {
        int page = request.Page ?? 1;
        int pageSize = request.PageSize ?? 20;

        if (page <= 0 || pageSize <= 0)
        {
            return Problem(
                detail: "Invalid pagination parameters.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Pagination.Invalid");
        }

        GetAllSapAccountsPagedQuery query = new(page, pageSize);

        Result<PagedResponse<SapAccountResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a SAP account by its account number.
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SAP account if found.</returns>
    /// <response code="200">Returns the SAP account.</response>
    /// <response code="400">If the accountNumber parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the SAP account was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{accountNumber}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(SapAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetSapAccountByAccountNumber", "Gets a SAP account by its account number.")]
    public async Task<ActionResult<SapAccountResponse>> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        GetSapAccountByAccountNumberQuery query = new(accountNumber);

        Result<SapAccountResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a SAP account by its CUIT.
    /// </summary>
    /// <param name="cuit">Account CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SAP account if found.</returns>
    /// <response code="200">Returns the SAP account.</response>
    /// <response code="400">If the cuit parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the SAP account was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("cuit/{cuit}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(SapAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetSapAccountByCuit", "Gets a SAP account by its CUIT.")]
    public async Task<ActionResult<SapAccountResponse>> GetByCuitAsync(
        string cuit,
        CancellationToken cancellationToken)
    {
        GetSapAccountByCuitQuery query = new(cuit);

        Result<SapAccountResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new SAP account.
    /// </summary>
    /// <param name="request">The SAP account data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created SAP account.</returns>
    /// <response code="201">Returns the created SAP account.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a SAP account with the same account number already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(SapAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateSapAccount", "Creates a new SAP account.")]
    public async Task<ActionResult<SapAccountResponse>> CreateAsync(
        [FromBody] CreateSapAccountRequest request,
        CancellationToken cancellationToken)
    {
        CreateSapAccountCommand command = new(
            request.Accountnumber,
            request.Name,
            request.Address1City,
            request.Address1Stateorprovince,
            request.Address1Postalcode,
            request.Address1Line1,
            request.Telephone1,
            request.Fax,
            request.Address1Country,
            request.NewCuit,
            request.NewBloqueado,
            request.NewRubro,
            request.NewIibb,
            request.Emailaddress1,
            request.Customertypecode,
            request.NewGproveedor,
            request.Cbu);

        Result<SapAccountResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByAccountNumberAsync), new { accountNumber = request.Accountnumber });
    }

    /// <summary>
    /// Updates an existing SAP account.
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="request">The SAP account data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated SAP account.</returns>
    /// <response code="200">Returns the updated SAP account.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the SAP account was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{accountNumber}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(SapAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateSapAccount", "Updates an existing SAP account.")]
    public async Task<ActionResult<SapAccountResponse>> UpdateAsync(
        string accountNumber,
        [FromBody] UpdateSapAccountRequest request,
        CancellationToken cancellationToken)
    {
        UpdateSapAccountCommand command = new(
            accountNumber,
            request.Name,
            request.Address1City,
            request.Address1Stateorprovince,
            request.Address1Postalcode,
            request.Address1Line1,
            request.Telephone1,
            request.Fax,
            request.Address1Country,
            request.NewCuit,
            request.NewBloqueado,
            request.NewRubro,
            request.NewIibb,
            request.Emailaddress1,
            request.Customertypecode,
            request.NewGproveedor,
            request.Cbu);

        Result<SapAccountResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a SAP account by its account number.
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">SAP account deleted successfully.</response>
    /// <response code="400">If the accountNumber parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the SAP account was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{accountNumber}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteSapAccount", "Deletes a SAP account by its account number.")]
    public async Task<ActionResult> DeleteAsync(string accountNumber, CancellationToken cancellationToken)
    {
        DeleteSapAccountCommand command = new(accountNumber);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

