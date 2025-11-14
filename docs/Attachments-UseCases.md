# Casos de Uso - Attachments

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Attachment`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllAttachments](#getallattachments)
  - [GetAttachmentById](#getattachmentbyid)
  - [GetAttachmentsByDocumentId](#getattachmentsbydocumentid)
- [Commands (Comandos)](#commands-comandos)
  - [CreateAttachment](#createattachment)
  - [UpdateAttachment](#updateattachment)
  - [DeleteAttachment](#deleteattachment)

---

## Queries (Consultas)

### GetAllAttachments

**Descripción:** Obtiene todos los adjuntos sin paginación, ordenados por fecha de creación descendente.

**Archivos:**
- `GetAllAttachmentsQuery.cs`
- `GetAllAttachmentsQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Attachments
```

**Respuestas:**
- `200 OK` - Lista de adjuntos
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Attachments
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "adjuntoId": 1,
    "path": "/uploads/documents/2024/01/factura_001.pdf",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "fechaModificacion": null,
    "fechaBorrado": null,
    "docId": 123
  },
  {
    "adjuntoId": 2,
    "path": "/uploads/documents/2024/01/comprobante_002.pdf",
    "fechaCreacion": "2024-01-14T14:20:00Z",
    "fechaModificacion": "2024-01-14T15:00:00Z",
    "fechaBorrado": null,
    "docId": 124
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`). Los adjuntos se ordenan por fecha de creación descendente (más recientes primero).

---

### GetAttachmentById

**Descripción:** Obtiene un adjunto por su ID.

**Archivos:**
- `GetAttachmentByIdQuery.cs`
- `GetAttachmentByIdQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Attachments/{adjuntoId}
```

**Parámetros:**
- `adjuntoId` (path) - ID del adjunto (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Adjunto encontrado
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Adjunto no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Attachments/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "adjuntoId": 1,
  "path": "/uploads/documents/2024/01/factura_001.pdf",
  "fechaCreacion": "2024-01-15T10:30:00Z",
  "fechaModificacion": null,
  "fechaBorrado": null,
  "docId": 123
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Attachment.NotFound",
  "status": 404,
  "detail": "Attachment with ID '1' was not found.",
  "instance": "/api/v1/Attachments/1"
}
```

---

### GetAttachmentsByDocumentId

**Descripción:** Obtiene todos los adjuntos asociados a un documento específico, ordenados por fecha de creación descendente.

**Archivos:**
- `GetAttachmentsByDocumentIdQuery.cs`
- `GetAttachmentsByDocumentIdQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Attachments/by-document/{docId}
```

**Parámetros:**
- `docId` (path) - ID del documento (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Lista de adjuntos del documento
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Attachments/by-document/123
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "adjuntoId": 1,
    "path": "/uploads/documents/2024/01/factura_001.pdf",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "fechaModificacion": null,
    "fechaBorrado": null,
    "docId": 123
  },
  {
    "adjuntoId": 3,
    "path": "/uploads/documents/2024/01/anexo_001.pdf",
    "fechaCreacion": "2024-01-14T14:20:00Z",
    "fechaModificacion": null,
    "fechaBorrado": null,
    "docId": 123
  }
]
```

**Nota:** Este endpoint es útil para obtener todos los archivos adjuntos de un documento específico. Los adjuntos se ordenan por fecha de creación descendente (más recientes primero).

---

## Commands (Comandos)

### CreateAttachment

**Descripción:** Crea un nuevo adjunto asociado a un documento.

**Archivos:**
- `CreateAttachmentCommand.cs`
- `CreateAttachmentCommandHandler.cs`
- `CreateAttachmentCommandValidator.cs`

**DTO de Request:**
- `CreateAttachmentRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/Attachments
```

**Body (JSON):**
```json
{
  "path": "/uploads/documents/2024/01/factura_001.pdf",
  "docId": 123
}
```

**Validaciones:**
- `path` - Requerido, no vacío, máximo 500 caracteres
- `docId` - Requerido, mayor que 0
- Verifica que el documento exista

**Respuestas:**
- `201 Created` - Adjunto creado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Documento no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/Attachments
Content-Type: application/json
Authorization: Bearer {token}

{
  "path": "/uploads/documents/2024/01/factura_001.pdf",
  "docId": 123
}
```

**Respuesta exitosa (201):**
```json
{
  "adjuntoId": 1,
  "path": "/uploads/documents/2024/01/factura_001.pdf",
  "fechaCreacion": "2024-01-15T12:00:00Z",
  "fechaModificacion": null,
  "fechaBorrado": null,
  "docId": 123
}
```

**Respuesta cuando el documento no existe (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Document.NotFound",
  "status": 404,
  "detail": "Document with ID '123' was not found.",
  "instance": "/api/v1/Attachments"
}
```

