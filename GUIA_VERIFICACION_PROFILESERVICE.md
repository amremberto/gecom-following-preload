# Guía para Verificar el ProfileService en IdentityServer

Esta guía te ayudará a verificar que el ProfileService en IdentityServer esté mapeando correctamente los claims del usuario a los claims del token.

## 1. Ubicar el ProfileService

El ProfileService generalmente se encuentra en el proyecto de IdentityServer. Busca archivos como:
- `ProfileService.cs`
- `CustomProfileService.cs`
- `UserProfileService.cs`
- O cualquier clase que implemente `IProfileService`

### Ubicación típica:
```
src/IdentityServer/
  └── Services/
      └── ProfileService.cs
```

## 2. Verificar la Implementación del ProfileService

### 2.1. Verificar que implementa IProfileService

El ProfileService debe implementar la interfaz `IProfileService` de Duende IdentityServer:

```csharp
public class ProfileService : IProfileService
{
    // ...
}
```

### 2.2. Verificar el método GetProfileDataAsync

Este método es responsable de mapear los claims del usuario a los claims del token. Debe incluir:

1. **Claims del IdentityResource `profile`**:
   - `name`
   - `given_name`
   - `family_name`
   - `preferred_username`
   - `middle_name`
   - `nickname`
   - `picture`
   - `website`
   - `gender`
   - `birthdate`
   - `zoneinfo`
   - `locale`
   - `updated_at`

2. **Claims del IdentityResource `email`**:
   - `email`
   - `email_verified`

3. **Claims del IdentityResource `roles`** (si aplica):
   - `role`

### Ejemplo de implementación correcta:

```csharp
public async Task GetProfileDataAsync(ProfileDataRequestContext context)
{
    // Obtener el usuario
    var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
    if (user == null) return;

    var claims = new List<Claim>();

    // Claims del IdentityResource "profile"
    if (!string.IsNullOrWhiteSpace(user.Name))
    {
        claims.Add(new Claim("name", user.Name));
    }
    
    if (!string.IsNullOrWhiteSpace(user.GivenName))
    {
        claims.Add(new Claim("given_name", user.GivenName));
    }
    
    if (!string.IsNullOrWhiteSpace(user.FamilyName))
    {
        claims.Add(new Claim("family_name", user.FamilyName));
    }
    
    if (!string.IsNullOrWhiteSpace(user.PreferredUsername))
    {
        claims.Add(new Claim("preferred_username", user.PreferredUsername));
    }

    // Claims del IdentityResource "email"
    if (!string.IsNullOrWhiteSpace(user.Email))
    {
        claims.Add(new Claim("email", user.Email));
        claims.Add(new Claim("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant()));
    }

    // Claims del IdentityResource "roles"
    var roles = await _userManager.GetRolesAsync(user);
    foreach (var role in roles)
    {
        claims.Add(new Claim("role", role));
    }

    // Agregar los claims solicitados
    context.IssuedClaims.AddRange(claims);
}
```

## 3. Verificar que los Claims se Incluyan en el Token

### 3.1. Verificar los RequestedClaimTypes

El método `GetProfileDataAsync` debe verificar qué claims se están solicitando y solo incluir esos:

```csharp
public async Task GetProfileDataAsync(ProfileDataRequestContext context)
{
    // context.RequestedClaimTypes contiene los claims solicitados
    // basados en los IdentityResources y ApiScopes configurados
    
    var requestedClaimTypes = context.RequestedClaimTypes;
    
    // Solo agregar los claims que se están solicitando
    if (requestedClaimTypes.Contains("name"))
    {
        claims.Add(new Claim("name", user.Name));
    }
    
    // ... etc
}
```

### 3.2. Verificar que los Claims se Agreguen Correctamente

Asegúrate de que los claims se agreguen a `context.IssuedClaims`:

```csharp
context.IssuedClaims.AddRange(claims);
```

## 4. Verificar la Configuración de IdentityResources

Verifica que los IdentityResources estén configurados correctamente en tu archivo de configuración (el que compartiste):

```json
{
  "IdentityResources": [
    {
      "Name": "profile",
      "Enabled": true,
      "DisplayName": "User profile",
      "UserClaims": [
        "name",
        "family_name",
        "given_name",
        "middle_name",
        "nickname",
        "preferred_username",
        "profile",
        "picture",
        "website",
        "gender",
        "birthdate",
        "zoneinfo",
        "locale",
        "updated_at"
      ]
    },
    {
      "Name": "email",
      "Enabled": true,
      "DisplayName": "Your email address",
      "UserClaims": [
        "email",
        "email_verified"
      ]
    }
  ]
}
```

