# Casos de Uso - Notes

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Note`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllNotes](#getallnotes)
  - [GetNoteById](#getnotebyid)
  - [GetNotesByDocumentId](#getnotesbydocumentid)
- [Commands (Comandos)](#commands-comandos)
  - [CreateNote](#createnote)
  - [UpdateNote](#updatenote)
  - [DeleteNote](#deletenote)

---

## Queries (Consultas)

### GetAllNotes

**Descripción:** Obtiene todas las notas sin paginación, ordenadas por fecha de creación descendente.

**Archivos:**
- `GetAllNotesQuery.cs`
- `GetAllNotesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Notes
```

**Respuestas:**
- `200 OK` - Lista de notas
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Notes
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "notaId": 1,
    "descripcion": "Nota importante sobre el documento",
    "usuarioCreacion": "Juan Pérez",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "docId": 123
  },
  {
    "notaId": 2,
    "descripcion": "Revisar factura pendiente",
    "usuarioCreacion": "María García",
    "fechaCreacion": "2024-01-14T14:20:00Z",
    "docId": 124
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`). Las notas se ordenan por fecha de creación descendente (más recientes primero).

---

### GetNoteById

**Descripción:** Obtiene una nota por su ID.

**Archivos:**
- `GetNoteByIdQuery.cs`
- `GetNoteByIdQueryHandler.cs`
- `GetNoteByIdValidator.cs`

**Endpoint:**
```
GET /api/v1/Notes/{notaId}
```

**Parámetros:**
- `notaId` (path) - ID de la nota (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Nota encontrada
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Nota no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Notes/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "notaId": 1,
  "descripcion": "Nota importante sobre el documento",
  "usuarioCreacion": "Juan Pérez",
  "fechaCreacion": "2024-01-15T10:30:00Z",
  "docId": 123
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Note.NotFound",
  "status": 404,
  "detail": "Note with ID '1' was not found.",
  "instance": "/api/v1/Notes/1"
}
```

---

### GetNotesByDocumentId

**Descripción:** Obtiene todas las notas asociadas a un documento específico, ordenadas por fecha de creación descendente.

**Archivos:**
- `GetNotesByDocumentIdQuery.cs`
- `GetNotesByDocumentIdQueryHandler.cs`
- `GetNotesByDocumentIdValidator.cs`

**Endpoint:**
```
GET /api/v1/Notes/by-document/{docId}
```

**Parámetros:**
- `docId` (path) - ID del documento (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Lista de notas del documento
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Notes/by-document/123
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "notaId": 1,
    "descripcion": "Nota más reciente",
    "usuarioCreacion": "Juan Pérez",
    "fechaCreacion": "2024-01-15T10:30:00Z",
    "docId": 123
  },
  {
    "notaId": 2,
    "descripcion": "Nota anterior",
    "usuarioCreacion": "María García",
    "fechaCreacion": "2024-01-14T14:20:00Z",
    "docId": 123
  }
]
```

**Nota:** Este endpoint es útil para obtener el historial de notas de un documento específico. Las notas se ordenan por fecha de creación descendente (más recientes primero).

---

## Commands (Comandos)

### CreateNote

**Descripción:** Crea una nueva nota asociada a un documento.

**Archivos:**
- `CreateNoteCommand.cs`
- `CreateNoteCommandHandler.cs`
- `CreateNoteCommandValidator.cs`

**DTO de Request:**
- `CreateNoteRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/Notes
```

**Body (JSON):**
```json
{
  "docId": 123,
  "descripcion": "Nota importante sobre el documento",
  "usuarioCreacion": "Juan Pérez"
}
```

**Validaciones:**
- `docId` - Requerido, mayor que 0
- `descripcion` - Requerido, no vacío
- `usuarioCreacion` - Requerido, no vacío
- Verifica que el documento exista

**Respuestas:**
- `201 Created` - Nota creada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Documento no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/Notes
Content-Type: application/json
Authorization: Bearer {token}

{
  "docId": 123,
  "descripcion": "Nota importante sobre el documento",
  "usuarioCreacion": "Juan Pérez"
}
```

**Respuesta exitosa (201):**
```json
{
  "notaId": 1,
  "descripcion": "Nota importante sobre el documento",
  "usuarioCreacion": "Juan Pérez",
  "fechaCreacion": "2024-01-15T12:00:00Z",
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
  "instance": "/api/v1/Notes"
}
```

**Nota:** El handler establece automáticamente `FechaCreacion = DateTime.UtcNow` al crear la nota. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

### UpdateNote

**Descripción:** Actualiza una nota existente. Solo se puede actualizar la descripción.

**Archivos:**
- `UpdateNoteCommand.cs`
- `UpdateNoteCommandHandler.cs`
- `UpdateNoteCommandValidator.cs`

**DTO de Request:**
- `UpdateNoteRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/Notes/{notaId}
```

**Parámetros:**
- `notaId` (path) - ID de la nota a actualizar

**Body (JSON):**
```json
{
  "descripcion": "Descripción actualizada de la nota"
}
```

**Validaciones:**
- `notaId` - Requerido, mayor que 0
- `descripcion` - Requerido, no vacío
- Verifica que la nota exista

**Respuestas:**
- `200 OK` - Nota actualizada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Nota no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/Notes/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Descripción actualizada de la nota"
}
```