**Nota:** El handler establece automáticamente `FechaCreacion = DateTime.UtcNow` al crear el adjunto. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

### UpdateAttachment

**Descripción:** Actualiza un adjunto existente. Solo se puede actualizar la ruta del archivo.

**Archivos:**
- `UpdateAttachmentCommand.cs`
- `UpdateAttachmentCommandHandler.cs`
- `UpdateAttachmentCommandValidator.cs`

**DTO de Request:**
- `UpdateAttachmentRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/Attachments/{adjuntoId}
```

**Parámetros:**
- `adjuntoId` (path) - ID del adjunto a actualizar

**Body (JSON):**
```json
{
  "path": "/uploads/documents/2024/01/factura_001_actualizada.pdf"
}
```

**Validaciones:**
- `adjuntoId` - Requerido, mayor que 0
- `path` - Requerido, no vacío, máximo 500 caracteres
- Verifica que el adjunto exista

**Respuestas:**
- `200 OK` - Adjunto actualizado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Adjunto no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/Attachments/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "path": "/uploads/documents/2024/01/factura_001_actualizada.pdf"
}
```

**Respuesta exitosa (200):**
```json
{
  "adjuntoId": 1,
  "path": "/uploads/documents/2024/01/factura_001_actualizada.pdf",
  "fechaCreacion": "2024-01-15T10:30:00Z",
  "fechaModificacion": "2024-01-15T12:00:00Z",
  "fechaBorrado": null,
  "docId": 123
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Attachment.NotFound",
  "status": 404,
  "detail": "Attachment with ID '1' was not found.",
  "instance": "/api/v1/Attachments/1"
}
```

**Nota:** Solo se puede actualizar la ruta del archivo (`Path`). Los campos `FechaCreacion`, `FechaBorrado` y `DocId` no se pueden modificar. El campo `FechaModificacion` se actualiza automáticamente a `DateTime.UtcNow` cuando se modifica el adjunto. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

### DeleteAttachment

**Descripción:** Elimina un adjunto por su ID.

**Archivos:**
- `DeleteAttachmentCommand.cs`
- `DeleteAttachmentCommandHandler.cs`
- `DeleteAttachmentCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/Attachments/{adjuntoId}
```

**Parámetros:**
- `adjuntoId` (path) - ID del adjunto (debe ser mayor que 0)

**Validaciones:**
- `adjuntoId` - Requerido, mayor que 0
- Verifica que el adjunto exista

**Respuestas:**
- `204 NoContent` - Adjunto eliminado exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Adjunto no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/Attachments/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Attachment.NotFound",
  "status": 404,
  "detail": "Attachment with ID '1' was not found.",
  "instance": "/api/v1/Attachments/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar como eliminada sin borrar físicamente usando el campo `FechaBorrado`), se puede refactorizar el handler. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Attachments` | Obtener todos los adjuntos | 200, 401, 403, 500 |
| GET | `/api/v1/Attachments/{adjuntoId}` | Obtener adjunto por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/Attachments/by-document/{docId}` | Obtener adjuntos por documento | 200, 400, 401, 403, 500 |
| POST | `/api/v1/Attachments` | Crear un nuevo adjunto | 201, 400, 401, 403, 404, 500 |
| PUT | `/api/v1/Attachments/{adjuntoId}` | Actualizar un adjunto | 200, 400, 401, 403, 404, 500 |
| DELETE | `/api/v1/Attachments/{adjuntoId}` | Eliminar un adjunto | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Attachments/{NombreCasoDeUso}/
├── {NombreCasoDeUso}Query.cs (o Command.cs)
├── {NombreCasoDeUso}QueryHandler.cs (o CommandHandler.cs)
└── {NombreCasoDeUso}Validator.cs (opcional)
```

### Validaciones

- Todos los validadores usan **FluentValidation**
- Se registran automáticamente con MediatR
- Validaciones comunes:
  - IDs: Mayor que 0
  - Strings: No vacíos, máximo 500 caracteres para `Path`
  - Documento existente: Verificado en Create

### Manejo de Errores

