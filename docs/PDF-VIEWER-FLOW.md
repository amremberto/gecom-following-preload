# Flujo de Obtención y Visualización de PDF

## Proceso Completo

### 1. Almacenamiento del PDF
- El PDF se guarda en el **sistema de archivos de Windows** (ej: `C:\Storage\2024\01\documento-123-abc.pdf`)
- La ruta completa se guarda en la base de datos en la tabla `Attachments` (campo `Path`)
- El documento tiene una relación con `Attachment` a través de `DocId`

### 2. Obtención del PDF desde la API

**Endpoint:** `GET /api/v1/Attachments/{adjuntoId}/download`

**Proceso en el Backend:**
```
1. Cliente Blazor → API: GET /api/v1/Attachments/123/download
2. API obtiene el Attachment de la BD (incluye el Path)
3. StorageService.ReadFileAsync(path) lee el archivo del sistema de archivos Windows
4. API devuelve: File(byte[], "application/pdf", fileName)
   - Content-Type: application/pdf
   - Body: bytes del PDF (binario)
```

**Código en AttachmentsController:**
```csharp
byte[] fileContent = await _storageService.ReadFileAsync(attachment.Path, cancellationToken);
return File(fileContent, "application/pdf", fileName);
```

### 3. Descarga en el Cliente Blazor

**Proceso:**
```
1. DocumentService.DownloadAttachmentAsync(adjuntoId)
   ↓
2. HttpClientService.DownloadFileAsync(uri)
   ↓
3. HttpClient.GetAsync() → Recibe HttpResponseMessage
   ↓
4. response.Content.ReadAsByteArrayAsync()
   ↓
5. Retorna: byte[] (array de bytes del PDF)
```

**Código:**
```csharp
// En HttpClientService
byte[] fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
return fileContent;
```

### 4. Conversión para el Visor PDF.js

**Problema:** PDF.js necesita un `Uint8Array` de JavaScript, pero Blazor Server no puede pasar arrays grandes directamente.

**Solución Actual:**
```
1. C#: byte[] → Convert.ToBase64String() → string base64
2. JavaScript Interop: Pasa el string base64 a JavaScript
3. JavaScript: 
   - atob(base64) → string binario
   - new Uint8Array() → Convierte a array de bytes
4. PDF.js: getDocument({ data: Uint8Array })
```

**Código en PdfViewer.razor.cs:**
```csharp
// Descargar bytes
byte[]? pdfBytes = await DocumentService.DownloadAttachmentAsync(AdjuntoId);

// Convertir a Base64 para pasar a JavaScript
string base64Pdf = Convert.ToBase64String(pdfBytes);

// Pasar a JavaScript
await _pdfViewerModule.InvokeAsync<JsonObject>(
    "pdfViewer.loadPdfFromBase64", base64Pdf);
```

**Código en pdf-viewer.js:**
```javascript
async loadPdfFromBase64(base64String) {
    // Convertir Base64 a Uint8Array
    const binaryString = atob(base64String);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    
    // Cargar en PDF.js
    const loadingTask = this.pdfjsLib.getDocument({ data: bytes });
    this.currentDocument = await loadingTask.promise;
}
```

## Resumen del Flujo

```
Windows File System (PDF almacenado)
    ↓
StorageService.ReadFileAsync() → byte[]
    ↓
API Controller → File(byte[], "application/pdf")
    ↓
HTTP Response (Content-Type: application/pdf, Body: bytes binarios)
    ↓
HttpClientService.DownloadFileAsync() → byte[]
    ↓
PdfViewer: Convert.ToBase64String(byte[]) → string base64
    ↓
JavaScript Interop: string base64
    ↓
JavaScript: atob() + Uint8Array → bytes
    ↓
PDF.js: getDocument({ data: Uint8Array })
    ↓
Render en Canvas HTML5
```

## Alternativas Mejores (para considerar)

### Opción 1: URL Directa (más eficiente)
- En lugar de descargar bytes y convertir a Base64
- Usar una URL directa al endpoint: `/api/v1/Attachments/{id}/download`
- PDF.js puede cargar directamente desde URL
- **Ventaja:** No necesita pasar datos grandes por JavaScript Interop
- **Desventaja:** Requiere que el endpoint sea accesible desde el navegador con autenticación

### Opción 2: Blob URL
- Descargar bytes en C#
- Crear un Blob en JavaScript
- Generar URL temporal con `URL.createObjectURL(blob)`
- Pasar la URL a PDF.js
- **Ventaja:** Más eficiente que Base64
- **Desventaja:** Aún requiere pasar los bytes

### Opción 3: Iframe (más simple)
- Crear un iframe con `src="/api/v1/Attachments/{id}/download"`
- El navegador maneja la descarga/visualización automáticamente
- **Ventaja:** Muy simple, no requiere JavaScript complejo
- **Desventaja:** Menos control sobre la visualización



















