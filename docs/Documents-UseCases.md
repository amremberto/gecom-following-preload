# Casos de Uso - Documents

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Document`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllDocuments](#getalldocuments)
  - [GetDocumentsByEmissionDatesAndProvider](#getdocumentsbyemissiondatesandprovider)
  - [GetPendingDocumentsByProvider](#getpendingdocumentsbyprovider)
- [Commands (Comandos)](#commands-comandos)
  - _(Pendiente de implementar)_

---

## Queries (Consultas)

### GetAllDocuments

**Descripción:** Obtiene todos los documentos sin paginación.

**Archivos:**
- `GetAllDocumentsQuery.cs`
- `GetAllDocumentsQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Documents
```

**Respuestas:**
- `200 OK` - Lista de documentos
- `401 Unauthorized` - Usuario no autenticado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Documents
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "docId": 1,
    "proveedorCuit": "20123456789",
    "proveedorRazonSocial": "Proveedor S.A.",
    "sociedadCuit": "30123456789",
    "sociedadDescripcion": "Sociedad Ejemplo",
    "tipoDocId": 1,
    "tipoDocDescripcion": "Factura A",
    "puntoDeVenta": "0001",
    "numeroComprobante": "00001234",
    "fechaEmisionComprobante": "2024-01-15",
    "moneda": "ARS",
    "montoBruto": 100000.50,
    "codigoDeBarras": "1234567890123",
    "caecai": "12345678901234",
    "vencimientoCaecai": "2024-12-31",
    "estadoId": 1,
    "estadoDescripcion": "Pendiente",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "fechaBaja": null,
    "idDocument": 123,
    "nombreSolicitante": "Juan Pérez",
    "idDetalleDePago": 1,
    "idMetodoDePago": 1,
    "fechaPago": "2024-01-20T14:00:00Z",
    "userCreate": "admin@example.com",
    "purchaseOrders": [
      {
        "ocid": 1,
        "codigoRecepcion": "REC001",
        "cantidadAfacturar": 10.00,
        "docId": 1,
        "ordenCompraId": 100,
        "fechaCreacion": "2024-01-10T08:00:00Z",
        "fechaBaja": null,
        "nroOc": "OC-2024-001",
        "posicionOc": 1,
        "codigoSociedadFi": "SOC001",
        "proveedorSap": "PROV001"
      }
    ],
    "notes": [
      {
        "notaId": 1,
        "descripcion": "Nota de ejemplo",
        "usuarioCreacion": "admin@example.com",
        "fechaCreacion": "2024-01-15T11:00:00Z",
        "docId": 1
      }
    ]
  }
]
```

**Nota:** Este endpoint requiere autenticación. Todos los campos son opcionales (nullable) excepto `docId` que es el identificador único del documento. El endpoint incluye automáticamente las relaciones con `Provider`, `Society`, `DocumentType`, `State`, `PurchaseOrders` y `Notes` para optimizar las consultas.

---

### GetDocumentsByEmissionDatesAndProvider

**Descripción:** Obtiene documentos filtrados por rango de fechas de emisión y opcionalmente por CUIT del proveedor. Si no se proporciona el CUIT del proveedor, retorna todos los documentos de todos los proveedores en el rango de fechas especificado.

**Archivos:**
- `GetDocumentsByEmissionDatesAndProviderQuery.cs`
- `GetDocumentsByEmissionDatesAndProviderQueryHandler.cs`
- `GetDocumentsByEmissionDatesAndProviderValidator.cs`

**Endpoint:**
```
GET /api/v1/Documents/by-dates-and-provider?dateFrom={dateFrom}&dateTo={dateTo}&providerCuit={providerCuit}
```

**Parámetros:**
- `dateFrom` (requerido) - Fecha de inicio del rango (DateOnly, formato: YYYY-MM-DD)
- `dateTo` (requerido) - Fecha de fin del rango (DateOnly, formato: YYYY-MM-DD)
- `providerCuit` (opcional) - CUIT del proveedor (string). Si no se proporciona, retorna documentos de todos los proveedores.

**Respuestas:**
- `200 OK` - Lista de documentos que cumplen los criterios
- `400 BadRequest` - Parámetros inválidos (fechas inválidas, dateTo menor que dateFrom)
- `401 Unauthorized` - Usuario no autenticado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso con proveedor:**
```http
GET /api/v1/Documents/by-dates-and-provider?dateFrom=2024-01-01&dateTo=2024-12-31&providerCuit=20123456789
Authorization: Bearer {token}
```

