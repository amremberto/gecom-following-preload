# Casos de Uso - Providers

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Provider`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllProviders](#getallproviders)
  - [GetProviderById](#getproviderbyid)
  - [GetProviderByCuit](#getproviderbycuit)
  - [SearchProviders](#searchproviders)
- [Commands (Comandos)](#commands-comandos)
  - _(Pendiente de implementar)_

---

## Queries (Consultas)

### GetAllProviders

**Descripción:** Obtiene todos los proveedores sin paginación.

**Archivos:**
- `GetAllProvidersQuery.cs`
- `GetAllProvidersQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Providers
```

**Respuestas:**
- `200 OK` - Lista de proveedores
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Providers
```

**Respuesta exitosa:**
```json
[
  {
    "provId": 1,
    "cuit": "20123456789",
    "razonSocial": "Proveedor S.A.",
    "mail": "contacto@proveedor.com",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null
  }
]
```

---

### GetProviderById

**Descripción:** Obtiene un proveedor por su ID.

**Archivos:**
- `GetProviderByIdQuery.cs`
- `GetProviderByIdQueryHandler.cs`
- `GetProviderByIdValidator.cs`

**Endpoint:**
```
GET /api/v1/Providers/id/{id}
```

**Parámetros:**
- `id` (path) - ID del proveedor (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Proveedor encontrado
- `400 BadRequest` - ID inválido
- `404 NotFound` - Proveedor no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Providers/id/1
```

**Respuesta exitosa:**
```json
{
  "provId": 1,
  "cuit": "20123456789",
  "razonSocial": "Proveedor S.A.",
  "mail": "contacto@proveedor.com",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Provider.NotFound",
  "status": 404,
  "detail": "Provider with ID '1' was not found.",
  "instance": "/api/v1/Providers/id/1"
}
```

---

### GetProviderByCuit

**Descripción:** Obtiene un proveedor por su CUIT.

**Archivos:**
- `GetProviderByCuitQuery.cs`
- `GetProviderByCuitQueryHandler.cs`
- `GetProviderByCuitValidator.cs`

**Endpoint:**
```
GET /api/v1/Providers/cuit/{cuit}
```

**Parámetros:**
- `cuit` (path) - CUIT del proveedor (requerido, no vacío)

**Respuestas:**
- `200 OK` - Proveedor encontrado
- `400 BadRequest` - CUIT inválido
- `404 NotFound` - Proveedor no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Providers/cuit/20123456789
```

**Respuesta exitosa:**
```json
{
  "provId": 1,
  "cuit": "20123456789",
  "razonSocial": "Proveedor S.A.",
  "mail": "contacto@proveedor.com",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Provider.NotFound",
  "status": 404,
  "detail": "Provider with CUIT '20123456789' was not found.",
  "instance": "/api/v1/Providers/cuit/20123456789"
}
```

**Nota:** El CUIT es la clave primaria de la entidad Provider en la base de datos, por lo que este método es más eficiente que buscar por ID.

---

### SearchProviders

**Descripción:** Busca proveedores por texto de búsqueda. La búsqueda se realiza sobre el CUIT y la razón social del proveedor.

**Archivos:**
- `SearchProvidersQuery.cs`
- `SearchProvidersQueryHandler.cs`
- `SearchProvidersValidator.cs`

**Endpoint:**
```
GET /api/v1/Providers/search?searchText={searchText}
```

**Parámetros:**
- `searchText` (query, requerido) - Texto de búsqueda (no vacío)

**Respuestas:**
- `200 OK` - Lista de proveedores que coinciden con el criterio de búsqueda
- `400 BadRequest` - Texto de búsqueda inválido o vacío
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Providers/search?searchText=Proveedor
```

**Respuesta exitosa:**
```json
[
  {
    "provId": 1,
    "cuit": "20123456789",
    "razonSocial": "Proveedor S.A.",
    "mail": "contacto@proveedor.com",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null
  },
  {
    "provId": 2,
    "cuit": "20234567890",
    "razonSocial": "Proveedor y Asociados",
    "mail": "info@proveedor.com",
    "fechaCreacion": "2024-01-02T00:00:00Z",
    "fechaBaja": null
  }
]
```

**Nota:** 
- La búsqueda se realiza sobre los campos `Cuit` y `RazonSocial` del proveedor
- La búsqueda es case-insensitive (no distingue mayúsculas/minúsculas)
- Se retornan máximo 20 resultados por defecto
- Los resultados se ordenan por razón social y luego por CUIT

---

## Commands (Comandos)

_(Pendiente de implementar)_

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Providers` | Obtener todos los proveedores | 200, 500 |
| GET | `/api/v1/Providers/id/{id}` | Obtener proveedor por ID | 200, 400, 404, 500 |
| GET | `/api/v1/Providers/cuit/{cuit}` | Obtener proveedor por CUIT | 200, 400, 404, 500 |
| GET | `/api/v1/Providers/search?searchText={searchText}` | Buscar proveedores por texto | 200, 400, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Providers/{NombreCasoDeUso}/
├── {NombreCasoDeUso}Query.cs (o Command.cs)
├── {NombreCasoDeUso}QueryHandler.cs (o CommandHandler.cs)
└── {NombreCasoDeUso}Validator.cs (opcional)
```

### Validaciones

- Todos los validadores usan **FluentValidation**
- Se registran automáticamente con MediatR
- Validaciones comunes:
  - IDs: Mayor que 0
  - Strings: No vacíos
  - CUIT: Requerido, no vacío

### Manejo de Errores

- Todos los handlers retornan `Result<T>` o `Result`
- Errores comunes:
  - `Error.NotFound` - Recurso no encontrado (404)
  - `Error.Conflict` - Conflicto de datos (409)
  - `Error.Validation` - Errores de validación (400)
  - `Error.Failure` - Errores del servidor (500)

### Respuestas HTTP

- **GET** con datos: `200 OK` con el recurso
- **GET** paginado: `200 OK` con `PagedResponse<T>`
- **POST**: `201 Created` con el recurso creado
- **PUT**: `200 OK` con el recurso actualizado
- **DELETE**: `204 NoContent` sin cuerpo
- **Errores**: `ProblemDetails` (RFC 7807) con el código apropiado

### Uso de ResultExtensions.Match

Todos los endpoints usan los métodos de extensión `Match` para simplificar el manejo de resultados:

```csharp
// GET con datos
return result.Match(this); // Ok(200) o Problem
```

---

## DTOs Utilizados

### Request DTOs

_(Pendiente de implementar)_

### Response DTOs

- `ProviderResponse` - Respuesta estándar de Provider

**Campos del ProviderResponse:**
- `provId` (int) - Identificador único del proveedor
- `cuit` (string) - CUIT del proveedor (clave primaria)
- `razonSocial` (string) - Razón social del proveedor
- `mail` (string) - Correo electrónico del proveedor
- `fechaCreacion` (DateTime) - Fecha de creación del proveedor
- `fechaBaja` (DateTime?) - Fecha de baja del proveedor (nullable)

---

## Notas Técnicas

### Repositorio

El repositorio `ProviderRepository` implementa `IProviderRepository` y extiende `GenericRepository<Provider, PreloadDbContext>`. Incluye:

- **GetByCuitAsync**: Método específico para obtener un proveedor por su CUIT (clave primaria)
- **GetPagedAsync**: Método sobrescrito para paginación, ordenado por `ProvId`

**Estructura del repositorio:**
```csharp
internal sealed class ProviderRepository : GenericRepository<Provider, PreloadDbContext>, IProviderRepository
{
    public async Task<Provider?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);
        return await GetQueryable()
            .FirstOrDefaultAsync(p => p.Cuit == cuit, cancellationToken);
    }

    public override async Task<(IReadOnlyList<Provider> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Ordena por ProvId
        // ...
    }
}
```

**Optimización de rendimiento:**
- Se utiliza `.AsNoTracking()` implícitamente a través de `GetQueryable()` para mejorar el rendimiento en operaciones de solo lectura
- El método `GetByCuitAsync` utiliza la clave primaria (CUIT) para búsquedas eficientes

### Mapeo

Todos los casos de uso usan `ProviderMappings.ToResponse` para mapear de la entidad de dominio `Provider` a `ProviderResponse`:

```csharp
internal static class ProviderMappings
{
    public static ProviderResponse ToResponse(Provider provider)
    {
        return new ProviderResponse(
            provider.ProvId,
            provider.Cuit,
            provider.RazonSocial,
            provider.Mail,
            provider.FechaCreacion,
            provider.FechaBaja
        );
    }
}
```

### Clave Primaria

La entidad `Provider` utiliza `Cuit` como clave primaria en la base de datos (no `ProvId`), aunque `ProvId` es un identificador único generado automáticamente. Esto significa que:

- `GetByCuitAsync` es más eficiente que `GetByIdAsync` para búsquedas por CUIT
- `GetByIdAsync` utiliza `ProvId` (que no es la clave primaria) y puede ser menos eficiente
- Se recomienda usar `GetByCuitAsync` cuando se conoce el CUIT del proveedor

### Relaciones

La entidad `Provider` tiene una relación con `Document` a través de la colección `Documents`. Sin embargo, los casos de uso actuales no incluyen estas relaciones por defecto. Si se necesita incluir los documentos asociados, se puede:

1. Agregar un método específico en el repositorio con `Include(d => d.Documents)`
2. Crear un endpoint específico que retorne proveedores con sus documentos
3. Usar un DTO diferente que incluya la información de documentos

---

### Controlador

El controlador `ProvidersController` está ubicado en `WebApi/Controllers/V1/ProvidersController.cs` y expone los siguientes endpoints:

- **GET** `/api/v1/Providers` - Obtiene todos los proveedores
- **GET** `/api/v1/Providers/id/{id}` - Obtiene un proveedor por su ID
- **GET** `/api/v1/Providers/cuit/{cuit}` - Obtiene un proveedor por su CUIT
- **GET** `/api/v1/Providers/search?searchText={searchText}` - Busca proveedores por texto

**Autenticación:**
- Todos los endpoints requieren autenticación mediante el atributo `[Authorize]` aplicado a nivel de controlador
- No se requieren políticas específicas adicionales para los endpoints de consulta

**Swagger/OpenAPI:**
- Todos los endpoints están documentados con XML comments para Swagger
- Incluyen ejemplos de respuestas y códigos de estado HTTP

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado GetAllProviders - Query para obtener todos los proveedores
- **2024-12-19**: Implementado GetProviderById - Query para obtener un proveedor por su ID
- **2024-12-19**: Implementado GetProviderByCuit - Query para obtener un proveedor por su CUIT
- **2024-12-19**: Creado ProviderResponse DTO en Contracts
- **2024-12-19**: Creado IProviderRepository interface y ProviderRepository implementation
- **2024-12-19**: Creado ProviderMappings para mapeo de entidad a DTO
- **2024-12-19**: Registrado ProviderRepository en InfrastructureDependencyInjection
- **2024-12-19**: Creado ProvidersController en WebApi con endpoints GET para consultas
- **2024-12-19**: Implementado endpoint Search en ProvidersController para búsqueda de proveedores por texto