- Todos los handlers retornan `Result<T>` o `Result`
- Errores comunes:
  - `Error.NotFound` - Recurso no encontrado (404)
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
return result.MatchCreated(this, nameof(GetAllAsync)); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateAttachmentRequest` - Para crear adjuntos
  - `Path` (string, requerido) - Ruta del archivo adjunto (máximo 500 caracteres)
  - `DocId` (int, requerido) - ID del documento asociado
- `UpdateAttachmentRequest` - Para actualizar adjuntos
  - `Path` (string, requerido) - Nueva ruta del archivo adjunto (máximo 500 caracteres)

### Response DTOs

- `AttachmentResponse` - Respuesta estándar de Attachment
  - `AdjuntoId` (int) - Identificador único del adjunto
  - `Path` (string) - Ruta del archivo adjunto
  - `FechaCreacion` (DateTime) - Fecha de creación del adjunto
  - `FechaModificacion` (DateTime?) - Fecha de última modificación (nullable)
  - `FechaBorrado` (DateTime?) - Fecha de eliminación (nullable, para soft delete)
  - `DocId` (int) - ID del documento asociado

---

## Notas Técnicas

### Repositorio

El repositorio `AttachmentRepository` implementa `IAttachmentRepository` y extiende `GenericRepository<Attachment, PreloadDbContext>`. Incluye:

- **GetByIdAsync**: Método sobrescrito porque Attachment usa `AdjuntoId` (int) como clave primaria
- **GetByDocumentIdAsync**: Método específico para obtener todos los adjuntos de un documento
- **GetByPathAsync**: Método específico para obtener adjuntos por ruta de archivo
- **GetByDateRangeAsync**: Método específico para obtener adjuntos creados en un rango de fechas

**Sobrescritura de GetByIdAsync:**

```csharp
public override async Task<Attachment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    return await GetQueryable()
        .FirstOrDefaultAsync(a => a.AdjuntoId == id, cancellationToken);
}
```

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `AttachmentMappings.ToResponse` para mapear de la entidad de dominio `Attachment` a `AttachmentResponse`.

### Validación de Documento

El `CreateAttachmentCommandHandler` verifica que el documento exista antes de crear el adjunto:

- Si el documento no existe, retorna un error `404 NotFound` con el mensaje "Document with ID '{docId}' was not found."

### Ordenamiento

Los endpoints que retornan múltiples adjuntos (`GetAllAttachments` y `GetAttachmentsByDocumentId`) ordenan los resultados por `FechaCreacion` descendente (más recientes primero).

### Autorización

- **GET endpoints**: Requieren `RequirePreloadRead` (permite lectura a usuarios con roles: Administrator, PreloadReadOnly, PreloadAllSocieties, PreloadSingleSociety)
- **POST/PUT/DELETE endpoints**: Requieren `RequirePreloadWrite` (permite escritura a usuarios con roles: Administrator, PreloadAllSocieties, PreloadSingleSociety)

### Relaciones

La entidad `Attachment` tiene las siguientes relaciones:

- **Document** - Documento al que pertenece el adjunto (relación muchos a uno)
  - Clave foránea: `DocId`
  - Propiedad de navegación: `Doc`
  - Constraint: `FK__Adjuntos__DocId__25869641`
  - Comportamiento de eliminación: `ClientSetNull` (no se elimina el documento al eliminar el adjunto)

**Nota:** Al eliminar un adjunto, no se afecta el documento asociado. La relación es unidireccional desde Attachment hacia Document.

### Campos de la Entidad

La entidad `Attachment` incluye los siguientes campos:

- `AdjuntoId` (int, PK) - Identificador único del adjunto
- `Path` (string, requerido) - Ruta del archivo adjunto
- `FechaCreacion` (DateTime, requerido) - Fecha de creación
- `FechaModificacion` (DateTime?, nullable) - Fecha de última modificación
- `FechaBorrado` (DateTime?, nullable) - Fecha de eliminación (para soft delete)
- `DocId` (int, FK) - ID del documento asociado

### Tabla de Base de Datos

- **Nombre de tabla**: `Adjuntos`
- **Clave primaria**: `AdjuntoId` (int, Identity)
- **Índice**: `PK__Adjuntos__2ECBD540E38F6223`

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado CRUD completo para Attachments
  - GetAllAttachments - Query para obtener todos los adjuntos
  - GetAttachmentById - Query para obtener un adjunto por ID
  - GetAttachmentsByDocumentId - Query para obtener adjuntos por documento
  - CreateAttachment - Command para crear un nuevo adjunto
  - UpdateAttachment - Command para actualizar un adjunto existente
  - DeleteAttachment - Command para eliminar un adjunto
- **2024-12-19**: Agregado AttachmentResponse DTO y AttachmentMappings
- **2024-12-19**: Implementada validación de documento existente en Create
- **2024-12-19**: Agregado AttachmentsController con todos los endpoints
- **2024-12-19**: Configurada autorización (RequirePreloadRead para GET, RequirePreloadWrite para POST/PUT/DELETE)
- **2024-12-19**: Sobrescrito GetByIdAsync en AttachmentRepository para usar AdjuntoId como clave primaria

