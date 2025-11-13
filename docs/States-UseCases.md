# Casos de Uso - States

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `State`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllStates](#getallstates)
  - [GetStateById](#getstatebyid)
  - [GetStateByCode](#getstatebycode)
- [Commands (Comandos)](#commands-comandos)
  - [CreateState](#createstate)
  - [UpdateState](#updatestate)
  - [DeleteState](#deletestate)

---

## Queries (Consultas)

### GetAllStates

**Descripción:** Obtiene todos los estados sin paginación.

**Archivos:**
- `GetAllStatesQuery.cs`
- `GetAllStatesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/States
```

**Respuestas:**
- `200 OK` - Lista de estados
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/States
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "estadoId": 1,
    "descripcion": "Pendiente Precarga",
    "codigo": "PendientePrecarga",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null
  },
  {
    "estadoId": 2,
    "descripcion": "Pagado",
    "codigo": "Pagado",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`). Todos los campos son requeridos excepto `fechaBaja` que puede ser null.

---

### GetStateById

**Descripción:** Obtiene un estado por su ID.

**Archivos:**
- `GetStateByIdQuery.cs`
- `GetStateByIdQueryHandler.cs`
- `GetStateByIdQueryValidator.cs`

**Endpoint:**
```
GET /api/v1/States/id/{estadoId}
```

**Parámetros:**
- `estadoId` (path) - ID del estado (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Estado encontrado
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Estado no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/States/id/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "estadoId": 1,
  "descripcion": "Pendiente Precarga",
  "codigo": "PendientePrecarga",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "State.NotFound",
  "status": 404,
  "detail": "State with ID '1' was not found.",
  "instance": "/api/v1/States/id/1"
}
```

---

### GetStateByCode

**Descripción:** Obtiene un estado por su código.

**Archivos:**
- `GetStateByCodeQuery.cs`
- `GetStateByCodeQueryHandler.cs`
- `GetStateByCodeQueryValidator.cs`

**Endpoint:**
```
GET /api/v1/States/code/{codigo}
```

**Parámetros:**
- `codigo` (path) - Código del estado (requerido, no vacío)

**Respuestas:**
- `200 OK` - Estado encontrado
- `400 BadRequest` - Código inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Estado no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/States/code/PendientePrecarga
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "estadoId": 1,
  "descripcion": "Pendiente Precarga",
  "codigo": "PendientePrecarga",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "State.NotFound",
  "status": 404,
  "detail": "State with code 'PendientePrecarga' was not found.",
  "instance": "/api/v1/States/code/PendientePrecarga"
}
```

---

## Commands (Comandos)

### CreateState

**Descripción:** Crea un nuevo estado.

**Archivos:**
- `CreateStateCommand.cs`
- `CreateStateCommandHandler.cs`
- `CreateStateCommandValidator.cs`

**DTO de Request:**
- `CreateStateRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/States
```

**Body (JSON):**
```json
{
  "descripcion": "Nuevo Estado",
  "codigo": "NuevoEstado"
}
```

**Validaciones:**
- `descripcion` - Requerido, no vacío
- `codigo` - Requerido, no vacío
- Verifica que no exista otro estado con el mismo código

**Respuestas:**
- `201 Created` - Estado creado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `409 Conflict` - Estado con mismo código ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/States
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Nuevo Estado",
  "codigo": "NuevoEstado"
}
```

**Respuesta exitosa (201):**
```json
{
  "estadoId": 3,
  "descripcion": "Nuevo Estado",
  "codigo": "NuevoEstado",
  "fechaCreacion": "2024-01-15T12:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "State.Conflict",
  "status": 409,
  "detail": "A state with code 'NuevoEstado' already exists.",
  "instance": "/api/v1/States"
}
```

**Nota:** El handler establece automáticamente `FechaCreacion = DateTime.UtcNow` al crear el estado. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### UpdateState

**Descripción:** Actualiza un estado existente.

**Archivos:**
- `UpdateStateCommand.cs`
- `UpdateStateCommandHandler.cs`
- `UpdateStateCommandValidator.cs`

**DTO de Request:**
- `UpdateStateRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/States/{estadoId}
```

**Parámetros:**
- `estadoId` (path) - ID del estado a actualizar

**Body (JSON):**
```json
{
  "descripcion": "Estado Actualizado",
  "codigo": "EstadoActualizado"
}
```

**Validaciones:**
- `estadoId` - Requerido, mayor que 0
- `descripcion` - Requerido, no vacío
- `codigo` - Requerido, no vacío
- Verifica que el estado exista
- Verifica que no exista otro estado con el mismo código (excluyendo el actual)

**Respuestas:**
- `200 OK` - Estado actualizado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Estado no encontrado
- `409 Conflict` - Estado con mismo código ya existe (excluyendo el actual)
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/States/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Estado Actualizado",
  "codigo": "EstadoActualizado"
}
```

**Respuesta exitosa (200):**
```json
{
  "estadoId": 1,
  "descripcion": "Estado Actualizado",
  "codigo": "EstadoActualizado",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "State.NotFound",
  "status": 404,
  "detail": "State with ID '1' was not found.",
  "instance": "/api/v1/States/1"
}
```

**Nota:** El handler excluye el estado actual del chequeo de conflictos, permitiendo actualizaciones sin cambiar el código. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### DeleteState

**Descripción:** Elimina un estado por su ID.

**Archivos:**
- `DeleteStateCommand.cs`
- `DeleteStateCommandHandler.cs`
- `DeleteStateCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/States/{estadoId}
```

**Parámetros:**
- `estadoId` (path) - ID del estado (debe ser mayor que 0)

**Validaciones:**
- `estadoId` - Requerido, mayor que 0
- Verifica que el estado exista

**Respuestas:**
- `204 NoContent` - Estado eliminado exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Estado no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/States/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "State.NotFound",
  "status": 404,
  "detail": "State with ID '1' was not found.",
  "instance": "/api/v1/States/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar `FechaBaja` en lugar de eliminar), se puede refactorizar el handler. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

**Advertencia:** Eliminar un estado puede afectar documentos que lo referencian. Se recomienda verificar las relaciones antes de eliminar o implementar soft delete.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/States` | Obtener todos los estados | 200, 401, 403, 500 |
| GET | `/api/v1/States/id/{estadoId}` | Obtener estado por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/States/code/{codigo}` | Obtener estado por código | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/States` | Crear un nuevo estado | 201, 400, 401, 403, 409, 500 |
| PUT | `/api/v1/States/{estadoId}` | Actualizar un estado | 200, 400, 401, 403, 404, 409, 500 |
| DELETE | `/api/v1/States/{estadoId}` | Eliminar un estado | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/States/{NombreCasoDeUso}/
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

- `CreateStateRequest` - Para crear estados
  - `Descripcion` (string, requerido) - Descripción del estado
  - `Codigo` (string, requerido) - Código único del estado
- `UpdateStateRequest` - Para actualizar estados
  - `Descripcion` (string, requerido) - Descripción del estado
  - `Codigo` (string, requerido) - Código único del estado

### Response DTOs

- `StateResponse` - Respuesta estándar de State
  - `EstadoId` (int) - Identificador único del estado
  - `Descripcion` (string) - Descripción del estado
  - `Codigo` (string) - Código único del estado
  - `FechaCreacion` (DateTime) - Fecha de creación del estado
  - `FechaBaja` (DateTime?) - Fecha de baja del estado (nullable)

---

## Notas Técnicas

### Repositorio

El repositorio `StateRepository` implementa `IStateRepository` y extiende `GenericRepository<State, PreloadDbContext>`. Incluye:

- **GetByCodeAsync**: Método específico para obtener un estado por su código
- **GetByEstadoIdAsync**: Método específico para obtener un estado por su ID
- **GetByIdAsync**: Método sobrescrito porque State usa `EstadoId` (int) como clave primaria

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `StateMappings.ToResponse` para mapear de la entidad de dominio `State` a `StateResponse`.

### Validación de Código Único

Tanto `CreateStateCommandHandler` como `UpdateStateCommandHandler` verifican que no exista otro estado con el mismo código:

- En **Create**: Verifica que no exista ningún estado con el código proporcionado
- En **Update**: Verifica que no exista otro estado con el código proporcionado, excluyendo el estado actual

### Autorización

- **GET endpoints**: Requieren `RequirePreloadRead` (permite lectura a usuarios con roles: Administrator, PreloadReadOnly, PreloadAllSocieties, PreloadSingleSociety)
- **POST/PUT/DELETE endpoints**: Requieren `RequireAdministrator` (solo usuarios con rol Administrator)

### Relaciones

La entidad `State` tiene las siguientes relaciones:

- **Documents** - Colección de documentos que tienen este estado
- **DocumentStates** - Colección de estados de documentos asociados

**Nota:** Al eliminar un estado, se debe considerar el impacto en los documentos que lo referencian. Se recomienda implementar soft delete o validar relaciones antes de eliminar.

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado CRUD completo para States
  - GetAllStates - Query para obtener todos los estados
  - GetStateById - Query para obtener un estado por ID
  - GetStateByCode - Query para obtener un estado por código
  - CreateState - Command para crear un nuevo estado
  - UpdateState - Command para actualizar un estado existente
  - DeleteState - Command para eliminar un estado
- **2024-12-19**: Agregado StateResponse DTO y StateMappings
- **2024-12-19**: Implementada validación de código único en Create y Update
- **2024-12-19**: Agregado StatesController con todos los endpoints
- **2024-12-19**: Configurada autorización (RequirePreloadRead para GET, RequireAdministrator para POST/PUT/DELETE)

