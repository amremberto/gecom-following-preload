# Plan: Recibo de Pago PDF (diseño profesional)

Especificación y plan del diseño del PDF de Recibo de Pago generado al confirmar el pago de un documento. Incluye el diseño actual y mejoras propuestas para un aspecto más profesional.

## Estado

El recibo de pago está **implementado** y se genera en el flujo de Confirmación de pago. La plantilla actual es funcional y legible; este documento sirve como especificación y hoja de ruta para evolución del diseño.

- **Implementación:** [QuestPdfDocumentService.BuildReciboDePagoDocument](src/GeCom.Following.Preload.Infrastructure/Pdf/QuestPdfDocumentService.cs)
- **Datos:** [ReciboDePagoData](src/GeCom.Following.Preload.Application/Abstractions/Pdf/ReciboDePagoData.cs)
- **Uso:** [ConfirmPaymentCommandHandler](src/GeCom.Following.Preload.Application/Features/Preload/Documents/ConfirmPayment/ConfirmPaymentCommandHandler.cs) → generación → guardado en storage como `Recibo_{DocId}_{yyyyMMdd}.pdf`

## Diseño actual

### Formato y márgenes

- **Tamaño:** A4
- **Márgenes:** 2 cm en todos los lados
- **Fuente base:** 11 pt (QuestPDF por defecto)

### Estructura del contenido (de arriba a abajo)

1. **Fila superior (dos columnas)**
   - **Izquierda — Bloque Proveedor**
     - Título "Proveedor" (negrita, 12 pt, subrayado)
     - Línea 1: CUIT o "Nro RazonSocial" según exista `ProveedorNro`
     - Línea 2: RazonSocial o CUIT (complemento)
     - Opcional: dirección, teléfono
   - **Derecha — Bloque recibo**
     - "RECIBO" (negrita, 18 pt)
     - Nro: valor o "—"
     - Fecha: dd-MM-yyyy de `FechaEmision`
     - Opcional: Orden de Pago (con padding superior)

2. **Bloque azul claro** (`Colors.Blue.Lighten3`)
   - Texto: "He recibido conforme el pago de: **{Cliente}**"

3. **Bloque ámbar claro** (`Colors.Amber.Lighten3`)
   - Texto: "Por concepto de: **{Concepto}**"

4. **Bloque gris claro** (`Colors.Grey.Lighten3`)
   - Texto: "Importe: **{ImporteRecibido N2}** {Moneda}"

5. **Sección Detalle de pago**
   - Título "Detalle de pago" (negrita, 12 pt)
   - Opciones con marca "X": Transferencia / Cheque o echeq
   - Si no es transferencia: Nro de cheque o echeq, Banco, Vencimiento (dd-MM-yyyy)

6. **Pie**
   - "Fecha de alta: {FechaAlta dd-MM-yyyy}" alineado a la derecha, 10 pt, gris medio

### Elementos no presentes en el diseño actual

- Sin header/footer de página (número de página, logo)
- Sin logo ni marca de la empresa
- Sin bordes o líneas decorativas
- Sin tabla formal para importes o detalle
- Una sola página; no hay paginación explícita

## Modelo de datos (ReciboDePagoData)

| Campo | Uso en el PDF |
|-------|-------------------------------|
| NumeroRecibo | Nro del recibo (ej. 00046) |
| FechaEmision | Fecha del recibo |
| ProveedorCuit, ProveedorRazonSocial, ProveedorNro | Bloque proveedor |
| ProveedorDireccion, ProveedorTelefono | Opcionales en proveedor |
| OrdenDePago | Opcional, derecha arriba |
| Cliente | "He recibido conforme el pago de: **Cliente**" |
| Concepto | "Por concepto de: **Concepto**" |
| ImporteRecibido, Moneda | Bloque importe |
| FechaAlta | Pie "Fecha de alta" |
| EsTransferencia | Marca Transferencia vs Cheque/echeq |
| NroCheque, Banco, Vencimiento | Solo si no es transferencia |

## Mejoras propuestas (diseño profesional)

- **Header fijo:** logo (imagen o texto de marca), nombre del sistema o empresa, línea separadora.
- **Footer:** número de página, texto legal o pie de página (ej. "Documento generado electrónicamente").
- **Identidad visual:** paleta de colores configurable (o alineada con marca), tipografía más definida (títulos vs cuerpo).
- **Recuadros y bordes:** bordes sutiles en bloques (proveedor, recibo, importe) en lugar de solo fondo de color.
- **Tabla de detalle:** importe y concepto en formato tabla con etiquetas alineadas para lectura más clara.
- **Espaciado y jerarquía:** márgenes internos consistentes, tamaños de fuente en escala (ej. 14 / 12 / 10).
- **Opcional:** firma/espacio para firma, código QR o código de barras con número de recibo para trazabilidad.

## Referencias

- [QuestPdfDocumentService](src/GeCom.Following.Preload.Infrastructure/Pdf/QuestPdfDocumentService.cs) — método `BuildReciboDePagoDocument`
- [ReciboDePagoData](src/GeCom.Following.Preload.Application/Abstractions/Pdf/ReciboDePagoData.cs)
- [QuestPDF-Integration-Plan](QuestPDF-Integration-Plan.md) — integración general de QuestPDF en la API
