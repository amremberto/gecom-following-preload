using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Documents.CreateDocument;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentById;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDates;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaidDocumentsByEmissionDates;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocuments;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocumentsByProvider;
using GeCom.Following.Preload.Application.Features.Preload.Documents.PreloadDocument;
using GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocument;
using GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocumentPdf;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Create;
using GeCom.Following.Preload.Contracts.Preload.Documents.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing documents.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class DocumentsController : VersionedApiController
{
    /// <summary>
    /// Gets all documents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all documents.</returns>
    /// <response code="200">Returns the list of documents.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllDocuments", "Gets all documents.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllDocumentsQuery query = new();

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets documents by emission date range based on user role.
    /// - Providers: Returns documents for the provider CUIT from the user's claim.
    /// - Societies: Returns documents for all societies assigned to the user.
    /// - Administrator/ReadOnly: Returns all documents without filtering.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of documents matching the criteria based on user role.</returns>
    /// <response code="200">Returns the list of documents.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-dates")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentsByDates", "Gets documents by emission date range based on user role.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetByDatesAsync(
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        CancellationToken cancellationToken)
    {
        // Get user roles
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role ||
                        c.Type == AuthorizationConstants.RoleClaimType ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        if (userRoles.Count == 0)
        {
            return BadRequest("User roles not found in the authentication token.");
        }

        // Get user email (for Societies role)
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value;

        // Validate email is present for Societies role
        if (string.IsNullOrWhiteSpace(userEmail) &&
            userRoles.Contains("Following.Preload.Societies", StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("User email is required for users with Societies role, but email claim was not found in the authentication token.");
        }

        // Get provider CUIT from claim (for Providers role)
        string? providerCuit = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType)?.Value;

        GetDocumentsByEmissionDatesQuery query = new(
            dateFrom,
            dateTo,
            userRoles,
            userEmail,
            providerCuit);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets paid documents by emission date range based on user role.
    /// Paid documents are those with state code "PagadoFin".
    /// - Providers: Returns paid documents for the provider CUIT from the user's claim.
    /// - Societies: Returns paid documents for all societies assigned to the user.
    /// - Administrator/ReadOnly: Returns all paid documents without filtering.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of paid documents matching the criteria based on user role.</returns>
    /// <response code="200">Returns the list of paid documents.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paid-by-dates")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetPaidDocumentsByDates", "Gets paid documents by emission date range based on user role.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetPaidDocumentsByDatesAsync(
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        CancellationToken cancellationToken)
    {
        // Get user roles
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role ||
                        c.Type == AuthorizationConstants.RoleClaimType ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        if (userRoles.Count == 0)
        {
            return BadRequest("User roles not found in the authentication token.");
        }

        // Get user email (for Societies role)
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value;

        // Validate email is present for Societies role
        if (string.IsNullOrWhiteSpace(userEmail) &&
            userRoles.Contains("Following.Preload.Societies", StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("User email is required for users with Societies role, but email claim was not found in the authentication token.");
        }

        // Get provider CUIT from claim (for Providers role)
        string? providerCuit = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType)?.Value;

        GetPaidDocumentsByEmissionDatesQuery query = new(
            dateFrom,
            dateTo,
            userRoles,
            userEmail,
            providerCuit);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets pending documents based on user role.
    /// - ReadOnly/Administrator: Returns all pending documents.
    /// - Societies: Returns pending documents for all societies assigned to the user.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of pending documents based on user role.</returns>
    /// <response code="200">Returns the list of pending documents.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("pending")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetPendingDocumentsAsync", "Gets pending documents based on user role.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetPendingDocumentsAsync(
        CancellationToken cancellationToken)
    {
        // Get user roles
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role ||
                        c.Type == AuthorizationConstants.RoleClaimType ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        if (userRoles.Count == 0)
        {
            return BadRequest("User roles not found in the authentication token. At least one role is required.");
        }

        // Get user email (required)
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return BadRequest("User email is required but was not found in the authentication token.");
        }

        GetPendingDocumentsQuery query = new(
            userRoles,
            userEmail);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets pending documents by provider CUIT.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="providerCuit">Provider CUIT. Must match the CUIT in the user's claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of pending documents for the specified provider.</returns>
    /// <response code="200">Returns the list of pending documents.</response>
    /// <response code="400">If the request parameters are invalid or the provider CUIT does not match the claim.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("pending-by-provider")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetPendingDocumentsByProviderAsync", "Gets pending documents by provider CUIT.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetPendingDocumentsByProviderAsync(
        [FromQuery] string providerCuit,
        CancellationToken cancellationToken)
    {
        // Validate that providerCuit is provided
        if (string.IsNullOrWhiteSpace(providerCuit))
        {
            return BadRequest("Provider CUIT is required.");
        }

        // Get CUIT from claim
        Claim? cuitClaim = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType);
        if (cuitClaim is null)
        {
            return BadRequest("CUIT claim not found in the authentication token.");
        }

        string claimCuit = cuitClaim.Value;

        // Validate that providerCuit matches the CUIT in the claim
        if (!string.Equals(providerCuit, claimCuit, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Provider CUIT does not match the CUIT in the authentication token.");
        }

        GetPendingDocumentsByProviderQuery query = new(
            providerCuit);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets documents by emission date range and provider CUIT.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT (required). Must match the CUIT in the user's claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of documents matching the criteria.</returns>
    /// <response code="200">Returns the list of documents.</response>
    /// <response code="400">If the request parameters are invalid or the provider CUIT does not match the claim.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-dates-and-provider")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentsByDatesAndProvider", "Gets documents by emission date range and provider CUIT.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetByDatesAndProviderAsync(
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        [FromQuery] string providerCuit,
        CancellationToken cancellationToken)
    {
        // Validate that providerCuit is provided
        if (string.IsNullOrWhiteSpace(providerCuit))
        {
            return BadRequest("Provider CUIT is required.");
        }

        // Get CUIT from claim
        Claim? cuitClaim = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType);
        if (cuitClaim is null)
        {
            return BadRequest("CUIT claim not found in the authentication token.");
        }

        string claimCuit = cuitClaim.Value;

        // Validate that providerCuit matches the CUIT in the claim
        if (!string.Equals(providerCuit, claimCuit, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Provider CUIT does not match the CUIT in the authentication token.");
        }

        GetDocumentsByEmissionDatesAndProviderQuery query = new(
            dateFrom,
            dateTo,
            providerCuit);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="request">The document data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document.</returns>
    /// <response code="201">Returns the created document.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateDocument", "Creates a new document.")]
    public async Task<ActionResult<DocumentResponse>> CreateAsync(
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        CreateDocumentCommand command = new(
            request.ProveedorCuit,
            request.SociedadCuit,
            request.TipoDocId,
            request.PuntoDeVenta,
            request.NumeroComprobante,
            request.FechaEmisionComprobante,
            request.Moneda,
            request.MontoBruto,
            request.Caecai,
            request.VencimientoCaecai,
            request.NombreSolicitante);

        Result<DocumentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetAllAsync));
    }

    /// <summary>
    /// Gets a document by its ID.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document if found.</returns>
    /// <response code="200">Returns the document.</response>
    /// <response code="400">If the docId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{docId}", Name = "GetDocumentById")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentById", "Gets a document by its ID.")]
    public async Task<ActionResult<DocumentResponse>> GetByIdAsync(int docId, CancellationToken cancellationToken)
    {
        GetDocumentByIdQuery query = new(docId);

        Result<DocumentResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Updates an existing document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="request">The document data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    /// <response code="200">Returns the updated document.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{docId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateDocument", "Updates an existing document.")]
    public async Task<ActionResult<DocumentResponse>> UpdateAsync(
        int docId,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        // Validar que el DocId de la URL coincida con el del cuerpo del request
        if (docId != request.DocId)
        {
            return BadRequest($"The DocId in the URL ({docId}) does not match the DocId in the request body ({request.DocId}).");
        }

        // Get the user email from claims
        string? userName = User.FindFirst(ClaimTypes.Name)?.Value ??
                           User.FindFirst("name")?.Value ?? "UnknownUser";
        string? userEmail = User.FindFirst(ClaimTypes.Email)?.Value ??
                            User.FindFirst("email")?.Value ?? userName;

        UpdateDocumentCommand command = new(
            request.DocId,
            userEmail,
            request.ProveedorCuit,
            request.SociedadCuit,
            request.TipoDocId,
            request.PuntoDeVenta,
            request.NumeroComprobante,
            request.FechaEmisionComprobante,
            request.Moneda,
            request.MontoBruto,
            request.CodigoDeBarras,
            request.Caecai,
            request.VencimientoCaecai,
            request.EstadoId,
            request.NombreSolicitante);

        Result<DocumentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Preloads a document by uploading a PDF file.
    /// Creates a new document with default values, saves the file to storage using impersonation,
    /// and creates an attachment record linking the file to the document.
    /// </summary>
    /// <param name="file">The PDF file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document with attachment.</returns>
    /// <response code="201">Returns the created document.</response>
    /// <response code="400">If the file is invalid or missing.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost("preload")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("PreloadDocument", "Preloads a document by uploading a PDF file.")]
    public async Task<ActionResult<DocumentResponse>> PreloadAsync(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A PDF file is required.");
        }

        // Get the user name and email from claims
        string? userName = User.FindFirst(ClaimTypes.Name)?.Value ??
                           User.FindFirst("name")?.Value ?? "UnknownUser";
        string? userEmail = User.FindFirst(ClaimTypes.Email)?.Value ??
                            User.FindFirst("email")?.Value ?? userName;

        // Check if user is a provider and get provider CUIT from claim
        string? providerCuit = null;
        bool isProvider = User.IsInRole(AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                          User.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                          User.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadProviders);

        if (isProvider)
        {
            providerCuit = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType)?.Value;
        }

        // Read file content
        byte[] fileContent;
        await using (MemoryStream memoryStream = new())
        {
            await file.CopyToAsync(memoryStream, cancellationToken);
            fileContent = memoryStream.ToArray();
        }

        PreloadDocumentCommand command = new(
            fileContent,
            file.FileName,
            file.ContentType,
            userEmail,
            providerCuit);

        Result<DocumentResponse> result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            // Devolver 201 Created usando ObjectResult directamente
            // Esto evita problemas con la validación de rutas en Created/CreatedAtAction
            ObjectResult objectResult = new(result.Value)
            {
                StatusCode = StatusCodes.Status201Created
            };
            return objectResult;
        }

        return CustomResults.ProblemActionResult(this, result);
    }

    /// <summary>
    /// Updates the PDF file associated with an existing document.
    /// Replaces the current PDF file with a new one uploaded by the user.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="file">The new PDF file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document with the new PDF attachment.</returns>
    /// <response code="200">Returns the updated document.</response>
    /// <response code="400">If the file is invalid or missing.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document or attachment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{docId}/pdf")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateDocumentPdf", "Updates the PDF file associated with an existing document.")]
    public async Task<ActionResult<DocumentResponse>> UpdatePdfAsync(
        int docId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A PDF file is required.");
        }

        // Get the user email from claims
        string? userName = User.FindFirst(ClaimTypes.Name)?.Value ??
                           User.FindFirst("name")?.Value ?? "UnknownUser";
        string? userEmail = User.FindFirst(ClaimTypes.Email)?.Value ??
                            User.FindFirst("email")?.Value ?? userName;

        // Read file content
        byte[] fileContent;
        await using (MemoryStream memoryStream = new())
        {
            await file.CopyToAsync(memoryStream, cancellationToken);
            fileContent = memoryStream.ToArray();
        }

        UpdateDocumentPdfCommand command = new(
            docId,
            fileContent,
            file.FileName,
            file.ContentType,
            userEmail);

        Result<DocumentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

}

