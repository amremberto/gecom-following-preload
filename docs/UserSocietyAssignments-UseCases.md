# Casos de Uso - UserSocietyAssignments

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `UserSocietyAssignment`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllUserSocietyAssignments](#getallusersocietyassignments)
  - [GetAllUserSocietyAssignmentsPaged](#getallusersocietyassignmentspaged)
  - [GetUserSocietyAssignmentById](#getusersocietyassignmentbyid)
- [Commands (Comandos)](#commands-comandos)
  - [CreateUserSocietyAssignment](#createusersocietyassignment)
  - [UpdateUserSocietyAssignment](#updateusersocietyassignment)
  - [DeleteUserSocietyAssignment](#deleteusersocietyassignment)

---

## Queries (Consultas)

### GetAllUserSocietyAssignments

**Descripción:** Obtiene todas las asignaciones de usuario-sociedad sin paginación.

**Archivos:**
- `GetAllUserSocietyAssignmentsQuery.cs`
- `GetAllUserSocietyAssignmentsQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/UserSocietyAssignments
```

**Respuestas:**
- `200 OK` - Lista de asignaciones usuario-sociedad
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/UserSocietyAssignments
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "id": 1,
    "email": "usuario@example.com",
    "cuitClient": "12345678901",
    "sociedadFi": "ABC1"
  },
  {
    "id": 2,
    "email": "otro@example.com",
    "cuitClient": "98765432109",
    "sociedadFi": "XYZ2"
  }
]
```

---

### GetAllUserSocietyAssignmentsPaged

**Descripción:** Obtiene asignaciones usuario-sociedad con paginación.

**Archivos:**
- `GetAllUserSocietyAssignmentsPagedQuery.cs`
- `GetAllUserSocietyAssignmentsPagedQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/UserSocietyAssignments/paged?page=1&pageSize=20
```

**Parámetros:**
- `page` (opcional, default: 1) - Número de página (1-based)
- `pageSize` (opcional, default: 20) - Tamaño de página

**Respuestas:**
- `200 OK` - PagedResponse con asignaciones usuario-sociedad
- `400 BadRequest` - Parámetros de paginación inválidos
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/UserSocietyAssignments/paged?page=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "items": [
    {
      "id": 1,
      "email": "usuario@example.com",
      "cuitClient": "12345678901",
      "sociedadFi": "ABC1"
    },
    {
      "id": 2,
      "email": "otro@example.com",
      "cuitClient": "98765432109",
      "sociedadFi": "XYZ2"
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

**Nota:** Este caso de uso utiliza el método `GetPagedAsync` del repositorio, que ordena por `Id`.

---

### GetUserSocietyAssignmentById

**Descripción:** Obtiene una asignación usuario-sociedad por su ID.

**Archivos:**
- `GetUserSocietyAssignmentByIdQuery.cs`
- `GetUserSocietyAssignmentByIdQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/UserSocietyAssignments/{id}
```

**Parámetros:**
- `id` (path) - ID de la asignación usuario-sociedad (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Asignación usuario-sociedad encontrada
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Asignación usuario-sociedad no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/UserSocietyAssignments/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "id": 1,
  "email": "usuario@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "UserSocietyAssignment.NotFound",
  "status": 404,
  "detail": "UserSocietyAssignment with ID '1' was not found.",
  "instance": "/api/v1/UserSocietyAssignments/1"
}
```

---

## Commands (Comandos)

### CreateUserSocietyAssignment

**Descripción:** Crea una nueva asignación usuario-sociedad.

**Archivos:**
- `CreateUserSocietyAssignmentCommand.cs`
- `CreateUserSocietyAssignmentCommandHandler.cs`
- `CreateUserSocietyAssignmentCommandValidator.cs`

**DTO de Request:**
- `CreateUserSocietyAssignmentRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/UserSocietyAssignments
```

**Body (JSON):**
```json
{
  "email": "usuario@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Validaciones:**
- `email` - Requerido, formato de email válido, máximo 255 caracteres
- `cuitClient` - Requerido, no vacío, máximo 12 caracteres
- `sociedadFi` - Requerido, no vacío, máximo 12 caracteres

**Respuestas:**
- `201 Created` - Asignación usuario-sociedad creada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/UserSocietyAssignments
Content-Type: application/json
Authorization: Bearer {token}

{
  "email": "usuario@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Respuesta exitosa (201):**
```json
{
  "id": 1,
  "email": "usuario@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Respuesta cuando falla la validación (400):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "See errors for details.",
  "errors": {
    "Email": [
      "Email is required.",
      "Email must be a valid email address."
    ],
    "CuitClient": [
      "CuitClient is required."
    ]
  }
}
```

---

### UpdateUserSocietyAssignment

**Descripción:** Actualiza una asignación usuario-sociedad existente.

**Archivos:**
- `UpdateUserSocietyAssignmentCommand.cs`
- `UpdateUserSocietyAssignmentCommandHandler.cs`
- `UpdateUserSocietyAssignmentCommandValidator.cs`

**DTO de Request:**
- `UpdateUserSocietyAssignmentRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/UserSocietyAssignments
```

**Body (JSON):**
```json
{
  "id": 1,
  "email": "usuario.actualizado@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Validaciones:**
- `id` - Requerido, mayor que 0
- `email` - Requerido, formato de email válido, máximo 255 caracteres
- `cuitClient` - Requerido, no vacío, máximo 12 caracteres
- `sociedadFi` - Requerido, no vacío, máximo 12 caracteres
- Verifica que la asignación usuario-sociedad exista

**Respuestas:**
- `200 OK` - Asignación usuario-sociedad actualizada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Asignación usuario-sociedad no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/UserSocietyAssignments
Content-Type: application/json
Authorization: Bearer {token}

{
  "id": 1,
  "email": "usuario.actualizado@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Respuesta exitosa (200):**
```json
{
  "id": 1,
  "email": "usuario.actualizado@example.com",
  "cuitClient": "12345678901",
  "sociedadFi": "ABC1"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "UserSocietyAssignment.NotFound",
  "status": 404,
  "detail": "UserSocietyAssignment with ID '1' was not found.",
  "instance": "/api/v1/UserSocietyAssignments"
}
```

---

### DeleteUserSocietyAssignment

**Descripción:** Elimina una asignación usuario-sociedad por su ID.

**Archivos:**
- `DeleteUserSocietyAssignmentCommand.cs`
- `DeleteUserSocietyAssignmentCommandHandler.cs`
- `DeleteUserSocietyAssignmentCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/UserSocietyAssignments/{id}
```

**Parámetros:**
- `id` (path) - ID de la asignación usuario-sociedad (debe ser mayor que 0)

**Validaciones:**
- `id` - Requerido, mayor que 0
- Verifica que la asignación usuario-sociedad exista

**Respuestas:**
- `204 NoContent` - Asignación usuario-sociedad eliminada exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Asignación usuario-sociedad no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/UserSocietyAssignments/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "UserSocietyAssignment.NotFound",
  "status": 404,
  "detail": "UserSocietyAssignment with ID '1' was not found.",
  "instance": "/api/v1/UserSocietyAssignments/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete**, se puede refactorizar el handler para agregar un campo de eliminación lógica.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/UserSocietyAssignments` | Obtener todas las asignaciones | 200, 401, 403, 500 |
| GET | `/api/v1/UserSocietyAssignments/paged` | Obtener asignaciones paginadas | 200, 400, 401, 403, 500 |
| GET | `/api/v1/UserSocietyAssignments/{id}` | Obtener asignación por ID | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/UserSocietyAssignments` | Crear una nueva asignación | 201, 400, 401, 403, 500 |
| PUT | `/api/v1/UserSocietyAssignments` | Actualizar una asignación | 200, 400, 401, 403, 404, 500 |
| DELETE | `/api/v1/UserSocietyAssignments/{id}` | Eliminar una asignación | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/UserSocietyAssignments/{NombreCasoDeUso}/
├── {NombreCasoDeUso}Query.cs (o Command.cs)
├── {NombreCasoDeUso}QueryHandler.cs (o CommandHandler.cs)
└── {NombreCasoDeUso}Validator.cs (opcional)
```

### Validaciones

- Todos los validadores usan **FluentValidation**
- Se registran automáticamente con MediatR
- Validaciones comunes:
  - IDs: Mayor que 0
  - Email: Formato válido, máximo 255 caracteres
  - CuitClient: No vacío, máximo 12 caracteres
  - SociedadFi: No vacío, máximo 12 caracteres
  - Paginación: Page > 0, PageSize > 0

### Manejo de Errores

- Todos los handlers retornan `Result<T>` o `Result`
- Errores comunes:
  - `Error.NotFound` - Recurso no encontrado (404)
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

- `CreateUserSocietyAssignmentRequest` - Para crear asignaciones usuario-sociedad
- `UpdateUserSocietyAssignmentRequest` - Para actualizar asignaciones usuario-sociedad
- `GetAllUserSocietyAssignmentsRequest` - Para paginación (opcional)

### Response DTOs

- `UserSocietyAssignmentResponse` - Respuesta estándar de UserSocietyAssignment
- `PagedResponse<UserSocietyAssignmentResponse>` - Respuesta paginada

---

## Notas Técnicas

### Repositorio

El repositorio `UserSocietyAssignmentRepository` extiende `GenericRepository` y sobrescribe el método `GetPagedAsync` para ordenar por `Id`.

### Configuración de EF Core

La entidad está configurada para mapearse a la tabla `MultiProveedorRelacion` en la base de datos:
- `Email`: VARCHAR(50), NOT NULL
- `CuitClient`: VARCHAR(12), NOT NULL
- `SociedadFi`: VARCHAR(4), NOT NULL
- `Id`: INT, PRIMARY KEY, IDENTITY

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `UserSocietyAssignmentMappings.ToResponse` para mapear de la entidad de dominio a `UserSocietyAssignmentResponse`.

### Autenticación

Todos los endpoints requieren autenticación mediante el atributo `[Authorize]` en el controlador.

---

## Fecha de Creación

Documento creado: 2024-01-01

**Última actualización:** 2024-01-01

