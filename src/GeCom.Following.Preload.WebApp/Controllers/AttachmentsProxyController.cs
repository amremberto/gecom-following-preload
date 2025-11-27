using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeCom.Following.Preload.WebApp.Controllers;

/// <summary>
/// Controller that acts as a proxy for attachment downloads.
/// This allows PDF.js to load PDFs directly from the same origin without exposing the JWT token to JavaScript.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public sealed class AttachmentsProxyController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<AttachmentsProxyController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentsProxyController"/> class.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    /// <param name="logger">The logger.</param>
    public AttachmentsProxyController(
        IDocumentService documentService,
        ILogger<AttachmentsProxyController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Downloads a PDF file by attachment ID (proxy endpoint).
    /// This endpoint acts as a proxy to the API, handling authentication server-side.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PDF file.</returns>
    /// <response code="200">Returns the PDF file.</response>
    /// <response code="400">If the adjuntoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the attachment was not found or file does not exist.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{adjuntoId}/download")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DownloadAsync(int adjuntoId, CancellationToken cancellationToken)
    {
        if (adjuntoId <= 0)
        {
            return BadRequest("El ID del adjunto debe ser mayor que 0.");
        }

        try
        {
            _logger.LogInformation("Iniciando descarga de adjunto {AdjuntoId} para usuario {UserName}", 
                adjuntoId, User.Identity?.Name);

            // Download PDF bytes from API (token is handled automatically by HttpClientService)
            byte[]? pdfBytes = await _documentService.DownloadAttachmentAsync(adjuntoId, cancellationToken);

            if (pdfBytes is null || pdfBytes.Length == 0)
            {
                _logger.LogWarning("No se encontró el archivo PDF para el adjunto con ID {AdjuntoId}", adjuntoId);
                return NotFound($"No se encontró el archivo PDF para el adjunto con ID {adjuntoId}.");
            }

            _logger.LogInformation("PDF descargado exitosamente. Tamaño: {Size} bytes para adjunto {AdjuntoId}", 
                pdfBytes.Length, adjuntoId);

            // Set headers for PDF.js compatibility
            Response.Headers.Append("Accept-Ranges", "bytes");
            Response.Headers.Append("Cache-Control", "private, max-age=3600");

            // Return PDF file with proper content type
            return File(pdfBytes, "application/pdf", $"documento-{adjuntoId}.pdf");
        }
        catch (Services.ApiRequestException apiEx)
        {
            _logger.LogError(apiEx, 
                "Error de API al descargar adjunto {AdjuntoId}: {StatusCode} - {Message}", 
                adjuntoId, apiEx.StatusCode, apiEx.Message);
            
            return apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => Unauthorized(),
                System.Net.HttpStatusCode.Forbidden => Forbid(),
                System.Net.HttpStatusCode.NotFound => NotFound(apiEx.Message),
                System.Net.HttpStatusCode.BadRequest => BadRequest(apiEx.Message),
                _ => StatusCode(500, apiEx.Message)
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, 
                "Acceso no autorizado al adjunto {AdjuntoId} para usuario {UserName}", 
                adjuntoId, User.Identity?.Name);
            return Unauthorized("No tiene autorización para acceder a este recurso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error inesperado al descargar adjunto {AdjuntoId}", adjuntoId);
            return StatusCode(500, $"Error al descargar el archivo: {ex.Message}");
        }
    }
}

