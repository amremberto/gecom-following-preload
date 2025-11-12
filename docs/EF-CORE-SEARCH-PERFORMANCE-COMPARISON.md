# Comparación de Rendimiento: EF.Functions.Like vs ToUpper().Contains() en Entity Framework Core

## Resumen Ejecutivo

Este documento analiza las diferencias de rendimiento y comportamiento entre dos enfoques comunes para realizar búsquedas case-insensitive en Entity Framework Core con SQL Server: `EF.Functions.Like` y `ToUpper().Contains()`. La comparación se realiza en el contexto de búsquedas de texto en las columnas `RazonSocial` y `Cuit` de la entidad `Provider`.

**Conclusión principal:** `EF.Functions.Like` es significativamente más eficiente que `ToUpper().Contains()` cuando se utiliza con collations case-insensitive, ya que evita la aplicación de funciones a nivel de fila y permite que SQL Server optimice mejor las consultas.

### Tabla Comparativa de Rendimiento

| Aspecto | EF.Functions.Like | ToUpper().Contains() | Ganador |
|---------|-------------------|----------------------|---------|
| **SQL Generado** | `LIKE '%pattern%'` | `UPPER(column) LIKE '%pattern%'` | EF.Functions.Like |
| **Funciones por fila** | 0 (ninguna) | 1 (UPPER) | EF.Functions.Like |
| **Overhead de CPU** | Mínimo | Medio-Alto (~2x) | EF.Functions.Like |
| **Rendimiento** | ⭐⭐⭐⭐⭐ Óptimo | ⭐⭐⭐ Regular | EF.Functions.Like |
| **Uso de índices** | ❌ No (patrón `%text%`) | ❌ No (función + patrón) | Empate |
| **Case-insensitive** | ✅ Collation CI | ✅ Forzado con UPPER | EF.Functions.Like |
| **Advertencias CA1862** | ✅ Sin advertencias | ❌ Genera advertencia | EF.Functions.Like |
| **Compatibilidad EF Core** | ✅ Nativa | ✅ Compatible | Empate |
| **Legibilidad del código** | ⭐⭐⭐⭐⭐ Excelente | ⭐⭐⭐ Buena | EF.Functions.Like |
| **Tiempo estimado (10K filas)** | ~50ms | ~100ms | EF.Functions.Like |

**Resultado:** `EF.Functions.Like` es superior en **8 de 10** aspectos evaluados.

---

## Contexto Técnico

### Configuración del Entorno

- **ORM:** Entity Framework Core 9
- **Base de Datos:** SQL Server
- **Collation:** `Modern_Spanish_CI_AS` (Case-Insensitive, Accent-Sensitive)
- **Entidad:** `Provider` con columnas `RazonSocial` (string) y `Cuit` (string)

### Problema Original

El código inicial intentaba usar `string.Contains()` con `StringComparison.CurrentCultureIgnoreCase`, lo cual genera un error de traducción de LINQ a SQL:

```
The LINQ expression could not be translated. Translation of method 'string.Contains' failed.
```

Este error ocurre porque EF Core no puede traducir directamente `Contains` con `StringComparison` a SQL Server.

---

## Análisis Comparativo

### Opción 1: EF.Functions.Like

#### Implementación

```csharp
public async Task<IEnumerable<Provider>> SearchAsync(string searchText, int maxResults = 20, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(searchText))
    {
        return [];
    }

    string searchPattern = $"%{searchText.Trim()}%";

    return await GetQueryable()
        .Where(p =>
            EF.Functions.Like(p.RazonSocial, searchPattern) ||
            EF.Functions.Like(p.Cuit, searchPattern))
        .OrderBy(p => p.RazonSocial)
        .ThenBy(p => p.Cuit)
        .Take(maxResults)
        .ToListAsync(cancellationToken);
}
```

#### SQL Generado

```sql
SELECT TOP(@__p_0) [p].[ProvId], [p].[Cuit], [p].[RazonSocial], ...
FROM [Providers] AS [p]
WHERE ([p].[RazonSocial] LIKE @__searchPattern_0) 
   OR ([p].[Cuit] LIKE @__searchPattern_1)
ORDER BY [p].[RazonSocial], [p].[Cuit]
```

#### Características

| Aspecto | Descripción |
|---------|-------------|
| **Traducción SQL** | Directa a `LIKE '%pattern%'` |
| **Aplicación de funciones** | Ninguna función aplicada a nivel de fila |
| **Case-insensitive** | Gestionado por la collation `Modern_Spanish_CI_AS` |
| **Uso de índices** | No puede usar índices (patrón comienza con `%`) |
| **Rendimiento** | Óptimo - sin overhead de funciones |
| **Compatibilidad** | Totalmente compatible con EF Core |

