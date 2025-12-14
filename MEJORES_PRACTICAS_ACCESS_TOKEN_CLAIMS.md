# Mejores Prácticas: Claims en Access Tokens con IdentityServer y SSO

## Resumen Ejecutivo

Este documento explica las mejores prácticas sobre qué claims deben incluirse en los **access tokens** cuando se usa IdentityServer con SSO y múltiples aplicaciones web que acceden a WebAPIs.

## Problema Identificado

El claim `"email"` no está llegando en el access token aunque el scope `"email"` está siendo solicitado. Esto ocurre porque:

1. ✅ El **ProfileService** está correctamente configurado para incluir el claim `"email"`
2. ✅ El **IdentityResource "email"** está correctamente configurado
3. ❌ El **ApiScope "gecom.following.preload.api"** NO incluye `"email"` en sus `UserClaims`

## Arquitectura: Access Token vs ID Token vs UserInfo

### Access Token
- **Propósito**: Autenticación y autorización para acceder a APIs
- **Contenido**: Claims necesarios para que la API pueda autorizar y procesar requests
- **Tamaño**: Debe ser pequeño (afecta performance en cada request)
- **Lifetime**: Generalmente corto (1 hora típicamente)

### ID Token
- **Propósito**: Identificar al usuario en la aplicación cliente
- **Contenido**: Claims básicos del usuario (sub, name, email, etc.)
- **Tamaño**: Puede ser más grande
- **Lifetime**: Muy corto (solo para el flujo de autenticación)

### UserInfo Endpoint
- **Propósito**: Obtener información adicional del usuario cuando se necesita
- **Contenido**: Todos los claims del usuario según los IdentityResources solicitados
- **Uso**: Llamada adicional HTTP (afecta performance)

## Mejores Prácticas: ¿Qué Debe Ir en el Access Token?

### ✅ DEBE Incluirse en el Access Token

1. **Claims de Identificación Básica** (si la API los necesita):
   - `sub` (subject) - Siempre incluido
   - `name` - Si la API necesita mostrar el nombre del usuario
   - `email` - **Si la API necesita filtrar/validar por email** ⚠️ **ESTE ES TU CASO**

2. **Claims de Autorización**:
   - `role` - Roles del usuario para autorización
   - `permission` - Permisos específicos si se usan
   - Claims personalizados de autorización (ej: `following.provider.cuit`)

3. **Claims de Contexto de la API**:
   - Claims específicos que la API necesita para procesar requests
   - Ejemplo: `provider_cuit`, `society_cuit`, etc.

### ❌ NO Debe Incluirse en el Access Token

1. **Información Sensible**:
   - Contraseñas o hashes
   - Información financiera completa
   - Datos médicos completos

2. **Información Voluminosa**:
   - Listas grandes de permisos
   - Historial completo del usuario
   - Preferencias detalladas

3. **Información que Cambia Frecuentemente**:
   - Última actividad
   - Estado en tiempo real
   - Notificaciones pendientes

### ⚠️ Consideraciones Especiales

1. **Email en Access Token**:
   - ✅ **SÍ incluirlo** si la API necesita filtrar/validar por email
   - ✅ **SÍ incluirlo** si se usa para búsquedas o asignaciones
   - ❌ **NO incluirlo** si solo se usa para mostrar en la UI (usa ID Token o UserInfo)

2. **Tamaño del Token**:
   - Cada claim adicional aumenta el tamaño
   - Tokens grandes afectan performance (headers HTTP)
   - Límite práctico: ~4KB (algunos proxies limitan a 8KB)

## Solución al Problema Actual

### Problema
El ApiScope `"gecom.following.preload.api"` no incluye `"email"` en sus `UserClaims`, por lo que aunque el ProfileService lo incluye cuando se solicita el IdentityResource `"email"`, no se está agregando al access token para este scope específico.

### Solución

**Opción 1: Agregar "email" al ApiScope (RECOMENDADO)**

Modificar `identityserver-seed.json`:

```json
{
  "Name": "gecom.following.preload.api",
  "DisplayName": "GeCom Following Preload API",
  "Description": "Scope para acceder a la API GeCom Following Preload",
  "enabled": true,
  "Required": false,
  "Emphasize": false,
  "ShowInDiscoveryDocument": true,
  "UserClaims": [
    "role",
    "name",
    "email",  // ← AGREGAR ESTO
    "permission"
  ]
}
```

**Opción 2: Agregar "email" al ApiResource**

Modificar `identityserver-seed.json`:

```json
{
  "Name": "gecom.following.preload.api",
  "DisplayName": "GeCom Following Preload API",
  "Enabled": true,
  "Scopes": [
    "gecom.following.preload.api"
  ],
  "userClaims": [
    "role",
    "email",  // ← AGREGAR ESTO
    "permission"
  ],
  "showInDiscoveryDocument": true
}
```

**Recomendación**: Usar **Opción 1** (ApiScope) porque:
- Los ApiScopes son más granulares
- Permite diferentes claims para diferentes APIs
- Es la práctica recomendada en IdentityServer 6+

### Pasos para Aplicar la Solución

1. **Modificar el archivo de configuración**:
   - Editar `gecom-identityserver/src/GeCom.IdentityServer.Sts/Configurations/jsons/identityserver-seed.json`
   - Agregar `"email"` a `UserClaims` del ApiScope `"gecom.following.preload.api"`

2. **Actualizar la base de datos**:
   - Opción A: Eliminar el ApiScope existente y ejecutar el seed nuevamente (solo en desarrollo)
   - Opción B: Actualizar manualmente en la base de datos:
     ```sql
     INSERT INTO ApiScopeClaims (ScopeId, Type)
     SELECT Id, 'email'
     FROM ApiScopes
     WHERE Name = 'gecom.following.preload.api'
     AND NOT EXISTS (
         SELECT 1 FROM ApiScopeClaims 
         WHERE ScopeId = ApiScopes.Id AND Type = 'email'
     );
     ```

3. **Reiniciar IdentityServer**

4. **Probar**:
   - Cerrar sesión y volver a iniciar sesión
   - Verificar que el claim `"email"` esté presente en el access token

## Configuración Actual vs Recomendada

### Configuración Actual

```json
{
  "ApiScopes": [
    {
      "Name": "gecom.following.preload.api",
      "UserClaims": [
        "role",
        "name",
        "permission"
        // ❌ Falta "email"
      ]
    }
  ]
}
```

### Configuración Recomendada

```json
{
  "ApiScopes": [
    {
      "Name": "gecom.following.preload.api",
      "UserClaims": [
        "role",
        "name",
        "email",  // ✅ Agregado porque la API lo necesita
        "permission"
      ]
    }
  ]
}
```

## Flujo de Claims en IdentityServer

```
1. Cliente solicita scopes: ["openid", "profile", "email", "gecom.following.preload.api"]
   ↓
2. IdentityServer valida los scopes solicitados
   ↓
3. ProfileService.GetProfileDataAsync() es llamado
   ↓
4. ProfileService incluye claims basado en:
   - IdentityResources solicitados (openid, profile, email, roles)
   - ApiScope/ApiResource solicitados (gecom.following.preload.api)
   ↓
5. IdentityServer combina los claims:
   - Claims de IdentityResources (si están en RequestedClaimTypes)
   - Claims de ApiScope/ApiResource (si están en UserClaims)
   ↓
6. Access Token se genera con los claims combinados
```

## Checklist de Verificación

Para cada ApiScope/ApiResource, verifica:

- [ ] ¿La API necesita el claim para autorización? → Incluirlo
- [ ] ¿La API necesita el claim para filtrar datos? → Incluirlo
- [ ] ¿La API necesita el claim para validación? → Incluirlo
- [ ] ¿El claim es sensible? → Considerar alternativas (UserInfo endpoint)
- [ ] ¿El claim es voluminoso? → Considerar alternativas (UserInfo endpoint)
- [ ] ¿El claim cambia frecuentemente? → Considerar alternativas (UserInfo endpoint)

## Ejemplos por Tipo de Claim

### Claims de Identificación
```json
{
  "UserClaims": [
    "sub",        // Siempre incluido
    "name",       // Si la API muestra el nombre
    "email"       // Si la API filtra/valida por email
  ]
}
```

### Claims de Autorización
```json
{
  "UserClaims": [
    "role",       // Roles para autorización
    "permission"  // Permisos específicos
  ]
}
```

### Claims Personalizados
```json
{
  "UserClaims": [
    "following.provider.cuit",  // CUIT del proveedor asignado
    "provider.cuit"            // CUIT del proveedor
  ]
}
```

## Referencias

- [Duende IdentityServer - Profile Service](https://docs.duendesoftware.com/identityserver/v6/fundamentals/claims/profileservice)
- [Duende IdentityServer - Resources](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources)
- [OAuth 2.0 Access Tokens Best Practices](https://oauth.net/2/access-tokens/)
- [OpenID Connect Core 1.0 - Claims](https://openid.net/specs/openid-connect-core-1_0.html#Claims)