## 5. Verificar los Datos del Usuario

Asegúrate de que el usuario en IdentityServer tenga datos de nombre configurados:

1. **Verificar en la base de datos**:
   - Busca la tabla de usuarios (generalmente `AspNetUsers` o similar)
   - Verifica que el usuario tenga valores en:
     - `Name` o `FullName`
     - `GivenName` o `FirstName`
     - `FamilyName` o `LastName`
     - `Email`
     - `PreferredUsername` o `UserName`

2. **Verificar en el código del ProfileService**:
   - Asegúrate de que el ProfileService esté leyendo correctamente los datos del usuario
   - Verifica que no haya valores nulos o vacíos

## 6. Probar el ProfileService

### 6.1. Agregar Logs de Depuración

Agrega logs en el ProfileService para ver qué claims se están agregando:

```csharp
public async Task GetProfileDataAsync(ProfileDataRequestContext context)
{
    _logger.LogInformation("GetProfileDataAsync called for subject: {Subject}", context.Subject.GetSubjectId());
    _logger.LogInformation("Requested claim types: {ClaimTypes}", string.Join(", ", context.RequestedClaimTypes));
    
    // ... código para obtener claims ...
    
    _logger.LogInformation("Issued claims: {Claims}", string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));
    
    context.IssuedClaims.AddRange(claims);
}
```

### 6.2. Verificar los Logs

1. Inicia sesión en la aplicación WebApp
2. Revisa los logs de IdentityServer
3. Verifica que los claims se estén agregando correctamente

## 7. Verificar el UserInfo Endpoint

El UserInfo endpoint debe devolver los mismos claims que se incluyen en el token. Puedes probarlo manualmente:

### 7.1. Obtener un Access Token

1. Inicia sesión en la aplicación WebApp
2. Obtén el access token de la cookie o del almacenamiento

### 7.2. Llamar al UserInfo Endpoint

```bash
curl -X GET "https://localhost:7100/connect/userinfo" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 7.3. Verificar la Respuesta

La respuesta debe incluir los claims del IdentityResource `profile`:

```json
{
  "sub": "60480359-d888-4e64-c041-08de1f8b7cbc",
  "name": "Juan Pérez",
  "given_name": "Juan",
  "family_name": "Pérez",
  "preferred_username": "juan.perez",
  "email": "juan.perez@example.com",
  "email_verified": true
}
```

## 8. Checklist de Verificación

- [ ] El ProfileService implementa `IProfileService`
- [ ] El método `GetProfileDataAsync` está implementado
- [ ] Los claims del IdentityResource `profile` se están agregando
- [ ] Los claims del IdentityResource `email` se están agregando
- [ ] Los claims se agregan a `context.IssuedClaims`
- [ ] Los datos del usuario tienen valores (no nulos o vacíos)
- [ ] Los IdentityResources están configurados correctamente
- [ ] Los logs muestran que los claims se están agregando
- [ ] El UserInfo endpoint devuelve los claims correctos

## 9. Problemas Comunes y Soluciones

### Problema: Los claims no aparecen en el token

**Solución**: Verifica que:
1. Los IdentityResources estén configurados correctamente
2. El cliente tenga acceso a los scopes necesarios (`profile`, `email`)
3. El ProfileService esté agregando los claims a `context.IssuedClaims`

### Problema: Los claims aparecen en el UserInfo pero no en el id_token

**Solución**: Esto es normal. El id_token solo incluye claims básicos. Los claims adicionales se obtienen del UserInfo endpoint, que ya está configurado en la aplicación WebApp.

### Problema: Los datos del usuario están vacíos

**Solución**: Verifica que:
1. El usuario tenga datos configurados en la base de datos
2. El ProfileService esté leyendo correctamente los datos del usuario
3. No haya errores en el mapeo de datos

## 10. Próximos Pasos

1. Ubica el ProfileService en tu proyecto de IdentityServer
2. Verifica que esté mapeando correctamente los claims
3. Agrega logs de depuración si es necesario
4. Prueba el flujo completo de autenticación
5. Verifica que los claims aparezcan en `/debug/claims` en la aplicación WebApp