#### Ventajas

1. ✅ **Rendimiento superior:** No aplica funciones a cada fila
2. ✅ **Optimización nativa:** SQL Server puede optimizar mejor la comparación
3. ✅ **Limpieza de código:** No requiere conversiones manuales
4. ✅ **Respeto a collation:** Utiliza la collation de la base de datos
5. ✅ **Sin advertencias de análisis:** No genera CA1862

#### Desventajas

1. ⚠️ **No puede usar índices:** El patrón `%text%` impide el uso de índices tradicionales
2. ⚠️ **Limitado a patrones LIKE:** Solo soporta patrones de SQL Server LIKE

---

### Opción 2: ToUpper().Contains()

#### Implementación

```csharp
public async Task<IEnumerable<Provider>> SearchAsync(string searchText, int maxResults = 20, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(searchText))
    {
        return [];
    }

    searchText = searchText.Trim().ToUpperInvariant();

    return await GetQueryable()
        .Where(p =>
            p.RazonSocial.ToUpper().Contains(searchText) ||
            p.Cuit.ToUpper().Contains(searchText))
        .OrderBy(p => p.RazonSocial)
        .ThenBy(p => p.Cuit)
        .Take(maxResults)
        .ToListAsync(cancellationToken);
}
```

#### SQL Generado

```sql
SELECT TOP(@__p_0) [p].[ProvId], [p].[Cuit], [p].[RazonSocial], ...
FROM [Providers] AS [p]
WHERE (UPPER([p].[RazonSocial]) LIKE N'%' + @__searchText_0 + N'%') 
   OR (UPPER([p].[Cuit]) LIKE N'%' + @__searchText_0 + N'%')
ORDER BY [p].[RazonSocial], [p].[Cuit]
```

#### Características

| Aspecto | Descripción |
|---------|-------------|
| **Traducción SQL** | `UPPER(column) LIKE '%pattern%'` |
| **Aplicación de funciones** | `UPPER()` aplicado a cada fila evaluada |
| **Case-insensitive** | Forzado mediante conversión a mayúsculas |
| **Uso de índices** | No puede usar índices (función + patrón con `%`) |
| **Rendimiento** | Inferior - overhead de `UPPER()` por fila |
| **Compatibilidad** | Compatible pero ineficiente |

#### Ventajas

1. ✅ **Control explícito:** Conversión case-insensitive explícita
2. ✅ **Funciona sin collation CI:** Útil si la collation es case-sensitive

#### Desventajas

1. ❌ **Rendimiento inferior:** Aplica `UPPER()` a cada fila antes de comparar
2. ❌ **Doble conversión:** Convierte tanto el texto de búsqueda como las columnas
3. ❌ **No puede usar índices:** La función `UPPER()` impide el uso de índices
4. ❌ **Advertencia CA1862:** Genera advertencia de análisis de código
5. ❌ **Redundante con collation CI:** La collation ya maneja case-insensitive

---

## Comparación de Rendimiento

### Métricas Teóricas

Para una tabla con **10,000 registros** y una búsqueda que devuelve **20 resultados**:

| Métrica | EF.Functions.Like | ToUpper().Contains() | Diferencia |
|---------|-------------------|----------------------|------------|
| **Operaciones por fila** | 1 comparación | 1 conversión + 1 comparación | +100% overhead |
| **CPU por fila** | Bajo | Medio-Alto | ~2x más CPU |
| **Memoria** | Mínima | Ligeramente mayor | Despreciable |
| **Tiempo estimado** | ~50ms | ~100ms | ~2x más lento |

### Análisis de Plan de Ejecución

#### EF.Functions.Like

```
Clustered Index Scan (Cost: 100%)
  - Predicate: [RazonSocial] LIKE '%pattern%' OR [Cuit] LIKE '%pattern%'
  - Rows examined: 10,000
  - Rows returned: 20
```

#### ToUpper().Contains()

```
Clustered Index Scan (Cost: 100%)
  - Predicate: UPPER([RazonSocial]) LIKE '%pattern%' OR UPPER([Cuit]) LIKE '%pattern%'
  - Rows examined: 10,000
  - Rows returned: 20
  - Computed Scalar: UPPER() applied to each row
```

**Observación:** Ambos realizan un scan completo, pero `ToUpper().Contains()` agrega el costo adicional de calcular `UPPER()` para cada fila evaluada.

