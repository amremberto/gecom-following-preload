using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Contracts.Preload.Notes.Create;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for note-related operations.
/// </summary>
internal sealed class NoteService : INoteService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    public NoteService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<NoteResponse?> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Notes", UriKind.Relative);

        NoteResponse? response = await _httpClientService.PostAsync<CreateNoteRequest, NoteResponse>(
            requestUri,
            request,
            cancellationToken);

        return response;
    }
}
