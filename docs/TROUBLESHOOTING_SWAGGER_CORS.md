# Troubleshooting: Error CORS en Swagger UI con IdentityServer

## Error

Al intentar autenticarse en Swagger UI, se produce el siguiente error en la consola del navegador:

```
Access to fetch at 'https://qa-accounts.gcgestion.com.ar/connect/token' 
from origin 'https://qa-fw-api-preload.gcgestion.com.ar' has been blocked 
by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.

POST https://qa-accounts.gcgestion.com.ar/connect/token net::ERR_FAILED 500 (Internal Server Error)
```

**Nota importante**: El error 500 (Internal Server Error) es la causa raíz. Cuando IdentityServer devuelve un error 500, no envía los headers CORS, lo que causa que el navegador bloquee la solicitud. El problema real es el error 500, no CORS en sí.

## ⚠️ CAUSA RAÍZ IDENTIFICADA

El error real es un problema de **permisos del certificado**:

```
System.Security.Cryptography.CryptographicException: The system cannot find the file specified.
at System.Security.Cryptography.CngKey.Open(...)
```

IdentityServer no puede acceder a la **clave privada del certificado** que usa para firmar los tokens JWT. El Application Pool de IIS (`IIS APPPOOL\QA-Accounts`) no tiene permisos para leer la clave privada del certificado.

## Causa

El error ocurre porque IdentityServer está devolviendo un error 500 (Internal Server Error) al procesar la solicitud al endpoint `/connect/token`. Cuando hay un error 500, IdentityServer no envía los headers CORS, lo que causa que el navegador bloquee la solicitud.

El error 500 generalmente está relacionado con:

1. **Código de autorización inválido**: El código de autorización no se encuentra en el almacenamiento (PersistedGrantStore) o ya fue usado/consumido.

2. **Problema con el almacenamiento de códigos**: Si IdentityServer está usando almacenamiento en memoria y hay múltiples instancias, o si el servidor se reinició, los códigos se pierden.

3. **Problema con PKCE**: El `code_verifier` no coincide con el `code_challenge` usado al generar el código.

4. **CORS como síntoma secundario**: Aunque el cliente tiene `AllowedCorsOrigins` configurado, cuando hay un error 500, los headers CORS no se envían, causando el error de CORS en el navegador.

## Solución

### 🔧 SOLUCIÓN PRINCIPAL: Permisos del Certificado

El problema es que el Application Pool de IIS no tiene permisos para acceder al archivo del certificado o a su clave privada. Sigue estos pasos según tu caso:

#### CASO 1: Certificado como archivo físico (.pfx, .p12, etc.)

Si el certificado es un archivo en la carpeta de la aplicación (por ejemplo, `certificates/cert.pfx`):

##### Paso 1: Identificar la ruta del certificado

Revisa la configuración de `SigningCertificate` en IdentityServer para encontrar la ruta exacta del archivo del certificado.

##### Paso 2: Otorgar permisos al archivo del certificado

