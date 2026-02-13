# Cambios realizados: Grillas Pagados y Pendientes (DataTables)

**Fecha:** 2026-02-13  
**Objetivo:** Corregir la inicialización de DataTables en las páginas **Documentos Pagados** y **Documentos Pendientes** para que las grillas muestren correctamente encabezados, paginación, buscador y ordenamiento (comportamiento alineado con la página **Documentos Procesados**).

---

## Problema

En las páginas **Pagados** (`/documents/paid`) y **Pendientes** (`/documents/pending`), la grilla se veía como una tabla HTML simple: sin paginación, sin barra de búsqueda y con encabezados que no se comportaban como en **Procesados**. En la consola del navegador aparecían advertencias:

- `Table element not found: documents-datatable`
- `Table element not found: pending-documents-datatable`

### Causa raíz

En **Pagados** y **Pendientes** la tabla está dentro de `@if (!_isDataTableLoading)`. Mientras se cargan los datos, `_isDataTableLoading` es `true`, por lo que la tabla **no se renderiza**. El código llamaba a `loadDataTable(...)` **antes** de poner `_isDataTableLoading = false`, es decir, cuando el elemento aún no existía en el DOM. El script `table-datatable.js` no encuentra el elemento y sale con un `console.warn`, por lo que DataTables nunca se inicializa.

En **Procesados** la tabla está siempre en el DOM (fuera del condicional del spinner), por eso `loadDataTable` sí encuentra el elemento y la grilla funciona bien.

---

## Solución aplicada

Se unificó el patrón de inicialización de DataTables en todos los puntos donde se carga o recarga la grilla:

1. Cargar los datos (p. ej. `GetPaidDocuments()` / `GetPendingDocuments()`).
2. Poner `_isDataTableLoading = false` y llamar a `StateHasChanged()` para que Blazor renderice la tabla.
3. Esperar un breve intervalo (`await Task.Delay(50)`) para que el DOM se actualice.
4. Llamar a `destroyDataTable` (por si ya existía una instancia) y luego a `loadDataTable` con el id de la tabla.

Así, `loadDataTable` se ejecuta siempre cuando el elemento de la tabla ya está presente en el DOM.

---

## Archivos modificados

### 1. `src/GeCom.Following.Preload.WebApp/Components/Pages/Documents/Paid.razor.cs`

| Ubicación | Cambio |
|-----------|--------|
| **OnAfterRenderAsync** (carga inicial con rol válido) | Se deja de llamar a `destroyDataTable`/`loadDataTable` antes de ocultar el spinner. Tras `GetPaidDocuments()` se asigna `_isDataTableLoading = false`, `StateHasChanged()`, `Task.Delay(50)`, y luego `destroyDataTable` + `loadDataTable`. |
| **OnAfterRenderAsync** (rama sin rol válido) | Se asegura que la tabla esté renderizada (`_isDataTableLoading = false`, `StateHasChanged()`, `Task.Delay(50)`) antes de `loadDataTable`. |
| **SearchPaidDocuments** | Mismo patrón: cargar datos, poner `_isDataTableLoading = false`, `StateHasChanged()`, `Task.Delay(50)`, y después `destroyDataTable` + `loadDataTable`. |
| **OnPaymentConfirmed** | Tras confirmar el pago y llamar a `GetPaidDocuments()`, se aplica el mismo flujo (bajar bandera, re-render, delay, destroy + loadDataTable) antes de mostrar el toast de éxito. |

### 2. `src/GeCom.Following.Preload.WebApp/Components/Pages/Documents/Pending.razor.cs`

