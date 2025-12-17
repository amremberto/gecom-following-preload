using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.PurchaseOrders;

public partial class PurchaseOrders : IAsyncDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private ISapPurchaseOrderService SapPurchaseOrderService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isDataTableLoading;
    private bool _hasSupportedRole;
    private IJSObjectReference? _tableDatatableModule;
    private Toast? _toast;
    private IEnumerable<SapPurchaseOrderResponse> _purchaseOrders = [];

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al inicializar la página: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is called after the component has rendered.
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                // Ensure _isLoading is false after initial render
                _isLoading = false;

                // Initialize toast service with component reference
                ToastService.SetToast(_toast);

                // Check if user has a supported role
                await HasSupportedRoleAsync();

                StateHasChanged();

                _tableDatatableModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/table-datatable.min.js");
                await InvokeAsync(StateHasChanged);

                // If user has a supported role, automatically load purchase orders
                if (UserHasSupportedRole())
                {
                    try
                    {
                        _isDataTableLoading = true;
                        StateHasChanged();

                        await JsRuntime.InvokeVoidAsync("destroyDataTable", "purchase-orders-datatable");
                        await GetPurchaseOrders();
                        await JsRuntime.InvokeVoidAsync("loadDataTable", "purchase-orders-datatable");
                    }
                    catch (Exception ex)
                    {
                        await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar órdenes de compra:", ex.Message);
                        await ShowToast("Ocurrió un error al cargar las órdenes de compra. Por favor, intente nuevamente más tarde.");
                    }
                    finally
                    {
                        _isDataTableLoading = false;
                        StateHasChanged();
                    }
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "purchase-orders-datatable");
                    await ShowToast("No tiene los permisos necesarios para ver las órdenes de compra. Por favor, contacte al administrador del sistema.", ToastType.Warning);
                }

                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            // Ensure loading states are reset on error
            _isLoading = false;
            await JsRuntime.InvokeVoidAsync("console.error", "Error en PurchaseOrders.OnAfterRenderAsync:", ex.Message);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Checks if the user has a supported role (Provider, Society, Administrator, or ReadOnly).
    /// </summary>
    /// <returns></returns>
    private async Task HasSupportedRoleAsync()
    {
        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            System.Security.Claims.ClaimsPrincipal? user = authState.User;

            if (user is null)
            {
                _hasSupportedRole = false;
                return;
            }

            // Check if user has any of the supported roles
            bool hasProviderRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                user.HasClaim(System.Security.Claims.ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadProviders);

            bool hasSocietyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                user.HasClaim(System.Security.Claims.ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadSocieties);

            bool hasAdminRole = user.IsInRole(AuthorizationConstants.Roles.FollowingAdministrator) ||
                user.HasClaim(System.Security.Claims.ClaimTypes.Role, AuthorizationConstants.Roles.FollowingAdministrator) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingAdministrator);

            bool hasReadOnlyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
                user.HasClaim(System.Security.Claims.ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadReadOnly);

            _hasSupportedRole = hasProviderRole || hasSocietyRole || hasAdminRole || hasReadOnlyRole;
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al verificar roles:", ex.Message);
            _hasSupportedRole = false;
        }
    }

    /// <summary>
    /// Checks if the user has a supported role.
    /// </summary>
    /// <returns>True if the user has a supported role, false otherwise.</returns>
    private bool UserHasSupportedRole()
    {
        return _hasSupportedRole;
    }

    /// <summary>
    /// Gets all purchase orders from the API.
    /// </summary>
    /// <returns></returns>
    private async Task GetPurchaseOrders()
    {
        try
        {
            IEnumerable<SapPurchaseOrderResponse>? response = await SapPurchaseOrderService.GetAllAsync(cancellationToken: default);

            _purchaseOrders = response ?? [];
        }
        catch (ApiRequestException httpEx)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar órdenes de compra:", httpEx.Message);
            await ShowToast(httpEx.Message);
            _purchaseOrders = [];
        }
        catch (UnauthorizedAccessException)
        {
            await ShowToast("No tiene autorización para realizar esta operación. Por favor, inicie sesión nuevamente.");
            _purchaseOrders = [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar órdenes de compra:", ex.Message);
            await ShowToast("Ocurrió un error al buscar las órdenes de compra. Por favor, intente nuevamente más tarde.");
            _purchaseOrders = [];
        }
        finally
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Shows a toast notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of toast.</param>
    /// <returns></returns>
    private async Task ShowToast(string message, ToastType type = ToastType.Error)
    {
        await ToastService.ShowAsync(message, type);
    }

    /// <summary>
    /// Disposes the component and cleans up resources.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_tableDatatableModule is not null)
            {
                await JsRuntime.InvokeVoidAsync("destroyDataTable", "purchase-orders-datatable");
                await _tableDatatableModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Ignore JS disconnected exceptions during disposal
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing PurchaseOrders component: {ex.Message}");
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
