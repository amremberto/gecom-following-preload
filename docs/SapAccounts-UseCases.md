# Casos de Uso - SapAccounts

Este documento contiene un resumen completo de todos los casos de uso implementados para la entidad `SapAccount`.

## Tabla de Contenidos

- [Queries (Consultas)](#queries-consultas)
  - [GetAllSapAccounts](#getallsapaccounts)
  - [GetAllSapAccountsPaged](#getallsapaccountspaged)
  - [GetSapAccountByAccountNumber](#getsapaccountbyaccountnumber)
  - [GetSapAccountByCuit](#getsapaccountbycuit)
- [Commands (Comandos)](#commands-comandos)
  - [CreateSapAccount](#createsapaccount)
  - [UpdateSapAccount](#updatesapaccount)
  - [DeleteSapAccount](#deletesapaccount)

---

## Queries (Consultas)

### GetAllSapAccounts

**Descripción:** Obtiene todas las cuentas SAP sin paginación.

**Archivos:**
- `GetAllSapAccountsQuery.cs`
- `GetAllSapAccountsQueryHandler.cs`

**Endpoint:**
```
GET /api/v1/SapAccounts
```

**Respuestas:**
- `200 OK` - Lista de cuentas SAP
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/SapAccounts
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
[
  {
    "accountnumber": "000001",
    "name": "Proveedor Ejemplo S.A.",
    "address1City": "Buenos Aires",
    "address1Stateorprovince": "CABA",
    "address1Postalcode": "1000",
    "address1Line1": "Av. Corrientes 1234",
    "telephone1": "11-1234-5678",
    "fax": "11-1234-5679",
    "address1Country": "Argentina",
    "newCuit": "20123456789",
    "newBloqueado": "N",
    "newRubro": "Tecnología",
    "newIibb": "123456789",
    "emailaddress1": "contacto@proveedor.com",
    "customertypecode": 11,
    "newGproveedor": "Grupo A",
    "cbu": "1234567890123456789012"
  }
]
```

**Nota:** Este endpoint requiere autenticación y permisos de lectura (`RequirePreloadRead`). Todos los campos son opcionales excepto `accountnumber`.

---

### GetAllSapAccountsPaged

**Descripción:** Obtiene cuentas SAP con paginación.

**Archivos:**
- `GetAllSapAccountsPagedQuery.cs`
- `GetAllSapAccountsPagedQueryHandler.cs`
- `GetAllSapAccountsPagedValidator.cs`

**Endpoint:**
```
GET /api/v1/SapAccounts/paged?page=1&pageSize=20
```

**Parámetros:**
- `page` (opcional, default: 1) - Número de página (1-based)
- `pageSize` (opcional, default: 20) - Tamaño de página

**Respuestas:**
- `200 OK` - PagedResponse con cuentas SAP
- `400 BadRequest` - Parámetros de paginación inválidos
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/SapAccounts/paged?page=1&pageSize=20
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "items": [
    {
      "accountnumber": "000001",
      "name": "Proveedor Ejemplo S.A.",
      "address1City": "Buenos Aires",
      "address1Stateorprovince": "CABA",
      "address1Postalcode": "1000",
      "address1Line1": "Av. Corrientes 1234",
      "telephone1": "11-1234-5678",
      "fax": "11-1234-5679",
      "address1Country": "Argentina",
      "newCuit": "20123456789",
      "newBloqueado": "N",
      "newRubro": "Tecnología",
      "newIibb": "123456789",
      "emailaddress1": "contacto@proveedor.com",
      "customertypecode": 11,
      "newGproveedor": "Grupo A",
      "cbu": "1234567890123456789012"
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

**Nota:** Este caso de uso utiliza el método `GetPagedAsync` del repositorio, ordenado por `Accountnumber`.

---

### GetSapAccountByAccountNumber

**Descripción:** Obtiene una cuenta SAP por su número de cuenta.

**Archivos:**
- `GetSapAccountByAccountNumberQuery.cs`
- `GetSapAccountByAccountNumberQueryHandler.cs`
- `GetSapAccountByAccountNumberValidator.cs`

**Endpoint:**
```
GET /api/v1/SapAccounts/{accountNumber}
```

**Parámetros:**
- `accountNumber` (path) - Número de cuenta (requerido, máximo 20 caracteres)

**Respuestas:**
- `200 OK` - Cuenta SAP encontrada
- `400 BadRequest` - Número de cuenta inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Cuenta SAP no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/SapAccounts/000001
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo S.A.",
  "address1City": "Buenos Aires",
  "address1Stateorprovince": "CABA",
  "address1Postalcode": "1000",
  "address1Line1": "Av. Corrientes 1234",
  "telephone1": "11-1234-5678",
  "fax": "11-1234-5679",
  "address1Country": "Argentina",
  "newCuit": "20123456789",
  "newBloqueado": "N",
  "newRubro": "Tecnología",
  "newIibb": "123456789",
  "emailaddress1": "contacto@proveedor.com",
  "customertypecode": 11,
  "newGproveedor": "Grupo A",
  "cbu": "1234567890123456789012"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "SapAccount.NotFound",
  "status": 404,
  "detail": "SAP account with account number '000001' was not found.",
  "instance": "/api/v1/SapAccounts/000001"
}
```

---

### GetSapAccountByCuit

**Descripción:** Obtiene una cuenta SAP por su CUIT.

**Archivos:**
- `GetSapAccountByCuitQuery.cs`
- `GetSapAccountByCuitQueryHandler.cs`
- `GetSapAccountByCuitValidator.cs`

**Endpoint:**
```
GET /api/v1/SapAccounts/cuit/{cuit}
```

**Parámetros:**
- `cuit` (path) - CUIT de la cuenta (requerido, no vacío)

**Respuestas:**
- `200 OK` - Cuenta SAP encontrada
- `400 BadRequest` - CUIT inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Cuenta SAP no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
GET /api/v1/SapAccounts/cuit/20123456789
Authorization: Bearer {token}
```

**Respuesta exitosa:**
```json
{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo S.A.",
  "address1City": "Buenos Aires",
  "address1Stateorprovince": "CABA",
  "address1Postalcode": "1000",
  "address1Line1": "Av. Corrientes 1234",
  "telephone1": "11-1234-5678",
  "fax": "11-1234-5679",
  "address1Country": "Argentina",
  "newCuit": "20123456789",
  "newBloqueado": "N",
  "newRubro": "Tecnología",
  "newIibb": "123456789",
  "emailaddress1": "contacto@proveedor.com",
  "customertypecode": 11,
  "newGproveedor": "Grupo A",
  "cbu": "1234567890123456789012"
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "SapAccount.NotFound",
  "status": 404,
  "detail": "SAP account with CUIT '20123456789' was not found.",
  "instance": "/api/v1/SapAccounts/cuit/20123456789"
}
```

---

## Commands (Comandos)

### CreateSapAccount

**Descripción:** Crea una nueva cuenta SAP.

**Archivos:**
- `CreateSapAccountCommand.cs`
- `CreateSapAccountCommandHandler.cs`
- `CreateSapAccountCommandValidator.cs`

**DTO de Request:**
- `CreateSapAccountRequest.cs` (en Contracts)

**Endpoint:**
```
POST /api/v1/SapAccounts
```

**Body (JSON):**
```json
{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo S.A.",
  "address1City": "Buenos Aires",
  "address1Stateorprovince": "CABA",
  "address1Postalcode": "1000",
  "address1Line1": "Av. Corrientes 1234",
  "telephone1": "11-1234-5678",
  "fax": "11-1234-5679",
  "address1Country": "Argentina",
  "newCuit": "20123456789",
  "newBloqueado": "N",
  "newRubro": "Tecnología",
  "newIibb": "123456789",
  "emailaddress1": "contacto@proveedor.com",
  "customertypecode": 11,
  "newGproveedor": "Grupo A",
  "cbu": "1234567890123456789012"
}
```

**Validaciones:**
- `accountnumber` - Requerido, no vacío, máximo 20 caracteres
- `name` - Opcional, máximo 250 caracteres
- `address1City` - Opcional, máximo 250 caracteres
- `address1Stateorprovince` - Opcional, máximo 250 caracteres
- `address1Postalcode` - Opcional, máximo 50 caracteres
- `address1Line1` - Opcional, máximo 250 caracteres
- `telephone1` - Opcional, máximo 50 caracteres
- `fax` - Opcional, máximo 50 caracteres
- `address1Country` - Opcional, máximo 80 caracteres
- `emailaddress1` - Opcional, máximo 250 caracteres, formato de email válido
- `cbu` - Opcional, máximo 22 caracteres
- Verifica que no exista otra cuenta SAP con el mismo número de cuenta

**Respuestas:**
- `201 Created` - Cuenta SAP creada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `409 Conflict` - Cuenta SAP con mismo número de cuenta ya existe
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
POST /api/v1/SapAccounts
Content-Type: application/json
Authorization: Bearer {token}

{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo S.A.",
  "address1City": "Buenos Aires",
  "newCuit": "20123456789",
  "emailaddress1": "contacto@proveedor.com"
}
```

**Respuesta exitosa (201):**
```json
{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo S.A.",
  "address1City": "Buenos Aires",
  "address1Stateorprovince": null,
  "address1Postalcode": null,
  "address1Line1": null,
  "telephone1": null,
  "fax": null,
  "address1Country": null,
  "newCuit": "20123456789",
  "newBloqueado": null,
  "newRubro": null,
  "newIibb": null,
  "emailaddress1": "contacto@proveedor.com",
  "customertypecode": null,
  "newGproveedor": null,
  "cbu": null
}
```

**Respuesta cuando hay conflicto (409):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
  "title": "SapAccount.Conflict",
  "status": 409,
  "detail": "A SAP account with account number '000001' already exists.",
  "instance": "/api/v1/SapAccounts"
}
```

---

### UpdateSapAccount

**Descripción:** Actualiza una cuenta SAP existente.

**Archivos:**
- `UpdateSapAccountCommand.cs`
- `UpdateSapAccountCommandHandler.cs`
- `UpdateSapAccountCommandValidator.cs`

**DTO de Request:**
- `UpdateSapAccountRequest.cs` (en Contracts)

**Endpoint:**
```
PUT /api/v1/SapAccounts/{accountNumber}
```

**Parámetros:**
- `accountNumber` (path) - Número de cuenta a actualizar

**Body (JSON):**
```json
{
  "name": "Proveedor Ejemplo Actualizado S.A.",
  "address1City": "Córdoba",
  "address1Stateorprovince": "Córdoba",
  "address1Postalcode": "5000",
  "address1Line1": "Av. Colón 5678",
  "telephone1": "351-9876-5432",
  "fax": "351-9876-5433",
  "address1Country": "Argentina",
  "newCuit": "20123456789",
  "newBloqueado": "S",
  "newRubro": "Servicios",
  "newIibb": "987654321",
  "emailaddress1": "nuevo@proveedor.com",
  "customertypecode": 12,
  "newGproveedor": "Grupo B",
  "cbu": "9876543210987654321098"
}
```

**Validaciones:**
- `accountNumber` (path) - Requerido, no vacío, máximo 20 caracteres
- Todos los campos del body son opcionales con las mismas validaciones que Create
- Verifica que la cuenta SAP exista

**Respuestas:**
- `200 OK` - Cuenta SAP actualizada exitosamente
- `400 BadRequest` - Validación fallida
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Cuenta SAP no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
PUT /api/v1/SapAccounts/000001
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Proveedor Ejemplo Actualizado S.A.",
  "address1City": "Córdoba",
  "emailaddress1": "nuevo@proveedor.com"
}
```

**Respuesta exitosa (200):**
```json
{
  "accountnumber": "000001",
  "name": "Proveedor Ejemplo Actualizado S.A.",
  "address1City": "Córdoba",
  "address1Stateorprovince": null,
  "address1Postalcode": null,
  "address1Line1": null,
  "telephone1": null,
  "fax": null,
  "address1Country": null,
  "newCuit": null,
  "newBloqueado": null,
  "newRubro": null,
  "newIibb": null,
  "emailaddress1": "nuevo@proveedor.com",
  "customertypecode": null,
  "newGproveedor": null,
  "cbu": null
}
```

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "SapAccount.NotFound",
  "status": 404,
  "detail": "SAP account with account number '000001' was not found.",
  "instance": "/api/v1/SapAccounts/000001"
}
```

---

### DeleteSapAccount

**Descripción:** Elimina una cuenta SAP por su número de cuenta.

**Archivos:**
- `DeleteSapAccountCommand.cs`
- `DeleteSapAccountCommandHandler.cs`
- `DeleteSapAccountCommandValidator.cs`

**Endpoint:**
```
DELETE /api/v1/SapAccounts/{accountNumber}
```

**Parámetros:**
- `accountNumber` (path) - Número de cuenta (requerido, máximo 20 caracteres)

**Validaciones:**
- `accountNumber` - Requerido, no vacío, máximo 20 caracteres
- Verifica que la cuenta SAP exista

**Respuestas:**
- `204 NoContent` - Cuenta SAP eliminada exitosamente
- `400 BadRequest` - Número de cuenta inválido
- `401 Unauthorized` - Usuario no autenticado
- `403 Forbidden` - Usuario sin permisos
- `404 NotFound` - Cuenta SAP no encontrada
- `500 InternalServerError` - Error del servidor

**Ejemplo de uso:**
```http
DELETE /api/v1/SapAccounts/000001
Authorization: Bearer {token}
```

**Respuesta exitosa (204):**
- Sin contenido en el cuerpo

**Respuesta cuando no se encuentra (404):**
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "SapAccount.NotFound",
  "status": 404,
  "detail": "SAP account with account number '000001' was not found.",
  "instance": "/api/v1/SapAccounts/000001"
}
```

**Nota:** Actualmente se usa **hard delete** (eliminación física). Esta entidad proviene de una base de datos externa de SAP, por lo que se debe tener precaución al eliminar registros.

---

## Resumen de Endpoints

| Método | Endpoint | Descripción | Códigos de Respuesta |
|--------|----------|-------------|---------------------|
| GET | `/api/v1/SapAccounts` | Obtener todas las cuentas SAP | 200, 401, 403, 500 |
| GET | `/api/v1/SapAccounts/paged` | Obtener cuentas SAP paginadas | 200, 400, 401, 403, 500 |
| GET | `/api/v1/SapAccounts/{accountNumber}` | Obtener cuenta SAP por número de cuenta | 200, 400, 401, 403, 404, 500 |
| GET | `/api/v1/SapAccounts/cuit/{cuit}` | Obtener cuenta SAP por CUIT | 200, 400, 401, 403, 404, 500 |
| POST | `/api/v1/SapAccounts` | Crear una nueva cuenta SAP | 201, 400, 401, 403, 409, 500 |
| PUT | `/api/v1/SapAccounts/{accountNumber}` | Actualizar una cuenta SAP | 200, 400, 401, 403, 404, 500 |
| DELETE | `/api/v1/SapAccounts/{accountNumber}` | Eliminar una cuenta SAP | 204, 400, 401, 403, 404, 500 |

---

## Patrones y Convenciones

### Estructura de Archivos

Todos los casos de uso siguen la siguiente estructura:

```
Application/Features/Spd_Sap/SapAccounts/{NombreCasoDeUso}/
├── {NombreCasoDeUso}Query.cs (o Command.cs)
├── {NombreCasoDeUso}QueryHandler.cs (o CommandHandler.cs)
└── {NombreCasoDeUso}Validator.cs (opcional)
```

### Validaciones

- Todos los validadores usan **FluentValidation**
- Se registran automáticamente con MediatR
- Validaciones comunes:
  - `accountnumber`: Requerido, no vacío, máximo 20 caracteres
  - Strings opcionales: Validación de longitud máxima según configuración de EF Core
  - Email: Validación de formato cuando se proporciona
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
return result.MatchCreated(this, nameof(GetByAccountNumberAsync), new { accountNumber = request.Accountnumber }); // 201 Created

// PUT
return result.MatchUpdated(this); // Ok(200)

// DELETE
return result.MatchDeleted(this); // NoContent(204)
```

---

## DTOs Utilizados

### Request DTOs

- `CreateSapAccountRequest` - Para crear cuentas SAP
- `UpdateSapAccountRequest` - Para actualizar cuentas SAP
- `GetAllSapAccountsRequest` - Para paginación (opcional)

### Response DTOs

- `SapAccountResponse` - Respuesta estándar de SapAccount
- `PagedResponse<SapAccountResponse>` - Respuesta paginada

---

## Notas Técnicas

### Repositorio Específico

El repositorio `SapAccountRepository` extiende `GenericRepository` y proporciona métodos específicos:
- `GetByAccountNumberAsync` - Obtiene por número de cuenta (sin tracking)
- `GetByAccountNumberForUpdateAsync` - Obtiene por número de cuenta (con tracking para actualizaciones)
- `GetByCuitAsync` - Obtiene por CUIT
- `RemoveByAccountNumberAsync` - Elimina por número de cuenta
- `GetPagedAsync` - Sobrescrito para ordenar por `Accountnumber`

### Unit of Work Específico

Esta entidad utiliza `ISpdSapUnitOfWork` en lugar de `IUnitOfWork` porque pertenece a la base de datos `SpdSapDbContext`, que es diferente de la base de datos principal `PreloadDbContext`.

### Mapeo

Todos los casos de uso usan `SapAccountMappings.ToResponse` para mapear de la entidad de dominio a `SapAccountResponse`.

### Base de Datos Externa

**Importante:** Esta entidad proviene de una base de datos externa de SAP (`SpdSapDbContext`). A diferencia de las entidades del dominio Preload, `SapAccount` no tiene campos de auditoría como `FechaCreacion` o `FechaBaja`. Se debe tener precaución al realizar operaciones de escritura (Create, Update, Delete) ya que pueden afectar datos del sistema SAP.

---

## Fecha de Creación

Documento creado: 2024-12-19

**Última actualización:** 2024-12-19