**Ejemplo de uso sin proveedor (todos los proveedores):**
```http
GET /api/v1/Documents/by-dates-and-provider?dateFrom=2024-01-01&dateTo=2024-12-31
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "docId": 1,
    "proveedorCuit": "20123456789",
    "proveedorRazonSocial": "Proveedor S.A.",
    "sociedadCuit": "30123456789",
    "sociedadDescripcion": "Sociedad Ejemplo",
    "tipoDocId": 1,
    "tipoDocDescripcion": "Factura A",
    "puntoDeVenta": "0001",
    "numeroComprobante": "00001234",
    "fechaEmisionComprobante": "2024-01-15",
    "moneda": "ARS",
    "montoBruto": 100000.50,
    "codigoDeBarras": "1234567890123",
    "caecai": "12345678901234",
    "vencimientoCaecai": "2024-12-31",
    "estadoId": 1,
    "estadoDescripcion": "Pendiente",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "fechaBaja": null,
    "idDocument": 123,
    "nombreSolicitante": "Juan Pérez",
    "idDetalleDePago": 1,
    "idMetodoDePago": 1,
    "fechaPago": "2024-01-20T14:00:00Z",
    "userCreate": "admin@example.com",
    "purchaseOrders": [],
    "notes": []
  }
]
```

**Nota:** Este endpoint requiere autenticación. Solo se retornan documentos que:
- Tengan un valor en `FechaEmisionComprobante` (no nulo)
- Su `FechaEmisionComprobante` esté dentro del rango especificado (inclusive)
- Si se proporciona `providerCuit`, su `ProveedorCuit` debe coincidir exactamente con el parámetro proporcionado
- Si no se proporciona `providerCuit`, se retornan documentos de todos los proveedores en el rango de fechas

Los resultados se ordenan por `FechaEmisionComprobante` de forma descendente (más recientes primero). El endpoint incluye automáticamente las relaciones con `Provider`, `Society`, `DocumentType`, `State`, `PurchaseOrders` y `Notes` para optimizar las consultas.

---

### GetPendingDocumentsByProvider

**Descripción:** Obtiene documentos pendientes filtrados por CUIT del proveedor. Los documentos pendientes son aquellos con `EstadoId == 2` o `EstadoId == 5` y que tienen `FechaEmisionComprobante` establecida. El CUIT del proveedor proporcionado debe coincidir con el CUIT en el token de autenticación del usuario.

**Archivos:**
- `GetPendingDocumentsByProviderQuery.cs`
- `GetPendingDocumentsByProviderQueryHandler.cs`
- `GetPendingDocumentsByProviderValidator.cs`

**Endpoint:**
```
GET /api/v1/Documents/pending-by-provider?providerCuit={providerCuit}
```

**Parámetros:**
- `providerCuit` (requerido) - CUIT del proveedor (string). Debe coincidir con el CUIT en el token de autenticación del usuario.