**Respuesta exitosa (200):**
```json
{
  "notaId": 1,
  "descripcion": "Descripción actualizada de la nota",
  "usuarioCreacion": "Juan Pérez",
  "fechaCreacion": "2024-01-15T10:30:00Z",
  "docId": 123
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Note.NotFound",
  "status": 404,
  "detail": "Note with ID '1' was not found.",
  "instance": "/api/v1/Notes/1"
}
```

**Nota:** Solo se puede actualizar la descripción de la nota. Los campos `UsuarioCreacion`, `FechaCreacion` y `DocId` no se pueden modificar. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

### DeleteNote

**Descripción:** Elimina una nota por su ID.

**Archivos:**
- `DeleteNoteCommand.cs`
- `DeleteNoteCommandHandler.cs`
- `DeleteNoteCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/Notes/{notaId}
```

**Parámetros:**
- `notaId` (path) - ID de la nota (debe ser mayor que 0)

**Validaciones:**
- `notaId` - Requerido, mayor que 0
- Verifica que la nota exista

**Respuestas:**
- `204 NoContent` - Nota eliminada exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Nota no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/Notes/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Note.NotFound",
  "status": 404,
  "detail": "Note with ID '1' was not found.",
  "instance": "/api/v1/Notes/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar como eliminada sin borrar físicamente), se puede refactorizar el handler. Este endpoint requiere permisos de escritura (`RequirePreloadWrite`).

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Notes` | Obtener todas las notas | 200, 401, 403, 500 |
| GET | `/api/v1/Notes/{notaId}` | Obtener nota por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/Notes/by-document/{docId}` | Obtener notas por documento | 200, 400, 401, 403, 500 |
| POST | `/api/v1/Notes` | Crear una nueva nota | 201, 400, 401, 403, 404, 500 |
| PUT | `/api/v1/Notes/{notaId}` | Actualizar una nota | 200, 400, 401, 403, 404, 500 |
| DELETE | `/api/v1/Notes/{notaId}` | Eliminar una nota | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Notes/{NombreCasoDeUso}/
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
return result.MatchCreated(this, nameof(GetByIdAsync), new { notaId = 0 }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateNoteRequest` - Para crear notas
  - `DocId` (int, requerido) - ID del documento asociado
  - `Descripcion` (string, requerido) - Descripción de la nota
  - `UsuarioCreacion` (string, requerido) - Usuario que crea la nota
- `UpdateNoteRequest` - Para actualizar notas
  - `Descripcion` (string, requerido) - Nueva descripción de la nota

### Response DTOs

- `NoteResponse` - Respuesta estándar de Note
  - `NotaId` (int) - Identificador único de la nota
  - `Descripcion` (string?) - Descripción de la nota (nullable)
  - `UsuarioCreacion` (string) - Usuario que creó la nota
  - `FechaCreacion` (DateTime) - Fecha de creación de la nota
  - `DocId` (int) - ID del documento asociado

---

## Notas Técnicas

### Repositorio

El repositorio `NoteRepository` implementa `INoteRepository` y extiende `GenericRepository<Note, PreloadDbContext>`. Incluye:

- **GetByNotaIdAsync**: Método específico para obtener una nota por su ID
- **GetByDocumentIdAsync**: Método específico para obtener todas las notas de un documento
- **GetByUsuarioCreacionAsync**: Método específico para obtener todas las notas creadas por un usuario
- **GetByIdAsync**: Método sobrescrito porque Note usa `NotaId` (int) como clave primaria

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `NoteMappings.ToResponse` para mapear de la entidad de dominio `Note` a `NoteResponse`.

### Validación de Documento

El `CreateNoteCommandHandler` verifica que el documento exista antes de crear la nota:

- Si el documento no existe, retorna un error `404 NotFound` con el mensaje "Document with ID '{docId}' was not found."

### Ordenamiento

Los endpoints que retornan múltiples notas (`GetAllNotes` y `GetNotesByDocumentId`) ordenan los resultados por `FechaCreacion` descendente (más recientes primero).

### Autorización

- **GET endpoints**: Requieren `RequirePreloadRead` (permite lectura a usuarios con roles: Administrator, PreloadReadOnly, PreloadAllSocieties, PreloadSingleSociety)
- **POST/PUT/DELETE endpoints**: Requieren `RequirePreloadWrite` (permite escritura a usuarios con roles: Administrator, PreloadAllSocieties, PreloadSingleSociety)

### Relaciones

La entidad `Note` tiene las siguientes relaciones:

- **Document** - Documento al que pertenece la nota (relación muchos a uno)
  - Clave foránea: `DocId`
  - Propiedad de navegación: `Document`

**Nota:** Al eliminar una nota, no se afecta el documento asociado. La relación es unidireccional desde Note hacia Document.

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado CRUD completo para Notes
  - GetAllNotes - Query para obtener todas las notas
  - GetNoteById - Query para obtener una nota por ID
  - GetNotesByDocumentId - Query para obtener notas por documento
  - CreateNote - Command para crear una nueva nota
  - UpdateNote - Command para actualizar una nota existente
  - DeleteNote - Command para eliminar una nota
- **2024-12-19**: Agregado NoteResponse DTO y NoteMappings
- **2024-12-19**: Implementada validación de documento existente en Create
- **2024-12-19**: Agregado NotesController con todos los endpoints
- **2024-12-19**: Configurada autorización (RequirePreloadRead para GET, RequirePreloadWrite para POST/PUT/DELETE)

