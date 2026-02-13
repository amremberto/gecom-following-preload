# Plan: Servicio IMonitorService con Dapper y BD Monitores

Servicio que consulta la base de datos **Monitores** mediante **Dapper** y SQL directo, expuesto en la capa Application como `IMonitorService`. Actualmente se usa para obtener el número de documento SAP asociado a un documento en Monitores (p. ej. Orden de Pago para el Recibo de Pago).

## Estado

**Implementado.** La abstracción está en Application, la implementación en Infrastructure usa Dapper y `Microsoft.Data.SqlClient`, y la conexión se configura con la cadena `MonitoresConnection`.

## Arquitectura

- **Application:** contrato [IMonitorService](src/GeCom.Following.Preload.Application/Abstractions/Monitor/IMonitorService.cs) y DTO [GetSapDocumentNumberRequest](src/GeCom.Following.Preload.Application/Abstractions/Monitor/GetSapDocumentNumberRequest.cs).
- **Infrastructure:** [MonitorService](src/GeCom.Following.Preload.Infrastructure/Monitor/MonitorService.cs) (Dapper + SqlConnection).
- **Registro DI:** en [InfrastructureDependencyInjection](src/GeCom.Following.Preload.Infrastructure/InfrastructureDependencyInjection.cs): `services.AddScoped<IMonitorService, MonitorService>()`.

## Base de datos Monitores

- **Conexión:** clave `MonitoresConnection` en `IConfiguration` (ConnectionStrings). Obligatoria; si falta o está vacía, el constructor de `MonitorService` lanza `InvalidOperationException`.
- **Configuración por entorno:** en `Configurations/jsons/database.json`, `database.Development.json`, `database.Staging.json`, `database.Production.json` (cada uno con su `MonitoresConnection`).
- **Tabla usada:** `[Monitores].[dbo].[Documents]`. Se consulta la columna `SapDocumentNumber` filtrando por número de documento, proveedor, cliente, punto de venta y letra.

## IMonitorService

### GetSapDocumentNumberAsync

- **Parámetro:** [GetSapDocumentNumberRequest](src/GeCom.Following.Preload.Application/Abstractions/Monitor/GetSapDocumentNumberRequest.cs): `DocumentNumber`, `ProviderNumber`, `ClientNumber`, `SalePoint`, `Letter`.
- **Retorno:** `Task<int?>` — valor de `SapDocumentNumber` si existe una fila que coincida; `null` si no hay resultado.
- **Uso actual:** en [ConfirmPaymentCommandHandler](src/GeCom.Following.Preload.Application/Features/Preload/Documents/ConfirmPayment/ConfirmPaymentCommandHandler.cs) para rellenar "Orden de Pago" en el Recibo de Pago. Mapeo desde el documento preload: `NumeroComprobante` → DocumentNumber, `ProveedorCuit` → ProviderNumber, `SociedadCuit` → ClientNumber, `PuntoDeVenta` → SalePoint, `DocumentType.Letra` → Letter.

### GetDocumentIdsBySapDocumentNumberAsync

- **Parámetros:** `sapDocumentNumber` (int), `cancellationToken` (opcional).
- **Retorno:** `Task<IReadOnlyList<int>>` — lista de IDs de documento en `[Monitores].[dbo].[Documents]` cuyo `SapDocumentNumber` coincide; lista vacía si no hay coincidencias.
- **Consulta SQL (referencia):** `SELECT Id FROM [Monitores].[dbo].[Documents] WHERE SapDocumentNumber = @SapDocumentNumber`. Si en la BD real la columna del identificador tiene otro nombre (p. ej. `DocumentId`), debe sustituirse en el `SELECT`.

## Implementación (MonitorService)

- **Tecnología:** Dapper para ejecutar una consulta SQL parametrizada; `Microsoft.Data.SqlClient.SqlConnection` para conectarse a SQL Server.
- **Conexión:** se obtiene con `configuration.GetConnectionString("MonitoresConnection")`. Se abre con `OpenAsync` y se usa en un `await using`.
- **Consulta actual (resumen):** `SELECT SapDocumentNumber AS Value FROM [Monitores].[dbo].[Documents] WHERE ...` filtrando por los cinco campos del request (DocumentNumber con `LIKE`, resto por igualdad).
- **Ejecución:** `connection.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(sql, new { ... }, cancellationToken: cancellationToken))` para un único valor nullable.
- **Log:** se registra en nivel Information la llamada con los parámetros (DocumentNumber, ProviderNumber, ClientNumber, SalePoint, Letter).
- **GetDocumentIdsBySapDocumentNumberAsync:** consulta `SELECT Id FROM [Monitores].[dbo].[Documents] WHERE SapDocumentNumber = @SapDocumentNumber`; ejecución con `QueryAsync<int>`; resultado convertido a `IReadOnlyList<int>`. Log en Information con `SapDocumentNumber` y cantidad de IDs devueltos.

## Dependencias de paquetes

- **Dapper:** referenciado en [GeCom.Following.Preload.Infrastructure.csproj](src/GeCom.Following.Preload.Infrastructure/GeCom.Following.Preload.Infrastructure.csproj) (versión centralizada en Directory.Packages.props).
- **Microsoft.Data.SqlClient:** implícito vía Dapper / conexión SQL Server; debe estar disponible para `SqlConnection`.

## Posibles extensiones

- Añadir más métodos a `IMonitorService` para otras consultas sobre Monitores (misma conexión y mismo patrón Dapper).
- Reutilizar la misma cadena `MonitoresConnection` si en el futuro se necesita otro servicio que consulte Monitores.

## Referencias

- [IMonitorService](src/GeCom.Following.Preload.Application/Abstractions/Monitor/IMonitorService.cs)
- [GetSapDocumentNumberRequest](src/GeCom.Following.Preload.Application/Abstractions/Monitor/GetSapDocumentNumberRequest.cs)
- [MonitorService](src/GeCom.Following.Preload.Infrastructure/Monitor/MonitorService.cs)
- [InfrastructureDependencyInjection](src/GeCom.Following.Preload.Infrastructure/InfrastructureDependencyInjection.cs)
- [ConfirmPayment-PDF-PaymentDetail-Plan](ConfirmPayment-PDF-PaymentDetail-Plan.md) — uso de GetSapDocumentNumberAsync en confirmación de pago