**Respuestas:**
- `200 OK` - Lista de documentos pendientes que cumplen los criterios
- `400 BadRequest` - Parámetros inválidos (providerCuit vacío, CUIT no coincide con el token, CUIT no encontrado en el token)
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario no tiene los permisos requeridos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Documents/pending-by-provider?providerCuit=20123456789
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "docId": 1,
    "proveedorCuit": "20123456789",
    "proveedorRazonSocial": "Proveedor S.A.",
    "sociedadCuit": "30123456789",
    "sociedadDescripcion": "Sociedad Ejemplo",
    "tipoDocId": 1,
    "tipoDocDescripcion": "Factura A",
    "puntoDeVenta": "0001",
    "numeroComprobante": "00001234",
    "fechaEmisionComprobante": "2024-01-15",
    "moneda": "ARS",
    "montoBruto": 100000.50,
    "codigoDeBarras": "1234567890123",
    "caecai": "12345678901234",
    "vencimientoCaecai": "2024-12-31",
    "estadoId": 2,
    "estadoDescripcion": "Pendiente de Aprobación",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "fechaBaja": null,
    "idDocument": 123,
    "nombreSolicitante": "Juan Pérez",
    "idDetalleDePago": 1,
    "idMetodoDePago": 1,
    "fechaPago": null,
    "userCreate": "admin@example.com",
    "purchaseOrders": [],
    "notes": []
  },
  {
    "docId": 2,
    "proveedorCuit": "20123456789",
    "proveedorRazonSocial": "Proveedor S.A.",
    "sociedadCuit": "30123456789",
    "sociedadDescripcion": "Sociedad Ejemplo",
    "tipoDocId": 1,
    "tipoDocDescripcion": "Factura A",
    "puntoDeVenta": "0001",
    "numeroComprobante": "00001235",
    "fechaEmisionComprobante": "2024-01-16",
    "moneda": "ARS",
    "montoBruto": 50000.00,
    "codigoDeBarras": "1234567890124",
    "caecai": "12345678901235",
    "vencimientoCaecai": "2024-12-31",
    "estadoId": 5,
    "estadoDescripcion": "Pendiente de Revisión",
    "fechaCreacion": "2024-01-16T10:30:00Z",
    "fechaBaja": null,
    "idDocument": 124,
    "nombreSolicitante": "María González",
    "idDetalleDePago": null,
    "idMetodoDePago": null,
    "fechaPago": null,
    "userCreate": "admin@example.com",
    "purchaseOrders": [],
    "notes": []
  }
]
```

**Nota:** Este endpoint requiere autenticación y el permiso `RequirePreloadRead`. Solo se retornan documentos que:
- Tengan un valor en `FechaEmisionComprobante` (no nulo)
- Su `ProveedorCuit` coincida exactamente con el parámetro `providerCuit` proporcionado
- Su `EstadoId` sea igual a `2` (Pendiente de Aprobación) o `5` (Pendiente de Revisión)

**Validaciones de seguridad:**
- El `providerCuit` proporcionado debe coincidir exactamente con el CUIT en el claim `SocietyCuitClaimType` del token de autenticación del usuario
- Si el CUIT no coincide, se retorna `400 BadRequest` con el mensaje "Provider CUIT does not match the CUIT in the authentication token."
- Si el CUIT no se encuentra en el token, se retorna `400 BadRequest` con el mensaje "CUIT claim not found in the authentication token."

Los resultados se ordenan por `FechaEmisionComprobante` de forma descendente (más recientes primero). El endpoint incluye automáticamente las relaciones con `Provider`, `Society`, `DocumentType`, `State`, `PurchaseOrders` y `Notes` para optimizar las consultas.

---

## Commands (Comandos)

_(Pendiente de implementar)_

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Documents` | Obtener todos los documentos | 200, 401, 500 |
| GET | `/api/v1/Documents/by-dates-and-provider` | Obtener documentos por rango de fechas de emisión y opcionalmente por proveedor | 200, 400, 401, 500 |
| GET | `/api/v1/Documents/pending-by-provider` | Obtener documentos pendientes por CUIT del proveedor | 200, 400, 401, 403, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Documents/{NombreCasoDeUso}/
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
  - Paginación: Page > 0, PageSize > 0 y <= 200

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

- `DocumentResponse` - Respuesta estándar de Document

**Campos del DocumentResponse:**
- `docId` (int) - Identificador único del documento
- `proveedorCuit` (string?) - CUIT del proveedor
- `proveedorRazonSocial` (string?) - Razón social del proveedor (obtenido de la relación Provider)
- `sociedadCuit` (string?) - CUIT de la sociedad
- `sociedadDescripcion` (string?) - Descripción de la sociedad (obtenido de la relación Society)
- `tipoDocId` (int?) - ID del tipo de documento
- `tipoDocDescripcion` (string?) - Descripción del tipo de documento (obtenido de la relación DocumentType)
- `puntoDeVenta` (string?) - Punto de venta
- `numeroComprobante` (string?) - Número de comprobante
- `fechaEmisionComprobante` (DateOnly?) - Fecha de emisión del comprobante
- `moneda` (string?) - Código de moneda
- `montoBruto` (decimal?) - Monto bruto del documento
- `codigoDeBarras` (string?) - Código de barras
- `caecai` (string?) - CAECAI (Código de Autorización Electrónico)
- `vencimientoCaecai` (DateOnly?) - Fecha de vencimiento del CAECAI
- `estadoId` (int?) - ID del estado del documento
- `estadoDescripcion` (string?) - Descripción del estado (obtenido de la relación State)
- `fechaCreacion` (DateTime?) - Fecha de creación del documento
- `fechaBaja` (DateTime?) - Fecha de baja del documento
- `idDocument` (int?) - ID del documento relacionado
- `nombreSolicitante` (string?) - Nombre del solicitante
- `idDetalleDePago` (int?) - ID del detalle de pago
- `idMetodoDePago` (int?) - ID del método de pago
- `fechaPago` (DateTime?) - Fecha de pago
- `userCreate` (string?) - Usuario que creó el documento
- `purchaseOrders` (ICollection<PurchaseOrderResponse>) - Colección de órdenes de compra asociadas
- `notes` (ICollection<NoteResponse>) - Colección de notas asociadas

