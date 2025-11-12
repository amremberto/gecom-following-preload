# Arquitectura de Servicios HTTP con Autenticación Automática

## 📋 Índice

- [Resumen](#resumen)
- [Arquitectura](#arquitectura)
- [Componentes](#componentes)
- [Flujo de Funcionamiento](#flujo-de-funcionamiento)
- [Uso](#uso)
- [Extensión para Nuevos Servicios](#extensión-para-nuevos-servicios)
- [Configuración](#configuración)
- [Ventajas](#ventajas)

## Resumen

Esta arquitectura proporciona una solución robusta y escalable para realizar peticiones HTTP a la WebAPI desde la aplicación Blazor Server, con autenticación JWT automática mediante un `DelegatingHandler`. La implementación separa las responsabilidades en capas, facilitando el mantenimiento y la extensibilidad.

### Características Principales

- ✅ **Autenticación automática**: El token JWT se agrega automáticamente a todas las peticiones
- ✅ **Sin repetición de código**: La lógica de autenticación está centralizada
- ✅ **Servicios especializados**: Cada dominio tiene su propio servicio (Dashboard, Documents, etc.)
- ✅ **Reutilizable**: Servicio base genérico para cualquier tipo de petición HTTP
- ✅ **Testeable**: Fácil de mockear en tests unitarios
- ✅ **Mantenible**: Cambios en autenticación solo afectan un componente

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Components                         │
│  (Dashboard.razor, Documents.razor, etc.)                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Usa
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              Domain Services                                 │
│  (IDashboardService, IDocumentsService, etc.)                │
│  - Lógica de negocio específica                              │
│  - Construye URIs de endpoints                              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Usa
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              IHttpClientService                             │
│  - Métodos genéricos (Get, Post, Put, Delete)               │
│  - Serialización/Deserialización JSON                       │
│  - Manejo de errores centralizado                           │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Usa
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              HttpClient                                      │
│  (Configurado con HttpClientFactory)                        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Pipeline
                       ▼
┌─────────────────────────────────────────────────────────────┐
│      AuthenticationDelegatingHandler                         │
│  - Agrega automáticamente el token JWT                     │
│  - Obtiene token desde HttpContext                          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Envía
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                    WebAPI                                    │
│  (https://localhost:7210/api/v1/...)                        │
└─────────────────────────────────────────────────────────────┘
```

## Componentes

### 1. AuthenticationDelegatingHandler

**Ubicación**: `Services/Handlers/AuthenticationDelegatingHandler.cs`

**Responsabilidad**: Intercepta todas las peticiones HTTP y agrega automáticamente el token JWT de autenticación al header `Authorization`.

**Características**:
- Obtiene el access token desde `HttpContext` usando `GetTokenAsync`
- Agrega el header `Authorization: Bearer {token}` si no está presente
- Se ejecuta automáticamente en el pipeline de HttpClient

**Código clave**:
```csharp
protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
{
    HttpContext? httpContext = _httpContextAccessor.HttpContext;
    if (httpContext is not null)
    {
        string? accessToken = await httpContext.GetTokenAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            "access_token");

        if (!string.IsNullOrWhiteSpace(accessToken) && 
            request.Headers.Authorization is null)
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    return await base.SendAsync(request, cancellationToken);
}
```

### 2. IHttpClientService / HttpClientService

**Ubicación**: 
- `Services/IHttpClientService.cs`
- `Services/HttpClientService.cs`

**Responsabilidad**: Servicio base genérico para realizar peticiones HTTP con serialización/deserialización JSON automática.

**Métodos disponibles**:
- `GetAsync<TResponse>(string requestUri, CancellationToken)` - GET request
- `PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken)` - POST request
- `PutAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken)` - PUT request
- `DeleteAsync(string requestUri, CancellationToken)` - DELETE request

**Características**:
- Serialización/Deserialización JSON automática
- Manejo centralizado de errores (401, 404, etc.)
- Configuración de `JsonSerializerOptions` cacheada (PropertyNameCaseInsensitive)
- Headers por defecto: `Accept: application/json`

**Ejemplo de uso interno**:
```csharp
DashboardResponse? response = await _httpClientService.GetAsync<DashboardResponse>(
    "/api/v1/Dashboard",
    cancellationToken);
```

### 3. IDashboardService / DashboardService

**Ubicación**:
- `Services/IDashboardService.cs`
- `Services/DashboardService.cs`

**Responsabilidad**: Servicio específico para operaciones del Dashboard. Encapsula la lógica de negocio y construye las URIs de los endpoints.

**Características**:
- Usa `IHttpClientService` internamente
- Conoce la versión de la API desde `PreloadApiSettings`
- Construye las URIs de endpoints específicos
- Fácil de testear (mockeable)

**Ejemplo**:
```csharp
public async Task<DashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
{
    string apiVersion = _apiSettings.Version;
    string requestUri = $"/api/{apiVersion}/Dashboard";

    return await _httpClientService.GetAsync<DashboardResponse>(
        requestUri,
        cancellationToken);
}
```

## Flujo de Funcionamiento

1. **Componente Blazor** (ej: `Dashboard.razor.cs`) llama a `IDashboardService.GetDashboardAsync()`

2. **DashboardService** construye la URI del endpoint y llama a `IHttpClientService.GetAsync<DashboardResponse>()`

3. **HttpClientService** crea un `HttpRequestMessage` y lo envía a través de `HttpClient.SendAsync()`

4. **AuthenticationDelegatingHandler** intercepta la petición:
   - Obtiene el `HttpContext` actual
   - Extrae el access token de la cookie de autenticación
   - Agrega el header `Authorization: Bearer {token}` si no está presente

5. La petición se envía a la **WebAPI** con el token incluido

6. La **WebAPI** valida el token y procesa la petición

7. La respuesta se deserializa automáticamente en `HttpClientService` y se retorna al componente

## Uso

### En un Componente Blazor

```csharp
@using GeCom.Following.Preload.WebApp.Services
@inject IDashboardService DashboardService

@code {
    private DashboardResponse? dashboardData;

    protected override async Task OnInitializedAsync()
    {
        dashboardData = await DashboardService.GetDashboardAsync();
    }
}
```

### Ejemplo Completo: Dashboard.razor.cs

```csharp
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Dashboard : IAsyncDisposable
{
    private bool _isLoading = true;
    private int _totalDocuments;
    private int _totalPurchaseOrders;
    private int _totalPendingDocuments;

    [Inject] private IDashboardService DashboardService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            // Manejo de errores
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        DashboardResponse? dashboardResponse = 
            await DashboardService.GetDashboardAsync();

        if (dashboardResponse is not null)
        {
            _totalDocuments = dashboardResponse.TotalDocuments;
            _totalPurchaseOrders = dashboardResponse.TotalPurchaseOrders;
            _totalPendingDocuments = dashboardResponse.TotalPendingsDocuments;
        }
    }
}
```

## Extensión para Nuevos Servicios

Para agregar un nuevo servicio (ej: Documents), sigue estos pasos:

### 1. Crear la Interfaz del Servicio

**Archivo**: `Services/IDocumentsService.cs`

```csharp
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document-related operations.
/// </summary>
public interface IDocumentsService
{
    /// <summary>
    /// Gets a document by ID.
    /// </summary>
    Task<DocumentResponse?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of documents.
    /// </summary>
    Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new document.
    /// </summary>
    Task<DocumentResponse?> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default);
}
```

### 2. Crear la Implementación del Servicio

**Archivo**: `Services/DocumentsService.cs`

```csharp
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document-related operations.
/// </summary>
internal sealed class DocumentsService : IDocumentsService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    public DocumentsService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService 
            ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    public async Task<DocumentResponse?> GetDocumentAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"/api/{_apiSettings.Version}/Documents/{id}";
        return await _httpClientService.GetAsync<DocumentResponse>(
            requestUri, 
            cancellationToken);
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"/api/{_apiSettings.Version}/Documents";
        IEnumerable<DocumentResponse>? documents = 
            await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
                requestUri, 
                cancellationToken);
        
        return documents ?? Enumerable.Empty<DocumentResponse>();
    }

    public async Task<DocumentResponse?> CreateDocumentAsync(
        CreateDocumentRequest request, 
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"/api/{_apiSettings.Version}/Documents";
        return await _httpClientService.PostAsync<CreateDocumentRequest, DocumentResponse>(
            requestUri, 
            request, 
            cancellationToken);
    }
}
```

### 3. Registrar el Servicio

**Archivo**: `Extensions/ServiceCollectionExtensions.cs`

```csharp
// En el método AddApiClient, agregar:
services.AddScoped<IDocumentsService, DocumentsService>();
```

### 4. Usar en el Componente

**Archivo**: `Components/Pages/Documents.razor.cs`

```csharp
[Inject] private IDocumentsService DocumentsService { get; set; } = default!;

private async Task LoadDocumentsAsync()
{
    var documents = await DocumentsService.GetDocumentsAsync();
    // ...
}
```

## Configuración

### Configuración de la API

La configuración se encuentra en los archivos JSON:

**Archivo**: `Configurations/jsons/web-api.json` (o `web-api.Development.json`)

```json
{
  "PreloadApi": {
    "BaseUrl": "https://localhost:7210",
    "Version": "v1"
  }
}
```

### Registro de Servicios

**Archivo**: `Program.cs`

```csharp
// Add API client service
builder.Services.AddApiClient(builder.Configuration);
```

**Archivo**: `Extensions/ServiceCollectionExtensions.cs`

El método `AddApiClient` registra:
- `AuthenticationDelegatingHandler` como Transient
- `IHttpClientService` / `HttpClientService` con HttpClientFactory
- Todos los servicios de dominio (Dashboard, Documents, etc.)

## Ventajas

### 1. Separación de Responsabilidades

- **DelegatingHandler**: Solo maneja autenticación
- **HttpClientService**: Solo maneja HTTP y serialización
- **Domain Services**: Solo manejan lógica de negocio y construcción de URIs

### 2. Sin Repetición de Código

El token se agrega automáticamente, no necesitas escribir esto en cada método:

```csharp
// ❌ ANTES (código repetitivo)
string? accessToken = await httpContext.GetTokenAsync(...);
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

// ✅ AHORA (automático)
// El token se agrega automáticamente por el DelegatingHandler
```

### 3. Fácil de Testear

```csharp
// Mock fácil de IHttpClientService
var mockHttpClientService = new Mock<IHttpClientService>();
mockHttpClientService
    .Setup(x => x.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new DashboardResponse(10, 5, 3));

// Mock fácil de IDashboardService
var mockDashboardService = new Mock<IDashboardService>();
mockDashboardService
    .Setup(x => x.GetDashboardAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(new DashboardResponse(10, 5, 3));
```

### 4. Escalable

Agregar un nuevo servicio es simple:
1. Crear interfaz y implementación
2. Registrar en `ServiceCollectionExtensions`
3. Usar en componentes

### 5. Mantenible

- Cambios en autenticación → Solo afectan `AuthenticationDelegatingHandler`
- Cambios en serialización → Solo afectan `HttpClientService`
- Cambios en endpoints → Solo afectan los servicios de dominio

### 6. Type-Safe

Uso de genéricos para type-safety en tiempo de compilación:

```csharp
// El compilador verifica que DashboardResponse existe
DashboardResponse? response = await _httpClientService.GetAsync<DashboardResponse>(...);
```

## Manejo de Errores

El `HttpClientService` maneja automáticamente:

- **401 Unauthorized**: Lanza `UnauthorizedAccessException` con mensaje descriptivo
- **404 Not Found**: Retorna `null` (para métodos que retornan objetos)
- **Otros errores**: Llama a `EnsureSuccessStatusCode()` que lanza la excepción apropiada

**Ejemplo de manejo en componentes**:

```csharp
try
{
    var data = await DashboardService.GetDashboardAsync();
}
catch (UnauthorizedAccessException)
{
    // Redirigir a login o mostrar mensaje
}
catch (HttpRequestException ex)
{
    // Manejar otros errores HTTP
}
```

## Consideraciones

### Blazor Server vs WebAssembly

Esta implementación está diseñada para **Blazor Server**:
- Usa `IHttpContextAccessor` para acceder al `HttpContext`
- El token se obtiene de las cookies de autenticación
- El `HttpContext` está disponible en el servidor

Para **Blazor WebAssembly**, se necesitaría:
- Usar `HttpClient` configurado con `AuthorizationMessageHandler` de `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
- Obtener el token desde `IAccessTokenProvider`

### Performance

- `JsonSerializerOptions` está cacheado como `static readonly` para evitar recreación
- `HttpClient` se crea mediante `HttpClientFactory` (mejores prácticas)
- `DelegatingHandler` es Transient (se crea por petición)

## Referencias

- [Microsoft Docs: HttpClientFactory](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [Microsoft Docs: DelegatingHandler](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler)
- [Microsoft Docs: Blazor Server Authentication](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-9.0)

---

**Última actualización**: Diciembre 2024  
**Autor**: Implementación por Remberto Aguilar

