# Plan: Nombre del fichero PDF del Recibo de Pago

Especificación del nombre con el que se guarda el PDF del Recibo de Pago en storage y se persiste en base de datos.

## Estado

Implementado en el flujo de Confirmación de pago. El nombre se genera en el handler y se usa tanto para guardar el archivo como para el campo `NamePdf` del detalle de pago.

## Patrón del nombre

**Formato actual:**

```
Recibo_{DocId}_{yyyyMMdd}.pdf
```

- **Recibo_** — Prefijo fijo que identifica el tipo de documento.
- **DocId** — Identificador del documento (clave del documento preload).
- **_** — Separador.
- **yyyyMMdd** — Fecha del día en que se confirma el pago (`DateTime.Today`), formato numérico.
- **.pdf** — Extensión.

**Ejemplo:** `Recibo_12345_20250213.pdf`

## Dónde se define y usa

| Paso | Ubicación | Uso |
|------|-----------|-----|
| Generación del nombre | [ConfirmPaymentCommandHandler](src/GeCom.Following.Preload.Application/Features/Preload/Documents/ConfirmPayment/ConfirmPaymentCommandHandler.cs) | `string uniqueFileName = $"Recibo_{document.DocId}_{DateTime.Today:yyyyMMdd}.pdf";` |
| Guardado en disco | `IStorageService.SavePaymentDetailFileAsync(pdfBytes, uniqueFileName)` | El archivo se guarda en `{PaymentDetailPath}\{año}\{mes}\{uniqueFileName}` (solo el nombre del fichero, sin ruta). |
| Persistencia en BD | Entidad `PaymentDetail.NamePdf` | Se asigna el mismo `uniqueFileName` para poder localizar el PDF luego. |

## Ruta completa en storage

El nombre del **fichero** es solo el string anterior. La **ruta completa** la construye el storage:

- Base: `Storage:PaymentDetailPath` (configuración).
- Subcarpetas: `{año}\{mes}` (ej. `2025\02`).
- Fichero: `Recibo_{DocId}_{yyyyMMdd}.pdf`.

Ejemplo de ruta completa: `C:\Storage\PaymentDetail\2025\02\Recibo_12345_20250213.pdf`.

## Restricciones

- **Longitud máxima de `NamePdf` en BD:** 50 caracteres ([PaymentDetailConfigurations](src/GeCom.Following.Preload.Infrastructure/Persistence/Configurations/Preload/PaymentDetailConfigurations.cs), [CreatePaymentDetailCommandValidator](src/GeCom.Following.Preload.Application/Features/Preload/PaymentDetails/CreatePaymentDetail/CreatePaymentDetailCommandValidator.cs)).
- Si `DocId` pudiera ser muy largo (p. ej. GUID como string), el nombre total podría superar 50 caracteres; en ese caso habría que acortar el patrón o ampliar el tamaño del campo.

## Unicidad

- Por documento y fecha: un mismo documento solo puede tener un pago confirmado (validado en el handler), por lo que `Recibo_{DocId}_{yyyyMMdd}.pdf` es único en la práctica para ese documento.
- Varios documentos confirmados el mismo día comparten `yyyyMMdd` pero distinto `DocId`, por lo que los nombres no colisionan.

## Referencias

- [ConfirmPaymentCommandHandler](src/GeCom.Following.Preload.Application/Features/Preload/Documents/ConfirmPayment/ConfirmPaymentCommandHandler.cs) — generación de `uniqueFileName` y asignación a `PaymentDetail.NamePdf`
- [IStorageService.SavePaymentDetailFileAsync](src/GeCom.Following.Preload.Application/Abstractions/Storage/IStorageService.cs)
- [StorageService.SavePaymentDetailFileAsync](src/GeCom.Following.Preload.Infrastructure/Storage/StorageService.cs) — construcción de ruta año/mes y escritura del archivo
- [PaymentDetail.NamePdf](src/GeCom.Following.Preload.Domain/Preloads/PaymentDetails/PaymentDetail.cs)
- [Recibo-De-Pago-PDF-Plan](Recibo-De-Pago-PDF-Plan.md) — plan del diseño del recibo
