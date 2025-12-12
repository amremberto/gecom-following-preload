# Guía de Troubleshooting

## Error: "Invalid authorization code" al acceder a Swagger UI en QA

### Síntomas

Al intentar autenticarse en Swagger UI después de desplegar en QA, se produce el siguiente error:

```
Invalid authorization code {@values}, details: {@details}
Error: invalid_grant
```

### Causa

El error "Invalid authorization code" ocurre cuando:

1. **El `redirect_uri` no está registrado en IdentityServer**: Swagger UI usa automáticamente el `redirect_uri` `{baseUrl}/swagger/oauth2-redirect.html`. Este URI debe estar registrado en IdentityServer para el cliente de Swagger UI.

2. **El código de autorización ya fue usado**: Los códigos de autorización son de un solo uso. Si se intenta reutilizar un código, se producirá este error.

3. **El código de autorización expiró**: Los códigos tienen una vida útil limitada (típicamente 5-10 minutos).

4. **Problema con el almacenamiento de códigos**: Si IdentityServer almacena los códigos en memoria y hay múltiples instancias sin compartir estado, o si el servidor se reinició, los códigos se perderán.

### Solución

#### 1. Verificar el redirect_uri en IdentityServer

El `redirect_uri` que Swagger UI usa automáticamente es:
```
{baseUrl}/swagger/oauth2-redirect.html
```

Para QA, esto sería:
```
https://qa-fw-api-preload.gcgestion.com.ar/swagger/oauth2-redirect.html
```

**Asegúrate de que este `redirect_uri` esté registrado en IdentityServer para el cliente:**
- **ClientId**: `gecom.following.preload.api.swaggerui`

#### 2. Verificar la configuración del cliente en IdentityServer

En el proyecto de IdentityServer (`GeCom.IdentityServer.Sts`), verifica que el cliente `gecom.following.preload.api.swaggerui` tenga:

- **RedirectUris**: Debe incluir `https://qa-fw-api-preload.gcgestion.com.ar/swagger/oauth2-redirect.html`
- **AllowedGrantTypes**: Debe incluir `authorization_code`
- **RequirePkce**: Debe estar habilitado (true)
- **AllowOfflineAccess**: Opcional, según necesidades

#### 3. Verificar el almacenamiento de códigos de autorización

Si IdentityServer está usando almacenamiento en memoria para los códigos de autorización y hay múltiples instancias o reinicios, considera:

- Usar un almacenamiento distribuido (Redis, SQL Server, etc.) para los códigos de autorización
- Configurar `IPersistedGrantStore` en IdentityServer para persistir los códigos

#### 4. Verificar la configuración en este proyecto

Asegúrate de que la configuración en `identityServer.Staging.json` sea correcta:

```json
{
  "IdentityServer": {
    "Authority": "https://qa-accounts.gcgestion.com.ar",
    "MetadataAddress": "https://qa-accounts.gcgestion.com.ar/.well-known/openid-configuration",
    "ApiAudience": "gecom.following.preload.api",
    "RequiredScopes": [ "gecom.following.preload.api" ],
    "RequireHttpsMetadata": true,
    "SwaggerClientId": "gecom.following.preload.api.swaggerui",
    "SwaggerUsePkce": true,
    "OidcSwaggerUIClientId": "gecom.following.preload.api.swaggerui",
    "OidcApiName": "gecom.following.preload.api"
  }
}
```

### Verificación

Para verificar que el problema está resuelto:

1. Accede a Swagger UI: `https://qa-fw-api-preload.gcgestion.com.ar/swagger`
2. Haz clic en "Authorize"
3. Completa el flujo de autenticación
4. Verifica que no aparezca el error "Invalid authorization code"

### Referencias

- [Duende IdentityServer Troubleshooting](https://docs.duendesoftware.com/identityserver/troubleshooting/)
- [OAuth 2.0 Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/)

