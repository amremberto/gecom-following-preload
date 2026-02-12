# Deshabilitación de WebDAV en IIS

Este documento describe por qué y cómo deshabilitar el módulo WebDAV en IIS cuando se hospedan APIs REST (por ejemplo, ASP.NET Core) que usan verbos HTTP como `PUT`, `PATCH` o `DELETE`.

---

## 1. ¿Qué es WebDAV?

**WebDAV** (Web Distributed Authoring and Versioning) es una extensión de HTTP que permite editar y gestionar archivos en un servidor web de forma remota (subir, borrar, mover, bloquear, etc.). IIS lo incluye por defecto y lo aplica a las peticiones antes de que lleguen a la aplicación.

---

## 2. ¿Por qué causa error 405 en APIs?

- Las APIs REST usan **PUT**, **PATCH** y **DELETE** para actualizar y eliminar recursos (p. ej. `PUT /api/v1/Documents/9673`).
- En IIS, el **WebDAVModule** intercepta las peticiones en la fase **MapRequestHandler**.
- Si WebDAV no está configurado para permitir ese verbo en esa ruta (o interpreta la URL como recurso estático), responde **405 Method Not Allowed** y la petición **nunca llega** a la aplicación ASP.NET Core.

**Síntoma típico:**

- En local (Kestrel o IIS Express) la API responde bien.
- En IIS (QA/Producción) aparece **HTTP 405.0** con detalle: *Module: WebDAVModule*.

---

## 3. ¿Afecta a otras webs quitar WebDAV?

Depende del **ámbito** donde se deshabilite:

| Ámbito | Efecto |
|--------|--------|
| **Todo el servidor IIS** | Todas las webs dejan de aceptar peticiones WebDAV. Cualquier sitio que use WebDAV para edición remota o publicación dejará de funcionar para ese uso. |
| **Solo un sitio (o aplicación)** | Solo ese sitio deja de usar WebDAV. **El resto de sitios del servidor no se ven afectados.** |

**Recomendación:** deshabilitar WebDAV **solo en el sitio o aplicación** donde corre la API, para no impactar otras webs del mismo servidor.

---

## 4. Cómo deshabilitar WebDAV

### 4.1 Por `web.config` (recomendado para el proyecto)

Incluir en el `web.config` del **sitio/aplicación** que sirve la API (por ejemplo, el que publica la API en `/api/...`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <!-- Quitar el módulo WebDAV para que PUT/PATCH/DELETE lleguen a la API -->
    <modules>
      <remove name="WebDAVModule" />
    </modules>
    <handlers>
      <remove name="WebDAV" />
    </handlers>
    <!-- resto de la configuración (handlers aspNetCore, aspNetCore, etc.) -->
  </system.webServer>
</configuration>
```

Si el `web.config` ya tiene `<system.webServer>`, se añaden dentro de él las secciones `<modules>` y `<handlers>` anteriores (sin duplicar la etiqueta `<system.webServer>`).

Tras desplegar, reciclar el Application Pool o reiniciar el sitio para aplicar los cambios.

---

### 4.2 Por IIS Manager (solo para ese sitio)

1. Abrir **Administrador de IIS**.
2. Seleccionar el **sitio** (o la aplicación virtual) donde está desplegada la API.
3. **Módulos (Modules):**
   - Doble clic en **Modules**.
   - Localizar **WebDAVModule**.
   - Clic en **Remove** (solo para este sitio).
4. **Asignaciones de controlador (Handler Mappings):**
   - Doble clic en **Handler Mappings**.
   - Localizar el handler **WebDAV** (si existe).
   - Clic en **Remove**.
5. Reciclar el Application Pool o ejecutar **IISReset** si se prefiere.

Con esto WebDAV queda deshabilitado solo en ese sitio; el resto del servidor no cambia.

---

### 4.3 A nivel de servidor (afecta a todos los sitios)

Solo tiene sentido si en ese servidor **ningún** sitio usa WebDAV:

1. En IIS Manager, seleccionar el **nombre del servidor** (nodo raíz).
2. Abrir **Modules** y quitar **WebDAVModule**.
3. Abrir **Handler Mappings** y quitar el handler **WebDAV**.

**Advertencia:** esto aplica a todos los sitios del servidor.

---

## 5. Verificación

1. Desplegar o aplicar los cambios y reciclar el sitio/Application Pool.
2. Repetir la operación que fallaba (p. ej. guardar/actualizar un documento desde la WebApp contra la API en QA).
3. Si el 405 desaparece y la API responde (200, 400, 500 según corresponda), WebDAV ya no está bloqueando la petición.

---

## 6. Referencias

- [HTTP Error 405.0 - Method Not Allowed (IIS)](https://learn.microsoft.com/en-us/troubleshoot/iis/http-error-405)
- [WebDAV Module (IIS)](https://learn.microsoft.com/en-us/iis/configuration/system.webserver/webdav/)

---

*Documento generado para el proyecto GeCom Following Preload. Ajustar rutas y nombres de sitio según el entorno (QA, Producción).*
