# Serilog + CorrelationId Implementation

Este directorio contiene la configuración de Serilog con CorrelationId usando paquetes oficiales para la solución GeCom.Following.Preload.

## Paquetes Utilizados

- **Serilog.AspNetCore**: Integración oficial de Serilog con ASP.NET Core
- **Serilog.Enrichers.CorrelationId**: Enricher oficial para añadir Correlation ID a los logs
- **CorrelationId**: Middleware oficial para generar y propagar Correlation ID

## ¿Por qué Paquetes Oficiales?

### ✅ **Ventajas de los Paquetes Oficiales**

1. **Mantenimiento**: Mantenidos por la comunidad y actualizados regularmente
2. **Estabilidad**: Probados en producción por miles de desarrolladores
3. **Funcionalidades Avanzadas**: Más características que implementación manual
4. **Documentación**: Documentación oficial y ejemplos
5. **Compatibilidad**: Garantizada compatibilidad con versiones de .NET
6. **Menos Código**: Menos código personalizado que mantener

### ❌ **Desventajas de Implementación Manual**

1. **Mantenimiento**: Código personalizado que mantener
2. **Bugs**: Posibles bugs que no existen en paquetes oficiales
3. **Funcionalidades**: Limitado a lo que implementes
4. **Tiempo**: Más tiempo de desarrollo y testing

## Configuración

### 1. **Paquetes NuGet**

```xml
<PackageReference Include="Serilog.AspNetCore" />
<PackageReference Include="Serilog.Enrichers.CorrelationId" />
<PackageReference Include="CorrelationId" />
```

### 2. **Registro en DI**

```csharp
// En Program.cs
builder.Services.AddCorrelationId();
```

### 3. **Middleware en Pipeline**

```csharp
// En Program.cs - ORDEN IMPORTANTE
app.UseCorrelationId();           // 1. Genera Correlation ID
app.UseSerilogRequestLogging();   // 2. Logs requests con Correlation ID
```

### 4. **Configuración de Serilog**

```json
{
  "Serilog": {
    "Enrich": [
      "FromLogContext",
      "WithCorrelationId",  // Enricher oficial
      "WithMachineName",
      "WithThreadId",
      "WithProcessId",
      "WithExceptionDetails"
    ]
  }
}
```

## Funcionalidades Incluidas

### 🔧 **CorrelationId Middleware**

- **Generación Automática**: Crea Correlation ID único para cada request
- **Header Support**: Respeta `X-Correlation-ID` header si se proporciona
- **Response Headers**: Añade Correlation ID a la respuesta
- **Context Propagation**: Propaga ID a través de toda la request
- **Async Support**: Funciona correctamente con operaciones asíncronas

### 📝 **Serilog.Enrichers.CorrelationId**

- **Enrichment Automático**: Añade Correlation ID a todos los logs
- **Zero Configuration**: Funciona automáticamente una vez configurado
- **Performance**: Optimizado para alto rendimiento
- **Thread Safety**: Seguro para uso en múltiples hilos

## Headers HTTP

### Request Headers
```
X-Correlation-ID: a1b2c3d4 (opcional)
```

### Response Headers
```
X-Correlation-ID: a1b2c3d4 (siempre incluido)
```

## Logs de Ejemplo

```json
{
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "Level": "Information",
  "Message": "Request processed successfully",
  "CorrelationId": "a1b2c3d4",
  "ApplicationName": "GeCom.Following.Preload.WebAPI",
  "MachineName": "SERVER-01",
  "ThreadId": 123,
  "ProcessId": 4567,
  "RequestId": "0HMQ8VQKJQJQJ",
  "RequestPath": "/api/test/correlation",
  "RequestMethod": "GET"
}
```

## Testing

### Endpoints de Prueba

1. **GET /api/test/correlation**
   - Prueba básica de Correlation ID
   - Verifica que se incluye en logs

2. **GET /api/test/correlation-multiple**
   - Prueba con múltiples logs
   - Verifica consistencia del ID

### Ejemplo de Prueba

```bash
# Request sin header
curl http://localhost:5000/api/test/correlation

# Request con header personalizado
curl -H "X-Correlation-ID: test-123" http://localhost:5000/api/test/correlation
```

## Configuración Avanzada

### Personalizar Header Name

```csharp
builder.Services.AddCorrelationId(options =>
{
    options.Header = "X-Custom-Correlation-ID";
});
```

### Personalizar Formato del ID

```csharp
builder.Services.AddCorrelationId(options =>
{
    options.GenerateId = () => Guid.NewGuid().ToString("N")[..8];
});
```

### Configurar Serilog con Opciones

```csharp
builder.Services.AddCorrelationId(options =>
{
    options.IncludeInResponse = true;
    options.UpdateTraceIdentifier = true;
});
```

## Comparación: Manual vs Oficial

| Aspecto | Implementación Manual | Paquetes Oficiales |
|---------|----------------------|-------------------|
| **Código** | ~200 líneas | ~5 líneas |
| **Mantenimiento** | Alto | Bajo |
| **Testing** | Requerido | Incluido |
| **Documentación** | Personal | Oficial |
| **Actualizaciones** | Manual | Automática |
| **Bugs** | Posibles | Probados |
| **Funcionalidades** | Básicas | Avanzadas |

## Conclusión

Los paquetes oficiales proporcionan una solución más robusta, mantenible y completa que una implementación manual. La configuración es mínima y las funcionalidades son superiores.

**Recomendación**: Siempre usar paquetes oficiales cuando estén disponibles, especialmente para funcionalidades críticas como logging y observabilidad.