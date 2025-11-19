using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Documents.CreateDocument;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDates;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Create;
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
        // Try multiple claim types as email might be stored differently in IdentityServer tokens
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value ??
                           User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ??
                           User.FindFirst("preferred_username")?.Value;

        // If email is still not found, try to extract from access token directly
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            userEmail = ExtractEmailFromAccessToken();
        }

        // If email is still not found, log all available claims for debugging
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            // Log to help debug email claim issue - this will appear in Serilog logs
            System.Diagnostics.Debug.WriteLine($"[GetByDatesAsync] Email not found in claims. Available claims: {string.Join(", ", allClaims)}");

            // Also return a more descriptive error for Societies role
            if (userRoles.Contains("Following.Preload.Societies", StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("User email is required for users with Societies role, but email claim was not found in the authentication token. Please contact the administrator.");
            }
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
    /// Attempts to extract email from the access token directly.
    /// This is a fallback method in case the email is not in the claims.
    /// </summary>
    /// <returns>The email if found, null otherwise.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "S6932:Use model binding instead of accessing the raw request data", Justification = "Need to extract JWT token from Authorization header to decode email claim. This is a fallback when email is not in User.Claims. Model binding cannot be used for JWT token extraction.")]
    private string? ExtractEmailFromAccessToken()
    {
        try
        {
            // Get the Authorization header using Request property (available in ControllerBase)
            // Note: SonarQube S6932 is suppressed here because we need to extract the JWT token
            // from the Authorization header, which cannot be done via model binding
            if (!Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeaderValues) ||
                authHeaderValues.Count == 0)
            {
                return null;
            }

            string? authHeader = authHeaderValues[0];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Extract the token
            string token = authHeader.Substring("Bearer ".Length).Trim();

            // Decode the JWT token (without validation, just to read claims)
            // Note: This is safe because the token was already validated by JWT middleware
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            // Decode the payload (second part)
            string payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            byte[] payloadBytes = Convert.FromBase64String(payload);
            string payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            // Parse JSON to find email
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("email", out System.Text.Json.JsonElement emailElement))
            {
                return emailElement.GetString();
            }

            // Try alternative claim names
            if (doc.RootElement.TryGetProperty("preferred_username", out System.Text.Json.JsonElement usernameElement))
            {
                string? username = usernameElement.GetString();
                // Only use if it looks like an email
                if (!string.IsNullOrWhiteSpace(username) && username.Contains('@'))
                {
                    return username;
                }
            }
        }
        catch
        {
            // Silently fail - this is just a fallback
        }

        return null;
    }
}