| Ubicación | Cambio |
|-----------|--------|
| **OnAfterRenderAsync** (usuario con rol soportado) | Tras `GetPendingDocuments()` se pone `_isDataTableLoading = false`, `StateHasChanged()`, `Task.Delay(50)`, y luego `destroyDataTable("pending-documents-datatable")` + `loadDataTable("pending-documents-datatable")`. |
| **OnAfterRenderAsync** (usuario sin permisos) | Se fuerza render de la tabla (`_isDataTableLoading = false`, `StateHasChanged()`, `Task.Delay(50)`) antes de `loadDataTable` y del toast de aviso. |
| **CheckAndOpenEditModalFromQueryString** | Al recargar la grilla por `editDocId` en la URL: cargar datos, bajar bandera, re-render, delay, destroy + loadDataTable; se mantiene el `Task.Delay(300)` para que DataTable esté listo antes de abrir el modal. |
| **RefreshPendingDocumentsDataTable** | Mismo patrón: cargar datos, `_isDataTableLoading = false`, `StateHasChanged()`, delay, destroy + loadDataTable. |
| **Eliminación de documento** (método que llama a `DocumentService.DeleteAsync`) | Tras `GetPendingDocuments()` se aplica el mismo flujo (bajar bandera, re-render, delay, destroy + loadDataTable) antes del toast de éxito. |
| **OnDocumentPreloaded** | Tras precargar un documento y recargar la grilla con `GetPendingDocuments()`, se aplica el mismo patrón antes de abrir el modal de edición. |

---

## Verificación

- Probar **Pagados** (`/documents/paid`): carga inicial, filtro por fechas (Buscar), confirmar pago. La grilla debe mostrar encabezados, paginación, buscador y orden por columnas.
- Probar **Pendientes** (`/documents/pending`): carga inicial, eliminar documento, precargar y abrir edición, refrescar grilla. Misma comprobación.
- En la consola del navegador (F12 → Console) no deben aparecer los avisos `Table element not found: documents-datatable` ni `Table element not found: pending-documents-datatable`.

---

## Corrección: duplicación de grillas al confirmar pago

Tras los cambios anteriores se detectó que, al confirmar un pago (y en otros flujos que ocultan la tabla con el spinner), aparecían **varias grillas** en pantalla (una vacía “0 a 0 de 0 registros” y otra con datos).

### Causa

DataTables modifica el DOM (envuelve la tabla en un `div` propio). Si se pone `_isDataTableLoading = true` **sin** destruir antes la instancia de DataTable, Blazor oculta el bloque de la tabla pero el DOM ya no coincide con lo que Blazor cree (el wrapper de DataTables sigue ahí o la estructura queda desincronizada). Al volver a mostrar la tabla, Blazor añade una nueva y la anterior (o su wrapper) puede quedar visible, generando duplicación.

### Solución

En **todos** los flujos donde se va a ocultar la tabla (mostrar “Cargando datos…”), se llama a **`destroyDataTable` antes** de asignar `_isDataTableLoading = true`. Así el DOM vuelve al estado que Blazor espera y, al ocultar el bloque, se elimina correctamente; al mostrarlo de nuevo solo hay una grilla.

**Archivos y puntos actualizados:**

- **Paid.razor.cs:** `OnPaymentConfirmed` y `SearchPaidDocuments`: se llama a `destroyDataTable("documents-datatable")` al inicio, antes de `_isDataTableLoading = true`.
- **Pending.razor.cs:** `OnAfterRenderAsync` (carga inicial con rol), `CheckAndOpenEditModalFromQueryString`, `RefreshPendingDocumentsDataTable`, eliminación de documento y `OnDocumentPreloaded`: se llama a `destroyDataTable("pending-documents-datatable")` antes de `_isDataTableLoading = true`.

---

## Notas técnicas

- El delay de 50 ms (`Task.Delay(50)`) da tiempo a que Blazor actualice el DOM tras `StateHasChanged()`. Es un patrón habitual en Blazor cuando se depende de que un elemento exista antes de llamar a JavaScript.
- Destruir DataTable **antes** de ocultar la tabla evita que el DOM mutado por DataTables provoque grillas duplicadas al volver a mostrar el contenido.
- No se modificaron las vistas `.razor` ni el script `table-datatable.js`; solo la lógica de cuándo se llama a `destroyDataTable` y `loadDataTable` en los code-behind de **Paid** y **Pending**.