---

## Impacto en Uso de Índices

### Limitación Común

Ambos métodos **no pueden usar índices tradicionales** cuando el patrón comienza con `%` porque:

1. El patrón `%text%` requiere buscar en cualquier posición de la cadena
2. SQL Server no puede usar índices B-tree para búsquedas que no comienzan con el valor

### Soluciones para Mejorar el Rendimiento

Si el rendimiento se convierte en un problema con tablas grandes, considera:

#### 1. Full-Text Search (Recomendado para búsquedas de texto)

```csharp
// Requiere configuración de Full-Text Index en SQL Server
return await GetQueryable()
    .Where(p =>
        EF.Functions.FreeText(p.RazonSocial, searchText) ||
        EF.Functions.FreeText(p.Cuit, searchText))
    .Take(maxResults)
    .ToListAsync(cancellationToken);
```

#### 2. Índices Computados con UPPER()

```sql
-- Crear índice computado
CREATE INDEX IX_Providers_RazonSocial_Upper 
ON Providers (UPPER(RazonSocial));

-- Luego usar en la consulta
WHERE UPPER(RazonSocial) = UPPER(@searchText)
```

#### 3. Cambiar Estrategia de Búsqueda

Si es posible, buscar desde el inicio en lugar de contener:

```csharp
// Buscar desde el inicio (puede usar índices)
string searchPattern = $"{searchText.Trim()}%";
EF.Functions.Like(p.RazonSocial, searchPattern)
```

---

## Recomendaciones

### Cuándo Usar EF.Functions.Like

✅ **Usa `EF.Functions.Like` cuando:**
- La base de datos tiene collation case-insensitive (`CI`)
- Necesitas el mejor rendimiento posible
- Realizas búsquedas que contienen texto en cualquier posición
- Quieres evitar advertencias de análisis de código

### Cuándo Usar ToUpper().Contains()

⚠️ **Considera `ToUpper().Contains()` solo cuando:**
- La collation de la base de datos es case-sensitive (`CS`)
- Necesitas control explícito sobre la conversión
- El rendimiento no es crítico (tablas pequeñas)

### Mejores Prácticas

1. **Aprovecha la collation:** Si tu base de datos usa collation CI, no necesitas conversiones manuales
2. **Usa EF.Functions.Like:** Es la forma más eficiente y nativa de EF Core
3. **Monitorea el rendimiento:** Para tablas grandes (>100K registros), considera Full-Text Search
4. **Evita conversiones redundantes:** No conviertas a mayúsculas si la collation ya es CI

---

## Ejemplo de Implementación Final

La implementación recomendada para el repositorio `ProviderRepository`:

```csharp
public async Task<IEnumerable<Provider>> SearchAsync(
    string searchText, 
    int maxResults = 20, 
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(searchText))
    {
        return [];
    }

    // No necesitamos convertir a mayúsculas porque la collation es CI
    string searchPattern = $"%{searchText.Trim()}%";

    return await GetQueryable()
        .Where(p =>
            EF.Functions.Like(p.RazonSocial, searchPattern) ||
            EF.Functions.Like(p.Cuit, searchPattern))
        .OrderBy(p => p.RazonSocial)
        .ThenBy(p => p.Cuit)
        .Take(maxResults)
        .ToListAsync(cancellationToken);
}
```

**Ventajas de esta implementación:**
- ✅ Rendimiento óptimo
- ✅ Código limpio y legible
- ✅ Sin advertencias de análisis
- ✅ Aprovecha la collation de la base de datos
- ✅ Traducción directa a SQL eficiente

---

## Conclusiones

1. **`EF.Functions.Like` es superior** en términos de rendimiento cuando se usa con collations case-insensitive
2. **Evita conversiones redundantes** - la collation `Modern_Spanish_CI_AS` ya maneja case-insensitive
3. **Ambos métodos tienen limitaciones** con índices cuando el patrón comienza con `%`
4. **Para tablas grandes**, considera implementar Full-Text Search o índices computados
5. **La implementación actual es óptima** para el caso de uso de búsqueda de proveedores

---

## Referencias

- [EF Core - Funciones de base de datos](https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping)
- [SQL Server - Collations](https://learn.microsoft.com/en-us/sql/relational-databases/collations/collation-and-unicode-support)
- [EF Core - Traducción de funciones de cadena](https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping)
- [SQL Server - LIKE Operator](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql)

---

**Autor:** Remberto Aguilar  
**Fecha:** 2024  
**Versión:** 1.0

