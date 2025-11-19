# Solución al Problema: Email Claim No Incluido en Access Token

## Problema Identificado

El IdentityServer está emitiendo el scope `"email"` en el access token, pero **NO está incluyendo el claim `"email"`** en el token mismo. Esto causa que la WebAPI no pueda obtener el email del usuario para validar y obtener los documentos correspondientes.

### Evidencia del Problema

Los logs muestran que el token contiene:
- ✅ Scope: `"email"` (solicitado correctamente)
- ❌ Claim: `"email"` (NO está presente en el token)

**Claims disponibles en el token:**
- `iss`, `nbf`, `iat`, `exp`, `aud`, `scope`
- `name`: "Remberto Aguilar - Outlook"
- `role`: "Following.Preload.Societies"
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`: GUID del usuario
- **NO hay claim `"email"`**

## Solución: Configurar IdentityServer para Incluir el Claim Email

El problema está en la configuración del **ProfileService** o en los **ApiScopes/ApiResources** del IdentityServer. El claim `"email"` debe incluirse en el access token cuando se solicita el scope `"email"`.

### Paso 1: Verificar el ProfileService

Ubica el `ProfileService` en tu proyecto de IdentityServer (generalmente en `src/IdentityServer/Services/ProfileService.cs` o similar).

El método `GetProfileDataAsync` debe incluir el claim `"email"` cuando se solicita:

```csharp
public async Task GetProfileDataAsync(ProfileDataRequestContext context)
{
    // Obtener el usuario
    var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
    if (user == null) return;

    var claims = new List<Claim>();

    // Verificar qué claims se están solicitando
    var requestedClaimTypes = context.RequestedClaimTypes;

    // Incluir el claim "email" si se está solicitando
    if (requestedClaimTypes.Contains("email") && !string.IsNullOrWhiteSpace(user.Email))
    {
        claims.Add(new Claim("email", user.Email));
        claims.Add(new Claim("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant()));
    }

    // Agregar otros claims necesarios...
    // (roles, name, etc.)

    // IMPORTANTE: Agregar los claims a context.IssuedClaims
    context.IssuedClaims.AddRange(claims);
}
```

### Paso 2: Verificar la Configuración de ApiScopes/ApiResources

Asegúrate de que el ApiScope o ApiResource tenga configurado el claim `"email"` en sus `UserClaims`:

```csharp
// En la configuración del IdentityServer (Program.cs o Config.cs)
new ApiScope
{
    Name = "gecom.following.preload.api",
    DisplayName = "GeCom Following Preload API",
    UserClaims = 
    {
        "role",
        "email",  // ← Asegúrate de que esto esté incluido
        "name"
    }
}
```

O si usas ApiResource:

```csharp
new ApiResource
{
    Name = "gecom.following.preload.api",
    DisplayName = "GeCom Following Preload API",
    UserClaims = 
    {
        "role",
        "email",  // ← Asegúrate de que esto esté incluido
        "name"
    }
}
```

### Paso 3: Verificar que el IdentityResource "email" esté Configurado

El IdentityResource `"email"` debe estar configurado correctamente:

```csharp
new IdentityResources.Email()  // Esto ya incluye "email" y "email_verified"
```

### Paso 4: Verificar que el Cliente Tenga Acceso al Scope

Asegúrate de que el cliente (WebApp) tenga acceso al scope `"email"`:

```csharp
new Client
{
    ClientId = "gecom.following.preload.webapp",
    // ... otras configuraciones ...
    AllowedScopes = 
    {
        "openid",
        "profile",
        "email",  // ← Asegúrate de que esto esté incluido
        "roles",
        "gecom.following.preload.api",
        "offline_access"
    }
}
```

## Verificación

Después de hacer los cambios:

1. **Reinicia el IdentityServer**
2. **Cierra sesión y vuelve a iniciar sesión** en la WebApp para obtener un nuevo token
3. **Verifica los logs de la WebAPI** - deberías ver que el email se encuentra correctamente
4. **Opcionalmente, decodifica el JWT** en [jwt.io](https://jwt.io) y verifica que el claim `"email"` esté presente en el payload

## Solución Temporal (Workaround)

Si no puedes modificar el IdentityServer inmediatamente, el código actual de la WebAPI intentará obtener el email de múltiples formas:

1. Busca el claim `"email"` en varios formatos
2. Intenta extraerlo del claim `"sub"` si contiene un email
3. Intenta extraerlo del claim `"upn"` si contiene un email
4. Intenta decodificar el token directamente

Sin embargo, **la solución correcta es arreglar el IdentityServer** para que incluya el claim `"email"` en el access token.

## Referencias

- Ver `GUIA_VERIFICACION_PROFILESERVICE.md` para más detalles sobre cómo verificar el ProfileService
- Documentación de Duende IdentityServer: [Profile Service](https://docs.duendesoftware.com/identityserver/v6/fundamentals/claims/profileservice)