**DTOs relacionados:**
- `PurchaseOrderResponse` - DTO para órdenes de compra (en `GeCom.Following.Preload.Contracts.Preload.PurchaseOrders`)
- `NoteResponse` - DTO para notas (en `GeCom.Following.Preload.Contracts.Preload.Notes`)

---

## Notas Técnicas

### Repositorio

El caso de uso utiliza `IDocumentRepository.GetAllAsync()` para obtener todos los documentos. El repositorio sobrescribe el método base para incluir las relaciones necesarias usando `Include()`:

```csharp
public override async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
{
    return await GetQueryable()
        .Include(d => d.Provider)
        .Include(d => d.Society)
        .Include(d => d.DocumentType)
        .Include(d => d.State)
        .Include(d => d.PurchaseOrders)
        .Include(d => d.Notes)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}
```

**Optimización de rendimiento:**
- Se utiliza `.AsNoTracking()` explícitamente para mejorar el rendimiento en operaciones de solo lectura
- Esto evita que EF Core rastree cambios en las entidades, reduciendo el consumo de memoria y mejorando la velocidad de las consultas
- Es especialmente importante cuando se incluyen múltiples relaciones con `Include()`

### Mapeo

Todos los casos de uso usan `DocumentMappings.ToResponse` para mapear de la entidad de dominio `Document` a `DocumentResponse`. El mapeo incluye:

- Mapeo de campos directos de la entidad
- Extracción de campos de relaciones (Provider.RazonSocial, Society.Descripcion, DocumentType.Descripcion, State.Descripcion)
- Mapeo de colecciones usando `PurchaseOrderMappings.ToResponse` y `NoteMappings.ToResponse`

### Autenticación

Todos los endpoints del controlador `DocumentsController` requieren autenticación mediante el atributo `[Authorize]` aplicado a nivel de controlador.

### Relaciones Incluidas

El endpoint `GetAllDocuments` incluye automáticamente las siguientes relaciones:

- **Provider** - Para obtener `ProveedorRazonSocial`
- **Society** - Para obtener `SociedadDescripcion`
- **DocumentType** - Para obtener `TipoDocDescripcion`
- **State** - Para obtener `EstadoDescripcion`
- **PurchaseOrders** - Colección completa de órdenes de compra asociadas
- **Notes** - Colección completa de notas asociadas

**Relaciones no incluidas por defecto:**
- `Attachments` - Colección de adjuntos
- `DocumentStates` - Colección de estados del documento
- `PaymentDetail` - Detalle de pago
- `Currency` - Moneda
- `ActionsRegisters` - Colección de registros de acciones

Si se necesita incluir estas relaciones adicionales, se pueden agregar más `Include()` en el repositorio o crear endpoints específicos.

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-20

### Cambios Recientes

- **2024-12-19**: Implementado GetPendingDocumentsByProvider - Query para obtener documentos pendientes (EstadoId == 2 o 5) por CUIT del proveedor
- **2024-12-19**: Agregado método GetPendingDocumentsByProviderCuitAsync al repositorio con filtrado por estado pendiente y Include de relaciones
- **2024-12-19**: Implementado GetDocumentsByEmissionDatesAndProvider - Query para obtener documentos por rango de fechas de emisión y opcionalmente por proveedor CUIT
- **2024-12-19**: Modificado para que el proveedor sea opcional - si no se proporciona, retorna documentos de todos los proveedores
- **2024-12-19**: Agregado método GetByEmissionDatesAndProviderCuitAsync al repositorio con Include de relaciones y AsNoTracking
- **2024-12-19**: Agregados campos de relaciones (ProveedorRazonSocial, SociedadDescripcion, TipoDocDescripcion, EstadoDescripcion)
- **2024-12-19**: Agregadas colecciones PurchaseOrders y Notes al DocumentResponse
- **2024-12-19**: Implementado Include de relaciones en el repositorio para optimizar consultas
- **2024-12-19**: Agregado AsNoTracking() explícito para mejorar rendimiento en operaciones de solo lectura

