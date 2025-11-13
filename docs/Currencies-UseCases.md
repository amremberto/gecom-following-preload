# Casos de Uso - Currencies

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `Currency`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllCurrencies](#getallcurrencies)
  - [GetAllCurrenciesPaged](#getallcurrenciespaged)
  - [GetCurrencyById](#getcurrencybyid)
  - [GetCurrencyByCode](#getcurrencybycode)
- [Commands (Comandos)](#commands-comandos)
  - [CreateCurrency](#createcurrency)
  - [UpdateCurrency](#updatecurrency)
  - [DeleteCurrency](#deletecurrency)

---

## Queries (Consultas)

### GetAllCurrencies

**Descripción:** Obtiene todas las monedas sin paginación.

**Archivos:**
- `GetAllCurrenciesQuery.cs`
- `GetAllCurrenciesQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/Currencies
```

**Respuestas:**
- `200 OK` - Lista de monedas
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Currencies
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "monedaId": 1,
    "codigo": "ARS",
    "descripcion": "Peso Argentino",
    "codigoAfip": "032"
  },
  {
    "monedaId": 2,
    "codigo": "USD",
    "descripcion": "Dólar Estadounidense",
    "codigoAfip": "001"
  }
]
```

**Nota:** Este endpoint requiere autenticación. Todos los campos son requeridos excepto `codigoAfip` que puede ser null.

---

### GetAllCurrenciesPaged

**Descripción:** Obtiene monedas con paginación.

**Archivos:**
- `GetAllCurrenciesPagedQuery.cs`
- `GetAllCurrenciesPagedQueryHandler.cs`
- `GetAllCurrenciesPagedValidator.cs`

**Endpoint:**
```
GET /api/v1/Currencies/paged?page=1&pageSize=20
```

**Parámetros:**
- `page` (opcional, default: 1) - Número de página (1-based)
- `pageSize` (opcional, default: 20) - Tamaño de página

**Respuestas:**
- `200 OK` - PagedResponse con monedas
- `400 BadRequest` - Parámetros de paginación inválidos
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Currencies/paged?page=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "items": [
    {
      "monedaId": 1,
      "codigo": "ARS",
      "descripcion": "Peso Argentino",
      "codigoAfip": "032"
    },
    {
      "monedaId": 2,
      "codigo": "USD",
      "descripcion": "Dólar Estadounidense",
      "codigoAfip": "001"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasPrevious": false,
  "hasNext": true
}
```

**Nota:** Este caso de uso utiliza el método `GetPagedAsync` del repositorio, que está sobrescrito en `CurrencyRepository` para ordenar por `MonedaId`.

---

### GetCurrencyById

**Descripción:** Obtiene una moneda por su ID.

**Archivos:**
- `GetCurrencyByIdQuery.cs`
- `GetCurrencyByIdQueryHandler.cs`
- `GetCurrencyByIdValidator.cs`

**Endpoint:**
```
GET /api/v1/Currencies/id/{id}
```

**Parámetros:**
- `id` (path) - ID de la moneda (debe ser mayor que 0)

**Respuestas:**
- `200 OK` - Moneda encontrada
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Moneda no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Currencies/id/1
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "monedaId": 1,
  "codigo": "ARS",
  "descripcion": "Peso Argentino",
  "codigoAfip": "032"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Currency.NotFound",
  "status": 404,
  "detail": "Currency with ID '1' was not found.",
  "instance": "/api/v1/Currencies/id/1"
}
```

**Nota:** El repositorio `CurrencyRepository` sobrescribe `GetByIdAsync` porque `Currency` usa `Codigo` (string) como clave primaria, pero se busca por `MonedaId` (int).

---

### GetCurrencyByCode

**Descripción:** Obtiene una moneda por su código.

**Archivos:**
- `GetCurrencyByCodeQuery.cs`
- `GetCurrencyByCodeQueryHandler.cs`
- `GetCurrencyByCodeValidator.cs`

**Endpoint:**
```
GET /api/v1/Currencies/{codigo}
```

**Parámetros:**
- `codigo` (path) - Código de la moneda (requerido, no vacío)

**Respuestas:**
- `200 OK` - Moneda encontrada
- `400 BadRequest` - Código inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Moneda no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/Currencies/ARS
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "monedaId": 1,
  "codigo": "ARS",
  "descripcion": "Peso Argentino",
  "codigoAfip": "032"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Currency.NotFound",
  "status": 404,
  "detail": "Currency with code 'ARS' was not found.",
  "instance": "/api/v1/Currencies/ARS"
}
```

**Nota:** El código de la moneda es único y se usa como clave primaria en la base de datos.

---

## Commands (Comandos)

### CreateCurrency

**Descripción:** Crea una nueva moneda.

**Archivos:**
- `CreateCurrencyCommand.cs`
- `CreateCurrencyCommandHandler.cs`
- `CreateCurrencyCommandValidator.cs`

**DTO de Request:**
- `CreateCurrencyRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/Currencies
```

**Body (JSON):**
```json
{
  "codigo": "EUR",
  "descripcion": "Euro",
  "codigoAfip": "060"
}
```

**Validaciones:**
- `codigo` - Requerido, no vacío, máximo 4 caracteres
- `descripcion` - Requerido, no vacío
- `codigoAfip` - Opcional, máximo 3 caracteres
- Verifica que no exista otra moneda con el mismo código

**Respuestas:**
- `201 Created` - Moneda creada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `409 Conflict` - Moneda con mismo código ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/Currencies
Content-Type: application/json
Authorization: Bearer {token}

{
  "codigo": "EUR",
  "descripcion": "Euro",
  "codigoAfip": "060"
}
```

**Respuesta exitosa (201):**
```json
{
  "monedaId": 3,
  "codigo": "EUR",
  "descripcion": "Euro",
  "codigoAfip": "060"
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "Currency.Conflict",
  "status": 409,
  "detail": "A currency with code 'EUR' already exists.",
  "instance": "/api/v1/Currencies"
}
```

**Nota:** Este endpoint requiere permisos de administrador (`RequireAdministrator`). El código de la moneda debe ser único.

---

### UpdateCurrency

**Descripción:** Actualiza una moneda existente.

**Archivos:**
- `UpdateCurrencyCommand.cs`
- `UpdateCurrencyCommandHandler.cs`
- `UpdateCurrencyCommandValidator.cs`

**DTO de Request:**
- `UpdateCurrencyRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/Currencies
```

**Body (JSON):**
```json
{
  "monedaId": 1,
  "codigo": "ARS",
  "descripcion": "Peso Argentino Actualizado",
  "codigoAfip": "032"
}
```

**Validaciones:**
- `monedaId` - Requerido, mayor que 0
- `codigo` - Requerido, no vacío, máximo 4 caracteres
- `descripcion` - Requerido, no vacío
- `codigoAfip` - Opcional, máximo 3 caracteres
- Verifica que la moneda exista
- Verifica que no exista otra moneda con el mismo código (excluyendo la actual)

**Respuestas:**
- `200 OK` - Moneda actualizada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Moneda no encontrada
- `409 Conflict` - Moneda con mismo código ya existe (excluyendo la actual)
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/Currencies
Content-Type: application/json
Authorization: Bearer {token}

{
  "monedaId": 1,
  "codigo": "ARS",
  "descripcion": "Peso Argentino Actualizado",
  "codigoAfip": "032"
}
```

**Respuesta exitosa (200):**
```json
{
  "monedaId": 1,
  "codigo": "ARS",
  "descripcion": "Peso Argentino Actualizado",
  "codigoAfip": "032"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Currency.NotFound",
  "status": 404,
  "detail": "Currency with ID '1' was not found.",
  "instance": "/api/v1/Currencies"
}
```

**Nota:** El handler excluye la moneda actual del chequeo de conflictos, permitiendo actualizaciones sin cambiar el código. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

---

### DeleteCurrency

**Descripción:** Elimina una moneda por su ID.

**Archivos:**
- `DeleteCurrencyCommand.cs`
- `DeleteCurrencyCommandHandler.cs`
- `DeleteCurrencyCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/Currencies/{id}
```

**Parámetros:**
- `id` (path) - ID de la moneda (debe ser mayor que 0)

**Validaciones:**
- `id` - Requerido, mayor que 0
- Verifica que la moneda exista

**Respuestas:**
- `204 NoContent` - Moneda eliminada exitosamente
- `400 BadRequest` - ID inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos (requiere `RequireAdministrator`)
- `404 NotFound` - Moneda no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/Currencies/1
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Currency.NotFound",
  "status": 404,
  "detail": "Currency with ID '1' was not found.",
  "instance": "/api/v1/Currencies/1"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Si necesitas **soft delete** (marcar como eliminada en lugar de eliminar), se puede refactorizar el handler. Este endpoint requiere permisos de administrador (`RequireAdministrator`).

**Advertencia:** Eliminar una moneda puede afectar documentos que la referencian. Se recomienda verificar las relaciones antes de eliminar o implementar soft delete.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/Currencies` | Obtener todas las monedas | 200, 401, 403, 500 |
| GET | `/api/v1/Currencies/paged` | Obtener monedas paginadas | 200, 400, 401, 403, 500 |
| GET | `/api/v1/Currencies/id/{id}` | Obtener moneda por ID | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/Currencies/{codigo}` | Obtener moneda por código | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/Currencies` | Crear una nueva moneda | 201, 400, 401, 403, 409, 500 |
| PUT | `/api/v1/Currencies` | Actualizar una moneda | 200, 400, 401, 403, 404, 409, 500 |
| DELETE | `/api/v1/Currencies/{id}` | Eliminar una moneda | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Preload/Currencies/{NombreCasoDeUso}/
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
  - Código: Máximo 4 caracteres
  - Código AFIP: Máximo 3 caracteres (opcional)
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
return result.MatchCreated(this, nameof(GetByCodigo), new { codigo = request.Codigo }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateCurrencyRequest` - Para crear monedas
  - `Codigo` (string, requerido) - Código único de la moneda (máximo 4 caracteres)
  - `Descripcion` (string, requerido) - Descripción de la moneda
  - `CodigoAfip` (string?, opcional) - Código AFIP de la moneda (máximo 3 caracteres)
- `UpdateCurrencyRequest` - Para actualizar monedas
  - `MonedaId` (int, requerido) - ID de la moneda a actualizar
  - `Codigo` (string, requerido) - Código único de la moneda (máximo 4 caracteres)
  - `Descripcion` (string, requerido) - Descripción de la moneda
  - `CodigoAfip` (string?, opcional) - Código AFIP de la moneda (máximo 3 caracteres)
- `GetAllCurrenciesRequest` - Para paginación (opcional)
  - `Page` (int?, opcional) - Número de página
  - `PageSize` (int?, opcional) - Tamaño de página

### Response DTOs

- `CurrencyResponse` - Respuesta estándar de Currency
  - `MonedaId` (int) - Identificador único de la moneda
  - `Codigo` (string) - Código único de la moneda
  - `Descripcion` (string) - Descripción de la moneda
  - `CodigoAfip` (string?) - Código AFIP de la moneda (nullable)
- `PagedResponse<CurrencyResponse>` - Respuesta paginada

---

## Notas Técnicas

### Repositorio

El repositorio `CurrencyRepository` implementa `ICurrencyRepository` y extiende `GenericRepository<Currency, PreloadDbContext>`. Incluye:

- **GetByCodeAsync**: Método específico para obtener una moneda por su código
- **GetByIdAsync**: Método sobrescrito porque Currency usa `Codigo` (string) como clave primaria, pero se busca por `MonedaId` (int)
- **GetPagedAsync**: Método sobrescrito para ordenar por `MonedaId`

### Clave Primaria

La entidad `Currency` tiene una particularidad: usa `Codigo` (string) como clave primaria en la base de datos, pero también tiene `MonedaId` (int) como identificador único. Por esta razón:

- `GetByIdAsync` busca por `MonedaId` en lugar de usar `FindAsync` con la clave primaria
- `GetByCodeAsync` busca por `Codigo` (la clave primaria real)

### Unit of Work

Todos los comandos (Create, Update, Delete) usan `IUnitOfWork.SaveChangesAsync` para persistir los cambios en la base de datos.

### Mapeo

Todos los casos de uso usan `CurrencyMappings.ToResponse` para mapear de la entidad de dominio `Currency` a `CurrencyResponse`.

### Validación de Código Único

Tanto `CreateCurrencyCommandHandler` como `UpdateCurrencyCommandHandler` verifican que no exista otra moneda con el mismo código:

- En **Create**: Verifica que no exista ninguna moneda con el código proporcionado
- En **Update**: Verifica que no exista otra moneda con el código proporcionado, excluyendo la moneda actual

### Autorización

- **GET endpoints**: Requieren autenticación (`[Authorize]`)
- **POST/PUT/DELETE endpoints**: Requieren permisos de administrador (`RequireAdministrator`)

### Relaciones

La entidad `Currency` tiene las siguientes relaciones:

- **Documents** - Colección de documentos que usan esta moneda

**Nota:** Al eliminar una moneda, se debe considerar el impacto en los documentos que la referencian. Se recomienda implementar soft delete o validar relaciones antes de eliminar.

### Configuración de Base de Datos

La entidad `Currency` está configurada en `CurrencyConfigurations`:

- Tabla: `Monedas`
- Clave primaria: `Codigo` (string, máximo 4 caracteres)
- `MonedaId`: Generado automáticamente
- `CodigoAfip`: Máximo 3 caracteres, nullable

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

### Cambios Recientes

- **2024-12-19**: Implementado CRUD completo para Currencies
  - GetAllCurrencies - Query para obtener todas las monedas
  - GetAllCurrenciesPaged - Query para obtener monedas paginadas
  - GetCurrencyById - Query para obtener una moneda por ID
  - GetCurrencyByCode - Query para obtener una moneda por código
  - CreateCurrency - Command para crear una nueva moneda
  - UpdateCurrency - Command para actualizar una moneda existente
  - DeleteCurrency - Command para eliminar una moneda
- **2024-12-19**: Agregado CurrencyResponse DTO y CurrencyMappings
- **2024-12-19**: Implementada validación de código único en Create y Update
- **2024-12-19**: Agregado CurrenciesController con todos los endpoints
- **2024-12-19**: Configurada autorización (autenticación requerida para GET, RequireAdministrator para POST/PUT/DELETE)
- **2024-12-19**: Actualizado CurrencyRepository con GetPagedAsync ordenado por MonedaId

