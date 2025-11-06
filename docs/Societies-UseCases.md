# Casos de Uso - Societies

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Society`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllSocieties](#getallsocieties)
  - [GetAllSocietiesPaged](#getallsocietiespaged)
  - [GetSocietyById](#getsocietybyid)
  - [GetSocietyByCodigo](#getsocietybycodigo)
  - [GetSocietyByCuit](#getsocietybycuit)
- [Commands (Comandos)](#commands-comandos)
  - [CreateSociety](#createsociety)
  - [UpdateSociety](#updatesociety)
  - [DeleteSociety](#deletesociety)

---

## Queries (Consultas)

### GetAllSocieties

**Descripción:** Obtiene todas las societies sin paginación.

**Archivos:**
- `GetAllSocietiesQuery.cs`
- `GetAllSocietiesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Societies
```

**Respuestas:**
- `200 OK` - Lista de societies
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Societies
```

**Respuesta exitosa:**
```json
[
  {
    "socId": 1,
    "codigo": "ABC123",
    "descripcion": "Descripción",
    "cuit": "12345678901",
    "fechaCreacion": "2024-01-01T00:00:00Z",
    "fechaBaja": null,
    "esPrecarga": true
  }
]
```

---

### GetAllSocietiesPaged

**Descripción:** Obtiene societies con paginación.

**Archivos:**
- `GetAllSocietiesPagedQuery.cs`
- `GetAllSocietiesPagedQueryHandler.cs`
- `GetAllSocietiesPagedValidator.cs`

**Endpoint:**
```
GET /api/v1/Societies/paged?page=1&pageSize=20
```

**Parámetros:**
- `page` (opcional, default: 1) - Número de página (1-based)
- `pageSize` (opcional, default: 20) - Tamaño de página

