# Ejemplos de uso de ResultExtensions.Match

Este documento muestra cómo usar los métodos de extensión `Match` en los controladores para simplificar el manejo de `Result<T>`.

## GET - Obtener recursos

### GET con datos (200 OK)
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<SocietyResponse>> GetById(int id, CancellationToken cancellationToken)
{
    GetSocietyByIdQuery query = new(id);
    Result<SocietyResponse> result = await Mediator.Send(query, cancellationToken);
    
    return result.Match(this); // Ok(200) o Problem
}
```

### GET con lista (200 OK)
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<SocietyResponse>>> GetAll(CancellationToken cancellationToken)
{
    GetAllSocietiesQuery query = new();
    Result<IEnumerable<SocietyResponse>> result = await Mediator.Send(query, cancellationToken);
    
    return result.Match(this); // Ok(200) o Problem
}
```

### GET con paginación (200 OK)
```csharp
[HttpGet("paged")]
public async Task<ActionResult<PagedResponse<SocietyResponse>>> GetAllPaged(
    [FromQuery] GetAllSocietiesRequest request, 
    CancellationToken cancellationToken)
{
    GetAllSocietiesPagedQuery query = new(request.Page ?? 1, request.PageSize ?? 20);
    Result<PagedResponse<SocietyResponse>> result = await Mediator.Send(query, cancellationToken);
    
    return result.Match(this); // Ok(200) o Problem
}
```

## POST - Crear recursos

### POST que devuelve el recurso creado (201 Created)
```csharp
[HttpPost]
public async Task<ActionResult<SocietyResponse>> Create(
    [FromBody] CreateSocietyRequest request, 
    CancellationToken cancellationToken)
{
    CreateSocietyCommand command = new(request.Codigo, request.Descripcion, request.Cuit);
    Result<SocietyResponse> result = await Mediator.Send(command, cancellationToken);
    
    // Opción 1: Usando CreatedAtAction
    return result.MatchCreated(this, nameof(GetById), new { id = result.Value.SocId });
    
    // Opción 2: Usando CreatedAtRoute (si tienes una ruta nombrada)
    // return result.MatchCreatedAtRoute(this, "GetSociety", new { id = result.Value.SocId });
}
```

### POST que no devuelve datos (204 NoContent)
```csharp
[HttpPost("{id}/activate")]
public async Task<ActionResult> Activate(int id, CancellationToken cancellationToken)
{
    ActivateSocietyCommand command = new(id);
    Result result = await Mediator.Send(command, cancellationToken);
    
    return result.Match(this); // NoContent(204) o Problem
}
```

## PUT - Actualizar recursos

### PUT que devuelve el recurso actualizado (200 OK)
```csharp
[HttpPut("{id}")]
public async Task<ActionResult<SocietyResponse>> Update(
    int id, 
    [FromBody] UpdateSocietyRequest request, 
    CancellationToken cancellationToken)
{
    UpdateSocietyCommand command = new(id, request.Descripcion, request.Cuit);
    Result<SocietyResponse> result = await Mediator.Send(command, cancellationToken);
    
    return result.MatchUpdated(this); // Ok(200) o Problem
}
```

### PUT que no devuelve datos (204 NoContent)
```csharp
[HttpPut("{id}")]
public async Task<ActionResult> Update(int id, [FromBody] UpdateSocietyRequest request, CancellationToken cancellationToken)
{
    UpdateSocietyCommand command = new(id, request.Descripcion, request.Cuit);
    Result result = await Mediator.Send(command, cancellationToken);
    
    return result.MatchUpdated(this); // NoContent(204) o Problem
}
```

## PATCH - Actualización parcial

### PATCH que devuelve el recurso actualizado (200 OK)
```csharp
[HttpPatch("{id}")]
public async Task<ActionResult<SocietyResponse>> Patch(
    int id, 
    [FromBody] PatchSocietyRequest request, 
    CancellationToken cancellationToken)
{
    PatchSocietyCommand command = new(id, request.Descripcion);
    Result<SocietyResponse> result = await Mediator.Send(command, cancellationToken);
    
    return result.MatchUpdated(this); // Ok(200) o Problem
}
```

## DELETE - Eliminar recursos

### DELETE (204 NoContent)
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
{
    DeleteSocietyCommand command = new(id);
    Result result = await Mediator.Send(command, cancellationToken);
    
    return result.MatchDeleted(this); // NoContent(204) o Problem
}
```

## Uso con lambdas personalizadas

Si necesitas un comportamiento personalizado, puedes usar las sobrecargas con lambdas:

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<SocietyResponse>> GetById(int id, CancellationToken cancellationToken)
{
    GetSocietyByIdQuery query = new(id);
    Result<SocietyResponse> result = await Mediator.Send(query, cancellationToken);
    
    return result.Match(
        onSuccess: value => Ok(value),
        onFailure: error => error.Error.Type == ErrorType.NotFound 
            ? NotFound() 
            : Problem(detail: error.Error.Description, statusCode: 500));
}
```

## Resumen de métodos disponibles

| Método | Uso | Código de éxito |
|--------|-----|-----------------|
| `Match(this)` | GET con datos, POST sin datos, PUT/PATCH sin datos | 200 OK / 204 NoContent |
| `MatchCreated(this, actionName, routeValues)` | POST que devuelve recurso creado | 201 Created |
| `MatchCreatedAtRoute(this, routeName, routeValues)` | POST que devuelve recurso creado (con ruta nombrada) | 201 Created |
| `MatchUpdated<T>(this)` | PUT/PATCH que devuelve recurso actualizado | 200 OK |
| `MatchUpdated(this)` | PUT/PATCH sin datos | 204 NoContent |
| `MatchDeleted(this)` | DELETE | 204 NoContent |

Todos los métodos devuelven `ProblemDetails` (RFC 7807) en caso de error, con el código de estado apropiado según el tipo de error.

