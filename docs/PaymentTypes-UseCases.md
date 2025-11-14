# Casos de Uso - PaymentTypes

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `PaymentType`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllPaymentTypes](#getallpaymenttypes)
  - [GetPaymentTypeById](#getpaymenttypebyid)
  - [GetPaymentTypeByDescripcion](#getpaymenttypebydescripcion)
- [Commands (Comandos)](#commands-comandos)
  - [CreatePaymentType](#createpaymenttype)
  - [UpdatePaymentType](#updatepaymenttype)
  - [DeletePaymentType](#deletepaymenttype)

---

## Queries (Consultas)

### GetAllPaymentTypes

**Descripción:** Obtiene todos los tipos de pago sin paginación.

**Archivos:**
- `GetAllPaymentTypesQuery.cs`
- `GetAllPaymentTypesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/PaymentTypes
```

**Respuestas:**
- `200 OK` - Lista de tipos de pago
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/PaymentTypes
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "id": 1,
    "descripcion": "Efectivo"
  },
  {
    "id": 2,
    "descripcion": "Cheque"
  },
  {
    "id": 3,
    "descripcion": "Transferencia"
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`).

---

### GetPaymentTypeById

**Descripción:** Obtiene un tipo de pago por su ID.

**Archivos:**
- `GetPaymentTypeByIdQuery.cs`
- `GetPaymentTypeByIdQueryHandler.cs`
- `GetPaymentTypeByIdValidator.cs`

**Endpoint:**
```
GET /api/v1/PaymentTypes/{id}
```

**Parámetros:**
- `id` (path) - ID del tipo de pago (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Tipo de pago encontrado
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Tipo de pago no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/PaymentTypes/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "id": 1,
  "descripcion": "Efectivo"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "PaymentType.NotFound",
  "status": 404,
  "detail": "Payment type with ID '1' was not found.",
  "instance": "/api/v1/PaymentTypes/1"
}
```

---

### GetPaymentTypeByDescripcion

**Descripción:** Obtiene un tipo de pago por su descripción.

**Archivos:**
- `GetPaymentTypeByDescripcionQuery.cs`
- `GetPaymentTypeByDescripcionQueryHandler.cs`
- `GetPaymentTypeByDescripcionValidator.cs`

**Endpoint:**
```
GET /api/v1/PaymentTypes/by-descripcion/{descripcion}
```

**Parámetros:**
- `descripcion` (path) - Descripción del tipo de pago (requerido, no vacío)

**Respuestas:**
- `200 OK` - Tipo de pago encontrado
- `400 BadRequest` - Descripción inválida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Tipo de pago no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/PaymentTypes/by-descripcion/Efectivo
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "id": 1,
  "descripcion": "Efectivo"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "PaymentType.NotFound",
  "status": 404,
  "detail": "Payment type with description 'Efectivo' was not found.",
  "instance": "/api/v1/PaymentTypes/by-descripcion/Efectivo"
}
```

---

## Commands (Comandos)

### CreatePaymentType

**Descripción:** Crea un nuevo tipo de pago.

**Archivos:**
- `CreatePaymentTypeCommand.cs`
- `CreatePaymentTypeCommandHandler.cs`
- `CreatePaymentTypeCommandValidator.cs`

**DTO de Request:**
- `CreatePaymentTypeRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/PaymentTypes
```

**Body (JSON):**
```json
{
  "descripcion": "Tarjeta de Crédito"
}
```

**Validaciones:**
- `descripcion` - Requerido, no vacío, máximo 50 caracteres
- Verifica que no exista otro tipo de pago con la misma descripción

**Respuestas:**
- `201 Created` - Tipo de pago creado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `409 Conflict` - Tipo de pago con misma descripción ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/PaymentTypes
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Tarjeta de Crédito"
}
```

**Respuesta exitosa (201):**
```json
{
  "id": 4,
  "descripcion": "Tarjeta de Crédito"
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "PaymentType.Conflict",
  "status": 409,
  "detail": "A payment type with description 'Tarjeta de Crédito' already exists.",
  "instance": "/api/v1/PaymentTypes"
}
```

**Nota:** Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### UpdatePaymentType

**Descripción:** Actualiza un tipo de pago existente.

**Archivos:**
- `UpdatePaymentTypeCommand.cs`
- `UpdatePaymentTypeCommandHandler.cs`
- `UpdatePaymentTypeCommandValidator.cs`

**DTO de Request:**
- `UpdatePaymentTypeRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/PaymentTypes/{id}
```

**Parámetros:**
- `id` (path) - ID del tipo de pago a actualizar

**Body (JSON):**
```json
{
  "descripcion": "Efectivo Actualizado"
}
```

**Validaciones:**
- `id` - Requerido, mayor que 0
- `descripcion` - Requerido, no vacío, máximo 50 caracteres
- Verifica que el tipo de pago exista
- Verifica que no exista otro tipo de pago con la misma descripción (excluyendo el actual)

**Respuestas:**
- `200 OK` - Tipo de pago actualizado exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Tipo de pago no encontrado
- `409 Conflict` - Tipo de pago con misma descripción ya existe (excluyendo el actual)
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/PaymentTypes/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "descripcion": "Efectivo Actualizado"
}
```

**Respuesta exitosa (200):**
```json
{
  "id": 1,
  "descripcion": "Efectivo Actualizado"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "PaymentType.NotFound",
  "status": 404,
  "detail": "Payment type with ID '1' was not found.",
  "instance": "/api/v1/PaymentTypes/1"
}
```

**Nota:** El handler excluye el tipo de pago actual del chequeo de conflictos, permitiendo actualizaciones sin cambiar la descripción. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### DeletePaymentType

**Descripción:** Elimina un tipo de pago por su ID.

**Archivos:**
- `DeletePaymentTypeCommand.cs`
- `DeletePaymentTypeCommandHandler.cs`
- `DeletePaymentTypeCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/PaymentTypes/{id}
```

**Parámetros:**
- `id` (path) - ID del tipo de pago (debe ser mayor que 0)

**Validaciones:**
- `id` - Requerido, mayor que 0
- Verifica que el tipo de pago exista

**Respuestas:**
- `204 NoContent` - Tipo de pago eliminado exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Tipo de pago no encontrado
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/PaymentTypes/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "PaymentType.NotFound",
  "status": 404,
  "detail": "Payment type with ID '1' was not found.",
  "instance": "/api/v1/PaymentTypes/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar como eliminado sin borrar físicamente), se puede refactorizar el handler. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

**Advertencia:** Eliminar un tipo de pago puede afectar detalles de pago que lo referencian. Se recomienda verificar las relaciones antes de eliminar o implementar soft delete.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/PaymentTypes` | Obtener todos los tipos de pago | 200, 401, 403, 500 |
| GET | `/api/v1/PaymentTypes/{id}` | Obtener tipo de pago por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/PaymentTypes/by-descripcion/{descripcion}` | Obtener tipo de pago por descripción | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/PaymentTypes` | Crear un nuevo tipo de pago | 201, 400, 401, 403, 409, 500 |
| PUT | `/api/v1/PaymentTypes/{id}` | Actualizar un tipo de pago | 200, 400, 401, 403, 404, 409, 500 |
| DELETE | `/api/v1/PaymentTypes/{id}` | Eliminar un tipo de pago | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/PaymentTypes/{NombreCasoDeUso}/
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
  - Descripción: Máximo 50 caracteres (según configuración de BD)
  - Descripción única: Verificado en Create y Update

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
return result.MatchCreated(this, nameof(GetByDescripcionAsync), new { descripcion = request.Descripcion }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreatePaymentTypeRequest` - Para crear tipos de pago
  - `Descripcion` (string, requerido) - Descripción del tipo de pago (máximo 50 caracteres)
- `UpdatePaymentTypeRequest` - Para actualizar tipos de pago
  - `Descripcion` (string, requerido) - Nueva descripción del tipo de pago (máximo 50 caracteres)

### Response DTOs

- `PaymentTypeResponse` - Respuesta estándar de PaymentType
  - `Id` (int) - Identificador único del tipo de pago
  - `Descripcion` (string) - Descripción del tipo de pago

---

## Notas Técnicas

### Repositorio

El repositorio `PaymentTypeRepository` implementa `IPaymentTypeRepository` y extiende `GenericRepository<PaymentType, PreloadDbContext>`. Incluye:

- **GetByDescripcionAsync**: Método específico para obtener un tipo de pago por su descripción
- **GetByIdAsync**: Método heredado del GenericRepository que usa `Id` (int) como clave primaria

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `PaymentTypeMappings.ToResponse` para mapear de la entidad de dominio `PaymentType` a `PaymentTypeResponse`.

### Validación de Descripción Única

Tanto `CreatePaymentTypeCommandHandler` como `UpdatePaymentTypeCommandHandler` verifican que no exista otro tipo de pago con la misma descripción:

- En **Create**: Verifica que no exista ningún tipo de pago con la descripción proporcionada
- En **Update**: Verifica que no exista otro tipo de pago con la descripción proporcionada, excluyendo el tipo de pago actual

### Autorización

- **GET endpoints**: Requieren `RequirePreloadRead` (permite lectura a usuarios con roles: Administrator, PreloadReadOnly, PreloadAllSocieties, PreloadSingleSociety)
- **POST/PUT/DELETE endpoints**: Requieren `RequireAdministrator` (solo usuarios con rol Administrator)

### Relaciones

La entidad `PaymentType` tiene las siguientes relaciones:

- **PaymentDetails** - Colección de detalles de pago que usan este tipo de pago
  - Propiedad de navegación: `PaymentDetails`
  - Relación: Uno a muchos (un PaymentType puede tener muchos PaymentDetails)

**Nota:** Al eliminar un tipo de pago, se debe considerar el impacto en los detalles de pago que lo referencian. Se recomienda implementar soft delete o validar relaciones antes de eliminar.

### Configuración de Base de Datos

- **Tabla**: `TipoDePago`
- **Clave primaria**: `id` (int)
- **Campo**: `descripcion` (varchar(50), no unicode)

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado CRUD completo para PaymentTypes
  - GetAllPaymentTypes - Query para obtener todos los tipos de pago
  - GetPaymentTypeById - Query para obtener un tipo de pago por ID
  - GetPaymentTypeByDescripcion - Query para obtener un tipo de pago por descripción
  - CreatePaymentType - Command para crear un nuevo tipo de pago
  - UpdatePaymentType - Command para actualizar un tipo de pago existente
  - DeletePaymentType - Command para eliminar un tipo de pago
- **2024-12-19**: Agregado PaymentTypeResponse DTO y PaymentTypeMappings
- **2024-12-19**: Implementada validación de descripción única en Create y Update
- **2024-12-19**: Agregado PaymentTypesController con todos los endpoints
- **2024-12-19**: Configurada autorización (RequirePreloadRead para GET, RequireAdministrator para POST/PUT/DELETE)