1. Navega a la carpeta donde está el certificado (por ejemplo, `C:\inetpub\wwwroot\GeCom.IdentityServer.Sts\certificates\`)
2. Haz clic derecho en el archivo del certificado (`.pfx` o `.p12`)
3. Selecciona **Properties** > **Security**
4. Haz clic en **Edit...**
5. Haz clic en **Add...**
6. Ingresa: `IIS APPPOOL\QA-Accounts`
7. Otorga permisos de **Read**
8. Haz clic en **OK**

**Alternativa usando PowerShell** (ejecutar como Administrador):

```powershell
# Ruta del archivo del certificado (ajusta según tu configuración)
$certPath = "C:\inetpub\wwwroot\GeCom.IdentityServer.Sts\certificates\tu-certificado.pfx"
$account = "IIS APPPOOL\QA-Accounts"

# Otorgar permisos de lectura al Application Pool
$acl = Get-Acl $certPath
$permission = $account, "Read", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $certPath $acl
```

##### Paso 3: Otorgar permisos a la carpeta (si es necesario)

Si el certificado está en una subcarpeta, también otorga permisos a la carpeta:

```powershell
# Ruta de la carpeta del certificado
$certFolder = "C:\inetpub\wwwroot\GeCom.IdentityServer.Sts\certificates"
$account = "IIS APPPOOL\QA-Accounts"

$acl = Get-Acl $certFolder
$permission = $account, "Read,ReadAndExecute", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $certFolder $acl
```

##### Paso 4: ⚠️ CRÍTICO - Otorgar permisos a la clave privada en el perfil de usuario

Cuando IdentityServer carga un certificado `.pfx` en IIS con `UserKeySet`, extrae la clave privada y la almacena en el perfil de usuario del Application Pool. El Application Pool necesita acceso a esta ubicación.

**Ubicación de la clave privada con `UserKeySet`:**
```
C:\Users\IIS APPPOOL\QA-Accounts\AppData\Roaming\Microsoft\Crypto\RSA\
```

**Solución usando PowerShell** (ejecutar como Administrador):

```powershell
$account = "IIS APPPOOL\QA-Accounts"
$userProfile = "C:\Users\IIS APPPOOL\QA-Accounts"
$rsaPath = "$userProfile\AppData\Roaming\Microsoft\Crypto\RSA"

# Crear la carpeta del perfil de usuario si no existe
if (-not (Test-Path $userProfile)) {
    New-Item -ItemType Directory -Path $userProfile -Force
    # Otorgar permisos al perfil de usuario
    $acl = Get-Acl $userProfile
    $permission = $account, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $userProfile $acl
}

# Crear la estructura de carpetas si no existe
$cryptoPath = "$userProfile\AppData\Roaming\Microsoft\Crypto"
if (-not (Test-Path $cryptoPath)) {
    New-Item -ItemType Directory -Path $cryptoPath -Force -Recurse
}

if (-not (Test-Path $rsaPath)) {
    New-Item -ItemType Directory -Path $rsaPath -Force
}

# Otorgar permisos completos a la carpeta RSA
$acl = Get-Acl $rsaPath
$permission = $account, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $rsaPath $acl

# También otorgar permisos a la carpeta Crypto
if (Test-Path $cryptoPath) {
    $acl = Get-Acl $cryptoPath
    $permission = $account, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $cryptoPath $acl
}
```

**Alternativa: Usar MachineKeySet en lugar de UserKeySet**

Si prefieres usar `MachineKeySet` (la clave privada se almacena en una ubicación compartida), puedes configurarlo en `signing-certificate.json`:

```json
{
  "SigningCertificate": {
    "RelativePath": "certificates/gecom_idsrv.pfx",
    "KeyStorageFlags": "MachineKeySet,PersistKeySet"
  }
}
```

Luego otorga permisos a la carpeta MachineKeys:

```powershell
$account = "IIS APPPOOL\QA-Accounts"
$machineKeysPath = "C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys"

$acl = Get-Acl $machineKeysPath
$permission = $account, "Read", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $machineKeysPath $acl
```

#### CASO 2: Certificado instalado en Certificate Store

Si el certificado está instalado en el Certificate Store de Windows:

##### Paso 1: Identificar el certificado

1. Abre **certlm.msc** (Certificate Manager) en el servidor
2. Navega a **Personal > Certificates**
3. Encuentra el certificado que IdentityServer está usando

##### Paso 2: Otorgar permisos a la clave privada

1. Haz clic derecho en el certificado > **All Tasks > Manage Private Keys...**
2. Haz clic en **Add...**
3. Ingresa: `IIS APPPOOL\QA-Accounts`
4. Otorga permisos de **Read**
5. Haz clic en **OK**

**Alternativa usando PowerShell** (ejecutar como Administrador):

```powershell
# Obtener el thumbprint del certificado
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*tu-certificado*" }
$thumbprint = $cert.Thumbprint

# Obtener la ruta de la clave privada
$rsaCert = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($cert)
$keyPath = $rsaCert.Key.UniqueName

# Otorgar permisos al Application Pool
$keyPath = "C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys\$keyPath"
$account = "IIS APPPOOL\QA-Accounts"
$acl = Get-Acl $keyPath
$permission = $account, "Read", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $keyPath $acl
```

#### Paso 5: Reiniciar el Application Pool

1. Abre **IIS Manager**
2. Selecciona **Application Pools**
3. Encuentra el Application Pool de IdentityServer (probablemente `QA-Accounts`)
4. Haz clic derecho > **Recycle** o **Stop** y luego **Start**

#### Paso 6: Verificar

Después de otorgar los permisos y reiniciar, intenta autenticarte nuevamente en Swagger UI. El error debería desaparecer.

### 1. Verificar los logs de IdentityServer

**PASO CRÍTICO**: Revisa los logs de IdentityServer para identificar la causa exacta del error 500. Busca mensajes como:
- "Invalid authorization code"
- "Authorization code not found"
- "Code verifier mismatch"
- Cualquier excepción relacionada con `PersistedGrantStore`

### 2. Verificar el almacenamiento de códigos de autorización

Verifica que los códigos de autorización se estén almacenando correctamente en la base de datos:

```sql
SELECT TOP 10 
    Key, 
    Type, 
    ClientId, 
    CreationTime, 
    Expiration, 
    ConsumedTime
FROM PersistedGrants
WHERE ClientId = 'gecom.following.preload.api.swaggerui'
    AND Type = 'authorization_code'
ORDER BY CreationTime DESC
```

- Si no hay códigos, el problema es que no se están generando/almacenando.
- Si hay códigos pero `ConsumedTime` no es NULL, el código ya fue usado (son de un solo uso).
- Si hay códigos pero `Expiration` es anterior a ahora, el código expiró.

### 3. Verificar la configuración del cliente en IdentityServer

En el archivo de configuración de seed (`identityserver-seed.Staging.json`), el cliente `gecom.following.preload.api.swaggerui` debe tener:

```json
{
  "ClientId": "gecom.following.preload.api.swaggerui",
  "AllowedCorsOrigins": [
    "https://qa-fw-api-preload.gcgestion.com.ar"
  ]
}
```

**Importante**: El origen debe coincidir exactamente, incluyendo:
- Protocolo: `https://` (no `http://`)
- Dominio completo: `qa-fw-api-preload.gcgestion.com.ar`
- Sin barra final: no debe terminar en `/`
- Sin puerto: a menos que se use un puerto específico

### 4. Verificar que el cliente esté correctamente seedeado en la base de datos

Si el cliente se configuró manualmente en la base de datos, verifica que la tabla `ClientCorsOrigins` tenga una entrada para el cliente con el origen correcto:

```sql
SELECT c.ClientId, co.Origin
FROM Clients c
INNER JOIN ClientCorsOrigins co ON c.Id = co.ClientId
WHERE c.ClientId = 'gecom.following.preload.api.swaggerui'
```

Debe mostrar:
```
ClientId: gecom.following.preload.api.swaggerui
Origin: https://qa-fw-api-preload.gcgestion.com.ar
```

### 5. Verificar que IdentityServer esté usando la configuración correcta

Asegúrate de que IdentityServer esté cargando la configuración del cliente desde la base de datos y no desde memoria. Duende IdentityServer usa `AddConfigurationStore` para cargar la configuración desde la base de datos.

### 6. Verificar el tiempo de vida del código de autorización

El tiempo de vida por defecto de un código de autorización es de 5 minutos. Si el usuario tarda más de 5 minutos en completar el flujo, el código expirará. Verifica la configuración de `AuthorizationCodeLifetime` en IdentityServer.

### 7. Reiniciar IdentityServer después de cambios

Si se modificó la configuración del cliente, es necesario reiniciar IdentityServer para que los cambios surtan efecto.

### 8. Verificar los headers CORS en la respuesta (solo si el error 500 está resuelto)

Puedes verificar si IdentityServer está enviando los headers CORS correctos haciendo una solicitud OPTIONS (preflight) al endpoint:

```bash
curl -X OPTIONS https://qa-accounts.gcgestion.com.ar/connect/token \
  -H "Origin: https://qa-fw-api-preload.gcgestion.com.ar" \
  -H "Access-Control-Request-Method: POST" \
  -v
```

La respuesta debe incluir:
- `Access-Control-Allow-Origin: https://qa-fw-api-preload.gcgestion.com.ar`
- `Access-Control-Allow-Methods: POST`
- `Access-Control-Allow-Headers: ...`

## Caso Especial: Funciona en Desarrollo pero no en QA

Si el problema ocurre solo en QA pero funciona correctamente en Visual Studio (desarrollo local), puede ser un problema con cómo Swagger UI está detectando la URL base en QA.

### Síntomas
- ✅ Funciona en Visual Studio (desarrollo local)
- ❌ No funciona en QA
- ✅ La autenticación SSO funciona en otras páginas (confirma que IdentityServer funciona)
- ❌ Error 500 en `/connect/token` solo desde Swagger UI en QA

### Posibles Causas

1. **Redirect URI no coincide exactamente**: Swagger UI construye automáticamente el `redirect_uri` como `{currentOrigin}/swagger/oauth2-redirect.html`. En QA, puede que la URL base no se esté detectando correctamente.

2. **Problema con la detección de la URL base**: Swagger UI puede estar usando una URL diferente a la esperada debido a:
   - Headers de proxy/load balancer
   - Configuración de IIS
   - Variables de entorno

### Solución

1. **Verificar el redirect_uri real usado**: Abre la consola del navegador en QA y verifica qué `redirect_uri` se está enviando en la solicitud a `/connect/token`. Debe ser exactamente: `https://qa-fw-api-preload.gcgestion.com.ar/swagger/oauth2-redirect.html`

2. **Verificar en los logs de IdentityServer**: Busca en los logs el `redirect_uri` que se está recibiendo y compáralo con el configurado en el cliente.

3. **Verificar configuración de IIS/Proxy**: Asegúrate de que los headers `X-Forwarded-Proto`, `X-Forwarded-Host`, etc. estén configurados correctamente si hay un proxy/load balancer delante.

## Verificación

Para verificar que el problema está resuelto:

1. Abre Swagger UI: `https://qa-fw-api-preload.gcgestion.com.ar/swagger`
2. Haz clic en "Authorize"
3. Deberías ser redirigido a IdentityServer para autenticarte
4. Después de autenticarte, deberías ser redirigido de vuelta a Swagger UI
5. No deberían aparecer errores de CORS en la consola del navegador

## Referencias

- [Duende IdentityServer CORS Documentation](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/cors/)
- [OAuth 2.0 Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/)

