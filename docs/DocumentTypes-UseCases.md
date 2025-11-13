# Casos de Uso - DocumentTypes

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `DocumentType`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllDocumentTypes](#getalldocumenttypes)
  - [GetDocumentTypeById](#getdocumenttypebyid)
  - [GetDocumentTypeByCode](#getdocumenttypebycode)
- [Commands (Comandos)](#commands-comandos)
  - [CreateDocumentType](#createdocumenttype)
  - [UpdateDocumentType](#updatedocumenttype)
  - [DeleteDocumentType](#deletedocumenttype)

---

## Queries (Consultas)

### GetAllDocumentTypes

**Descripción:** Obtiene todos los tipos de documento sin paginación.

**Archivos:**
- `GetAllDocumentTypesQuery.cs`
- `GetAllDocumentTypesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/DocumentTypes
```

**Respuestas:**
- `200 OK` - Lista de tipos de documento
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/DocumentTypes
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "tipoDocId": 1,
    "descripcion": "Factura A",
    "letra": "A",
    "codigo": "001",
    "descripcionLarga": "Factura A - Comprobante Fiscal",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null,
    "isFec": true
  },
  {
    "tipoDocId": 2,
    "descripcion": "Factura B",
    "letra": "B",
    "codigo": "006",
    "descripcionLarga": "Factura B - Comprobante Fiscal",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null,
    "isFec": true
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`). Los campos `letra` y `descripcionLarga` son opcionales y pueden ser null.

---

### GetDocumentTypeById

**Descripción:** Obtiene un tipo de documento por su ID.

**Archivos:**
- `GetDocumentTypeByIdQuery.cs`
- `GetDocumentTypeByIdQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/DocumentTypes/id/{tipoDocId}
```

**Parámetros:**
- `tipoDocId` (path) - ID del tipo de documento (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Tipo de documento encontrado
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Tipo de documento no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/DocumentTypes/id/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "tipoDocId": 1,
  "descripcion": "Factura A",
  "letra": "A",
  "codigo": "001",
  "descripcionLarga": "Factura A - Comprobante Fiscal",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "isFec": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "DocumentType.NotFound",
  "status": 404,
  "detail": "Document type with ID '1' was not found.",
  "instance": "/api/v1/DocumentTypes/id/1"
}
```

---

### GetDocumentTypeByCode

**Descripción:** Obtiene un tipo de documento por su código.

**Archivos:**
- `GetDocumentTypeByCodeQuery.cs`
- `GetDocumentTypeByCodeQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/DocumentTypes/code/{codigo}
```

**Parámetros:**
- `codigo` (path) - Código del tipo de documento (requerido, no vacío, máximo 4 caracteres)

**Respuestas:**
- `200 OK` - Tipo de documento encontrado
- `400 BadRequest` - Código inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Tipo de documento no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/DocumentTypes/code/001
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "tipoDocId": 1,
  "descripcion": "Factura A",
  "letra": "A",
  "codigo": "001",
  "descripcionLarga": "Factura A - Comprobante Fiscal",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "isFec": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "DocumentType.NotFound",
  "status": 404,
  "detail": "Document type with code '001' was not found.",
  "instance": "/api/v1/DocumentTypes/code/001"
}
```

---

## Commands (Comandos)

### CreateDocumentType

**Descripción:** Crea un nuevo tipo de documento.

**Archivos:**
- `CreateDocumentTypeCommand.cs`
- `CreateDocumentTypeCommandHandler.cs`
- `CreateDocumentTypeCommandValidator.cs`

**DTO de Request:**
- `CreateDocumentTypeRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/DocumentTypes
```

**Body (JSON):**
```json
{
  "descripcion": "Factura C",
  "letra": "C",
  "codigo": "011",
  "descripcionLarga": "Factura C - Comprobante Fiscal",
  "isFec": true
}
```

**Validaciones:**
- `descripcion` - Requerido, no vacío, máximo 90 caracteres
- `codigo` - Requerido, no vacío, máximo 4 caracteres
- `letra` - Opcional, máximo 1 carácter (si se proporciona)
- `descripcionLarga` - Opcional, máximo 90 caracteres (si se proporciona)
- `isFec` - Requerido (boolean)
- Verifica que no exista otro tipo de documento con el mismo código

**Respuestas:**
- `201 Created` - Tipo de documento creado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `409 Conflict` - Tipo de documento con mismo código ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/DocumentTypes
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Factura C",
  "letra": "C",
  "codigo": "011",
  "descripcionLarga": "Factura C - Comprobante Fiscal",
  "isFec": true
}
```

**Respuesta exitosa (201):**
```json
{
  "tipoDocId": 3,
  "descripcion": "Factura C",
  "letra": "C",
  "codigo": "011",
  "descripcionLarga": "Factura C - Comprobante Fiscal",
  "fechaCreacion": "2024-01-15T12:00:00Z",
  "fechaBaja": null,
  "isFec": true
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "DocumentType.Conflict",
  "status": 409,
  "detail": "A document type with code '011' already exists.",
  "instance": "/api/v1/DocumentTypes"
}
```

**Nota:** El handler establece automáticamente `FechaCreacion = DateTime.UtcNow` al crear el tipo de documento. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### UpdateDocumentType

**Descripción:** Actualiza un tipo de documento existente.

**Archivos:**
- `UpdateDocumentTypeCommand.cs`
- `UpdateDocumentTypeCommandHandler.cs`
- `UpdateDocumentTypeCommandValidator.cs`

**DTO de Request:**
- `UpdateDocumentTypeRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/DocumentTypes/{tipoDocId}
```

**Parámetros:**
- `tipoDocId` (path) - ID del tipo de documento a actualizar

**Body (JSON):**
```json
{
  "descripcion": "Factura C Actualizada",
  "letra": "C",
  "codigo": "011",
  "descripcionLarga": "Factura C - Comprobante Fiscal Actualizado",
  "isFec": true
}
```

**Validaciones:**
- `tipoDocId` - Requerido, mayor que 0
- `descripcion` - Requerido, no vacío, máximo 90 caracteres
- `codigo` - Requerido, no vacío, máximo 4 caracteres
- `letra` - Opcional, máximo 1 carácter (si se proporciona)
- `descripcionLarga` - Opcional, máximo 90 caracteres (si se proporciona)
- `isFec` - Requerido (boolean)
- Verifica que el tipo de documento exista
- Verifica que no exista otro tipo de documento con el mismo código (excluyendo el actual)

**Respuestas:**
- `200 OK` - Tipo de documento actualizado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Tipo de documento no encontrado
- `409 Conflict` - Tipo de documento con mismo código ya existe (excluyendo el actual)
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/DocumentTypes/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Factura A Actualizada",
  "letra": "A",
  "codigo": "001",
  "descripcionLarga": "Factura A - Comprobante Fiscal Actualizado",
  "isFec": true
}
```

**Respuesta exitosa (200):**
```json
{
  "tipoDocId": 1,
  "descripcion": "Factura A Actualizada",
  "letra": "A",
  "codigo": "001",
  "descripcionLarga": "Factura A - Comprobante Fiscal Actualizado",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "isFec": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "DocumentType.NotFound",
  "status": 404,
  "detail": "Document type with ID '1' was not found.",
  "instance": "/api/v1/DocumentTypes/1"
}
```

**Nota:** El handler excluye el tipo de documento actual del chequeo de conflictos, permitiendo actualizaciones sin cambiar el código. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### DeleteDocumentType

**Descripción:** Elimina un tipo de documento por su ID.

**Archivos:**
- `DeleteDocumentTypeCommand.cs`
- `DeleteDocumentTypeCommandHandler.cs`

**Endpoint:**
```
DELETE /api/v1/DocumentTypes/{tipoDocId}
```

**Parámetros:**
- `tipoDocId` (path) - ID del tipo de documento (debe ser mayor que 0)

**Validaciones:**
- `tipoDocId` - Requerido, mayor que 0
- Verifica que el tipo de documento exista

**Respuestas:**
- `204 NoContent` - Tipo de documento eliminado exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Tipo de documento no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/DocumentTypes/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "DocumentType.NotFound",
  "status": 404,
  "detail": "Document type with ID '1' was not found.",
  "instance": "/api/v1/DocumentTypes/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar `FechaBaja` en lugar de eliminar), se puede refactorizar el handler. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

**Advertencia:** Eliminar un tipo de documento puede afectar documentos que lo referencian. Se recomienda verificar las relaciones antes de eliminar o implementar soft delete.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/DocumentTypes` | Obtener todos los tipos de documento | 200, 401, 403, 500 |
| GET | `/api/v1/DocumentTypes/id/{tipoDocId}` | Obtener tipo de documento por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/DocumentTypes/code/{codigo}` | Obtener tipo de documento por código | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/DocumentTypes` | Crear un nuevo tipo de documento | 201, 400, 401, 403, 409, 500 |
| PUT | `/api/v1/DocumentTypes/{tipoDocId}` | Actualizar un tipo de documento | 200, 400, 401, 403, 404, 409, 500 |
| DELETE | `/api/v1/DocumentTypes/{tipoDocId}` | Eliminar un tipo de documento | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/DocumentTypes/{NombreCasoDeUso}/
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
  - Longitudes máximas según configuración de EF Core:
    - `Descripcion`: 90 caracteres
    - `Codigo`: 4 caracteres
    - `Letra`: 1 carácter
    - `DescripcionLarga`: 90 caracteres
  - Código único: Verificado en Create y Update

### Manejo de Errores

- Todos los handlers retornan `Result<T>` o `Result`
- Errores comunes:
  - `Error.NotFound` - Recurso no encontrado (404)
  - `Error.Conflict` - Conflicto de datos (409)
  - `Error.Validation` - Errores de validación (400)
  - `Error.Failure` - Errores del servidor (500)

### Respuestas HTTP

- **GET** con datos: `200 OK` con el recurso
- **POST**: `201 Created` con el recurso creado
- **PUT**: `200 OK` con el recurso actualizado
- **DELETE**: `204 NoContent` sin cuerpo
- **Errores**: `ProblemDetails` (RFC 7807) con el código apropiado

### Uso de ResultExtensions.Match

Todos los endpoints usan los métodos de extensión `Match` para simplificar el manejo de resultados:

```csharp
// GET con datos
return result.Match(this); // Ok(200) o Problem

// POST
return result.MatchCreated(this, nameof(GetByCodeAsync), new { codigo = request.Codigo }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateDocumentTypeRequest` - Para crear tipos de documento
  - `Descripcion` (string, requerido) - Descripción del tipo de documento (máx. 90 caracteres)
  - `Letra` (string?, opcional) - Letra del tipo de documento (máx. 1 carácter)
  - `Codigo` (string, requerido) - Código único del tipo de documento (máx. 4 caracteres)
  - `DescripcionLarga` (string?, opcional) - Descripción extendida (máx. 90 caracteres)
  - `IsFec` (bool, requerido) - Indica si es un documento FEC (Fiscal Electrónico)
- `UpdateDocumentTypeRequest` - Para actualizar tipos de documento
  - `Descripcion` (string, requerido) - Descripción del tipo de documento (máx. 90 caracteres)
  - `Letra` (string?, opcional) - Letra del tipo de documento (máx. 1 carácter)
  - `Codigo` (string, requerido) - Código único del tipo de documento (máx. 4 caracteres)
  - `DescripcionLarga` (string?, opcional) - Descripción extendida (máx. 90 caracteres)
  - `IsFec` (bool, requerido) - Indica si es un documento FEC (Fiscal Electrónico)

### Response DTOs

- `DocumentTypeResponse` - Respuesta estándar de DocumentType
  - `TipoDocId` (int) - Identificador único del tipo de documento
  - `Descripcion` (string) - Descripción del tipo de documento
  - `Letra` (string?) - Letra del tipo de documento (nullable)
  - `Codigo` (string) - Código único del tipo de documento
  - `DescripcionLarga` (string?) - Descripción extendida del tipo de documento (nullable)
  - `FechaCreacion` (DateTime) - Fecha de creación del tipo de documento
  - `FechaBaja` (DateTime?) - Fecha de baja del tipo de documento (nullable)
  - `IsFec` (bool) - Indica si es un documento FEC (Fiscal Electrónico)

---

## Notas Técnicas

### Repositorio

El repositorio `DocumentTypeRepository` implementa `IDocumentTypeRepository` y extiende `GenericRepository<DocumentType, PreloadDbContext>`. Incluye:

- **GetByCodeAsync**: Método específico para obtener un tipo de documento por su código
- **GetByTipoDocIdAsync**: Método específico para obtener un tipo de documento por su ID
- **GetByIdAsync**: Método sobrescrito porque DocumentType usa `TipoDocId` (int) como clave primaria

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `DocumentTypeMappings.ToResponse` para mapear de la entidad de dominio `DocumentType` a `DocumentTypeResponse`.

### Validación de Código Único

Tanto `CreateDocumentTypeCommandHandler` como `UpdateDocumentTypeCommandHandler` verifican que no exista otro tipo de documento con el mismo código:

- En **Create**: Verifica que no exista ningún tipo de documento con el código proporcionado
- En **Update**: Verifica que no exista otro tipo de documento con el código proporcionado, excluyendo el tipo de documento actual

### Autorización

- **GET endpoints**: Requieren `RequirePreloadRead` (permite lectura a usuarios con roles: Administrator, PreloadReadOnly, PreloadAllSocieties, PreloadSingleSociety)
- **POST/PUT/DELETE endpoints**: Requieren `RequireAdministrator` (solo usuarios con rol Administrator)

### Relaciones

La entidad `DocumentType` tiene las siguientes relaciones:

- **Documents** - Colección de documentos que usan este tipo de documento

**Nota:** Al eliminar un tipo de documento, se debe considerar el impacto en los documentos que lo referencian. Se recomienda implementar soft delete o validar relaciones antes de eliminar.

### Campos Opcionales

A diferencia de otras entidades como `State`, `DocumentType` tiene varios campos opcionales:

- `Letra` - Puede ser null
- `DescripcionLarga` - Puede ser null

Estos campos se validan solo si se proporcionan (usando `.When(x => x.Letra is not null)` en FluentValidation).

### Campo IsFec

El campo `IsFec` (IsFEC en la base de datos) indica si el tipo de documento es un documento FEC (Fiscal Electrónico). Este campo es requerido y se debe proporcionar tanto en Create como en Update.

---

## Fecha de Creación

Documento creado: 2024-11-13

**Última actualización:** 2024-11-13

### Cambios Recientes

- **2024-11-13**: Implementado CRUD completo para DocumentTypes
  - GetAllDocumentTypes - Query para obtener todos los tipos de documento
  - GetDocumentTypeById - Query para obtener un tipo de documento por ID
  - GetDocumentTypeByCode - Query para obtener un tipo de documento por código
  - CreateDocumentType - Command para crear un nuevo tipo de documento
  - UpdateDocumentType - Command para actualizar un tipo de documento existente
  - DeleteDocumentType - Command para eliminar un tipo de documento
- **2024-11-13**: Agregado DocumentTypeResponse DTO y DocumentTypeMappings
- **2024-11-13**: Implementada validación de código único en Create y Update
- **2024-11-13**: Agregado DocumentTypesController con todos los endpoints
- **2024-11-13**: Configurada autorización (RequirePreloadRead para GET, RequireAdministrator para POST/PUT/DELETE)
- **2024-11-13**: Implementada validación de campos opcionales (Letra, DescripcionLarga) con FluentValidation