**Respuestas:**
- `200 OK` - PagedResponse con societies
- `400 BadRequest` - Parámetros de paginación inválidos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Societies/paged?page=1&pageSize=20
```

**Respuesta exitosa:**
```json
{
  "items": [
    {
      "socId": 1,
      "codigo": "ABC123",
      "descripcion": "Descripción",
      "cuit": "12345678901",
      "fechaCreacion": "2024-01-01T00:00:00Z",
      "fechaBaja": null,
      "esPrecarga": true
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

**Nota:** Este caso de uso utiliza el método `GetPagedAsync` del repositorio genérico, que está disponible para todas las entidades.

---

### GetSocietyById

**Descripción:** Obtiene una society por su ID.

**Archivos:**
- `GetSocietyByIdQuery.cs`
- `GetSocietyByIdQueryHandler.cs`
- `GetSocietyByIdValidator.cs`

**Endpoint:**
```
GET /api/v1/Societies/id/{id}
```

**Parámetros:**
- `id` (path) - ID de la society (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Society encontrada
- `400 BadRequest` - ID inválido
- `404 NotFound` - Society no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Societies/id/1
```

**Respuesta exitosa:**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción",
  "cuit": "12345678901",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "esPrecarga": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Society.NotFound",
  "status": 404,
  "detail": "Society with ID '1' was not found.",
  "instance": "/api/v1/Societies/id/1"
}
```

---

### GetSocietyByCodigo

**Descripción:** Obtiene una society por su código.

**Archivos:**
- `GetSocietyByCodigoQuery.cs`
- `GetSocietyByCodigoQueryHandler.cs`
- `GetSocietyByCodigoValidator.cs`

**Endpoint:**
```
GET /api/v1/Societies/{codigo}
```

**Parámetros:**
- `codigo` (path) - Código de la society (requerido, no vacío)

**Respuestas:**
- `200 OK` - Society encontrada
- `400 BadRequest` - Código inválido
- `404 NotFound` - Society no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Societies/ABC123
```

**Respuesta exitosa:**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción",
  "cuit": "12345678901",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "esPrecarga": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Society.NotFound",
  "status": 404,
  "detail": "Society with code 'ABC123' was not found.",
  "instance": "/api/v1/Societies/ABC123"
}
```

---

### GetSocietyByCuit

**Descripción:** Obtiene una society por su CUIT.

**Archivos:**
- `GetSocietyByCuitQuery.cs`
- `GetSocietyByCuitQueryHandler.cs`
- `GetSocietyByCuitValidator.cs`

**Endpoint:**
```
GET /api/v1/Societies/cuit/{cuit}
```

**Parámetros:**
- `cuit` (path) - CUIT de la society (requerido, no vacío)

**Respuestas:**
- `200 OK` - Society encontrada
- `400 BadRequest` - CUIT inválido
- `404 NotFound` - Society no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Societies/cuit/12345678901
```

**Respuesta exitosa:**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción",
  "cuit": "12345678901",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "esPrecarga": true
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Society.NotFound",
  "status": 404,
  "detail": "Society with CUIT '12345678901' was not found.",
  "instance": "/api/v1/Societies/cuit/12345678901"
}
```

---

## Commands (Comandos)

### CreateSociety

**Descripción:** Crea una nueva society.

**Archivos:**
- `CreateSocietyCommand.cs`
- `CreateSocietyCommandHandler.cs`
- `CreateSocietyCommandValidator.cs`

**DTO de Request:**
- `CreateSocietyRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/Societies
```

**Body (JSON):**
```json
{
  "codigo": "ABC123",
  "descripcion": "Descripción de la sociedad",
  "cuit": "12345678901",
  "esPrecarga": true
}
```

**Validaciones:**
- `codigo` - Requerido, no vacío
- `descripcion` - Requerido, no vacío
- `cuit` - Requerido, no vacío
- Verifica que no exista otra society con el mismo código
- Verifica que no exista otra society con el mismo CUIT

**Respuestas:**
- `201 Created` - Society creada exitosamente
- `400 BadRequest` - Validación fallida
- `409 Conflict` - Society con mismo código o CUIT ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/Societies
Content-Type: application/json

{
  "codigo": "ABC123",
  "descripcion": "Descripción de la sociedad",
  "cuit": "12345678901",
  "esPrecarga": true
}
```

**Respuesta exitosa (201):**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción de la sociedad",
  "cuit": "12345678901",
  "fechaCreacion": "2024-01-01T12:00:00Z",
  "fechaBaja": null,
  "esPrecarga": true
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "Society.Conflict",
  "status": 409,
  "detail": "A society with code 'ABC123' already exists.",
  "instance": "/api/v1/Societies"
}
```

**Nota:** El handler establece automáticamente `FechaCreacion = DateTime.UtcNow` al crear la society.

---

### UpdateSociety

**Descripción:** Actualiza una society existente.

**Archivos:**
- `UpdateSocietyCommand.cs`
- `UpdateSocietyCommandHandler.cs`
- `UpdateSocietyCommandValidator.cs`

**DTO de Request:**
- `UpdateSocietyRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/Societies
```

**Body (JSON):**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción actualizada",
  "cuit": "12345678901",
  "esPrecarga": false
}
```

**Validaciones:**
- `socId` - Requerido, mayor que 0
- `codigo` - Requerido, no vacío
- `descripcion` - Requerido, no vacío
- `cuit` - Requerido, no vacío
- Verifica que la society exista
- Verifica que no exista otra society con el mismo código (excluyendo la actual)
- Verifica que no exista otra society con el mismo CUIT (excluyendo la actual)

**Respuestas:**
- `200 OK` - Society actualizada exitosamente
- `400 BadRequest` - Validación fallida
- `404 NotFound` - Society no encontrada
- `409 Conflict` - Society con mismo código o CUIT ya existe (excluyendo la actual)
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/Societies
Content-Type: application/json

{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción actualizada",
  "cuit": "12345678901",
  "esPrecarga": false
}
```

**Respuesta exitosa (200):**
```json
{
  "socId": 1,
  "codigo": "ABC123",
  "descripcion": "Descripción actualizada",
  "cuit": "12345678901",
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "fechaBaja": null,
  "esPrecarga": false
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Society.NotFound",
  "status": 404,
  "detail": "Society with ID '1' was not found.",
  "instance": "/api/v1/Societies"
}
```

**Nota:** El handler excluye la society actual del chequeo de conflictos, permitiendo actualizaciones sin cambiar código o CUIT.

---

### DeleteSociety

**Descripción:** Elimina una society por su ID.

**Archivos:**
- `DeleteSocietyCommand.cs`
- `DeleteSocietyCommandHandler.cs`
- `DeleteSocietyCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/Societies/{id}
```

**Parámetros:**
- `id` (path) - ID de la society (debe ser mayor que 0)

**Validaciones:**
- `id` - Requerido, mayor que 0
- Verifica que la society exista

**Respuestas:**
- `204 NoContent` - Society eliminada exitosamente
- `400 BadRequest` - ID inválido
- `404 NotFound` - Society no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/Societies/1
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Society.NotFound",
  "status": 404,
  "detail": "Society with ID '1' was not found.",
  "instance": "/api/v1/Societies/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar `FechaBaja` en lugar de eliminar), se puede refactorizar el handler.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Societies` | Obtener todas las societies | 200, 500 |
| GET | `/api/v1/Societies/paged` | Obtener societies paginadas | 200, 400, 500 |
| GET | `/api/v1/Societies/id/{id}` | Obtener society por ID | 200, 400, 404, 500 |
| GET | `/api/v1/Societies/{codigo}` | Obtener society por código | 200, 400, 404, 500 |
| GET | `/api/v1/Societies/cuit/{cuit}` | Obtener society por CUIT | 200, 400, 404, 500 |
| POST | `/api/v1/Societies` | Crear una nueva society | 201, 400, 409, 500 |
| PUT | `/api/v1/Societies` | Actualizar una society | 200, 400, 404, 409, 500 |
| DELETE | `/api/v1/Societies/{id}` | Eliminar una society | 204, 400, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Societies/{NombreCasoDeUso}/
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

// POST
return result.MatchCreated(this, nameof(GetById), new { id = result.Value.Id }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateSocietyRequest` - Para crear societies
- `UpdateSocietyRequest` - Para actualizar societies
- `GetAllSocietiesRequest` - Para paginación (opcional)

### Response DTOs

- `SocietyResponse` - Respuesta estándar de Society
- `PagedResponse<SocietyResponse>` - Respuesta paginada

---

## Notas Técnicas

### Repositorio Genérico

El método `GetPagedAsync` está implementado en el repositorio genérico (`GenericRepository`), por lo que está disponible para todas las entidades. Si una entidad específica necesita un comportamiento diferente, puede sobrescribir el método (como se hace en `SocietyRepository` para ordenar por `SocId`).

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `SocietyMappings.ToResponse` para mapear de la entidad de dominio a `SocietyResponse`.

---

## Fecha de Creación

Documento creado: 2024-01-01

**Última actualización:** 2024-01-01

