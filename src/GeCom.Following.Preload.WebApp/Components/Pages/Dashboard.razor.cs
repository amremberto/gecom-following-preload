using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Dashboard : IAsyncDisposable
{
    private bool _isLoading = true;

    private int _totalDocuments;
    private int _totalPurchaseOrders;
    private int _totalPendingDocuments;

    private IJSObjectReference? _dashboardModule;

    private List<ClaimInfo> _userClaims = [];
    private List<string> _userRoles = [];
    private string _accessToken = string.Empty;
    private string _idToken = string.Empty;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private IDashboardService DashboardService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadDashboardDataAsync();
            await LoadUserClaimsAndTokensAsync();
        }
        catch (Exception ex)
        {
            // Log error (in production, use ILogger)
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            // Set default values on error
            _totalDocuments = 0;
            _totalPurchaseOrders = 0;
            _totalPendingDocuments = 0;
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Loads dashboard data from the API.
    /// </summary>
    private async Task LoadDashboardDataAsync()
    {
        DashboardResponse? dashboardResponse = await DashboardService.GetDashboardAsync();

        if (dashboardResponse is not null)
        {
            _totalDocuments = dashboardResponse.TotalDocuments;
            _totalPurchaseOrders = dashboardResponse.TotalPurchaseOrders;
            _totalPendingDocuments = dashboardResponse.TotalPendingsDocuments;
        }
    }

    /// <summary>
    /// Loads user claims, roles, and tokens for display.
    /// </summary>
    private async Task LoadUserClaimsAndTokensAsync()
    {
        try
        {
            // Get authentication state
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user is not null)
            {
                // Load claims
                _userClaims = user.Claims
                    .Select(c => new ClaimInfo
                    {
                        Type = c.Type,
                        Value = c.Value,
                        Issuer = c.Issuer
                    })
                    .OrderBy(c => c.Type)
                    .ToList();

                // Load roles from claims first
                HashSet<string> rolesSet = new(user.Claims
                    .Where(c => c.Type == ClaimTypes.Role ||
                                c.Type == "role" ||
                                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .Distinct());

                // Get tokens from HttpContext
                HttpContext? httpContext = HttpContextAccessor.HttpContext;
                if (httpContext is not null)
                {
                    _accessToken = await httpContext.GetTokenAsync(
                        OpenIdConnectDefaults.AuthenticationScheme,
                        "access_token") ?? string.Empty;

                    _idToken = await httpContext.GetTokenAsync(
                        OpenIdConnectDefaults.AuthenticationScheme,
                        "id_token") ?? string.Empty;

                    // Extract roles from access_token if available
                    if (!string.IsNullOrWhiteSpace(_accessToken))
                    {
                        List<string> rolesFromToken = ExtractRolesFromAccessToken(_accessToken);
                        foreach (string role in rolesFromToken)
                        {
                            rolesSet.Add(role);
                        }
                    }
                }

                // Convert to sorted list
                _userRoles = rolesSet.OrderBy(r => r).ToList();
            }
            else
            {
                // Even if user is null, try to get tokens
                HttpContext? httpContext = HttpContextAccessor.HttpContext;
                if (httpContext is not null)
                {
                    _accessToken = await httpContext.GetTokenAsync(
                        OpenIdConnectDefaults.AuthenticationScheme,
                        "access_token") ?? string.Empty;

                    _idToken = await httpContext.GetTokenAsync(
                        OpenIdConnectDefaults.AuthenticationScheme,
                        "id_token") ?? string.Empty;

                    // Extract roles from access_token if available
                    if (!string.IsNullOrWhiteSpace(_accessToken))
                    {
                        _userRoles = ExtractRolesFromAccessToken(_accessToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user claims and tokens: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts roles from the access_token JWT.
    /// </summary>
    /// <param name="accessToken">The access token JWT string.</param>
    /// <returns>List of roles found in the token.</returns>
    private static List<string> ExtractRolesFromAccessToken(string accessToken)
    {
        List<string> roles = [];

        try
        {
            // JWT format: header.payload.signature
            string[] parts = accessToken.Split('.');
            if (parts.Length < 2)
            {
                System.Diagnostics.Debug.WriteLine("Invalid JWT format: access_token does not have the expected structure");
                return roles;
            }

            // Decode the payload (second part)
            string payloadBase64 = parts[1];

            // Add padding if necessary (Base64URL encoding may omit padding)
            switch (payloadBase64.Length % 4)
            {
                case 2:
                    payloadBase64 += "==";
                    break;
                case 3:
                    payloadBase64 += "=";
                    break;
            }

            // Replace Base64URL characters with Base64 characters
            payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');

            // Decode from Base64
            byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
            string payloadJson = Encoding.UTF8.GetString(payloadBytes);

            System.Diagnostics.Debug.WriteLine($"Access token payload: {payloadJson}");

            // Parse JSON
            using var doc = JsonDocument.Parse(payloadJson);
            JsonElement root = doc.RootElement;

            // Extract roles from the "role" claim
            // Roles can be a single string or an array of strings
            if (root.TryGetProperty("role", out JsonElement roleElement))
            {
                if (roleElement.ValueKind == JsonValueKind.Array)
                {
                    // Roles are in an array
                    foreach (JsonElement roleItem in roleElement.EnumerateArray())
                    {
                        string? roleValue = roleItem.GetString();
                        if (!string.IsNullOrWhiteSpace(roleValue))
                        {
                            roles.Add(roleValue);
                            System.Diagnostics.Debug.WriteLine($"Found role in access_token: {roleValue}");
                        }
                    }
                }
                else if (roleElement.ValueKind == JsonValueKind.String)
                {
                    // Single role as string
                    string? roleValue = roleElement.GetString();
                    if (!string.IsNullOrWhiteSpace(roleValue))
                    {
                        roles.Add(roleValue);
                        System.Diagnostics.Debug.WriteLine($"Found role in access_token: {roleValue}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No 'role' claim found in access_token payload");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error extracting roles from access_token: {ex.Message}");
        }

        return roles;
    }

    /// <summary>
    /// This method is called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                _dashboardModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/pages/dashboard.min.js");
                await JsRuntime.InvokeVoidAsync("loadDashboard");
                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");
            }

        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error en Dashboard.OnAfterRenderAsync:", ex.Message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dashboardModule is not null)
        {
            try
            {
                await _dashboardModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // El circuito ya está desconectado, no podemos hacer llamadas de JavaScript interop
                // Esto es normal cuando el componente se está eliminando
            }
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Copies text to clipboard using a safe method.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    private async Task CopyToClipboardAsync(string text)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("copyToClipboard", text);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"Error al copiar al portapapeles: {ex.Message}");
        }
    }

    /// <summary>
    /// Represents a claim for display purposes.
    /// </summary>
    private sealed class ClaimInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
    }
}
