# Problema con OPENJSON en SQL Server y Solución Implementada

## Resumen

Este documento describe el error encontrado al usar `Contains()` con listas en Entity Framework Core cuando se ejecuta contra SQL Server, y la solución implementada para evitarlo.

## Error Encontrado

### Síntoma

Al ejecutar consultas que utilizan `Contains()` con una lista de valores, se produce el siguiente error:

```
Microsoft.Data.SqlClient.SqlException
  HResult=0x80131904
  Message=Incorrect syntax near '$'.
  Source=Core Microsoft Sql Client Data Provider
```

### Ubicación del Error

El error ocurre en el `GetProviderSocietiesByProviderCuitQueryHandler` cuando se intenta filtrar cuentas SAP por una lista de `AccountNumber`:

```csharp
IEnumerable<SapAccount> societyAccounts = await _sapAccountRepository.FindAsync(
    a => sociedadFiList.Contains(a.Accountnumber)
        && a.Customertypecode == 3
        && a.NewCuit != null
        && !string.IsNullOrWhiteSpace(a.NewCuit),
    cancellationToken);
```

## Causa del Error

### ¿Por qué ocurre?

Entity Framework Core traduce la operación `Contains()` con una lista en memoria a una consulta SQL que utiliza `OPENJSON` cuando la versión de compatibilidad de SQL Server es 130 o superior (SQL Server 2016+).

Sin embargo, el problema surge cuando:

1. **Nivel de compatibilidad de la base de datos**: Si la base de datos tiene un nivel de compatibilidad inferior a 130, `OPENJSON` no está disponible.
2. **Configuración de SQL Server**: Algunas versiones o configuraciones de SQL Server pueden tener `OPENJSON` deshabilitado o no disponible.
3. **Sintaxis generada**: EF Core genera una sintaxis SQL que puede no ser compatible con todas las versiones/configuraciones de SQL Server.

### SQL Generado por EF Core (Problemático)

Cuando EF Core encuentra `Contains()` con una lista, genera algo similar a:

```sql
SELECT * FROM cuenta
WHERE customertypecode = 3
  AND new_cuit IS NOT NULL
  AND new_cuit != ''
  AND accountnumber IN (
    SELECT value FROM OPENJSON('["VAL1","VAL2","VAL3"]')
  )
```

O en algunos casos:

```sql
SELECT * FROM cuenta
WHERE customertypecode = 3
  AND new_cuit IS NOT NULL
  AND new_cuit != ''
  AND accountnumber IN (
    SELECT $.[value] FROM OPENJSON('["VAL1","VAL2","VAL3"]') AS $
  )
```

El problema está en la sintaxis `$.[value]` o en el uso de `OPENJSON` cuando no está disponible.

## Solución Implementada

### Estrategia

En lugar de usar `Contains()`, construimos expresiones OR explícitas manualmente usando Expression Trees de .NET. Esto garantiza que EF Core genere SQL estándar compatible con todas las versiones de SQL Server.

### Código de la Solución

```csharp
// Construir consulta base
IQueryable<SapAccount> query = _context.SapAccounts
    .AsNoTracking()
    .Where(a => a.Customertypecode == 3
        && a.NewCuit != null
        && !string.IsNullOrWhiteSpace(a.NewCuit));

// Construir expresión OR manualmente para evitar OPENJSON
if (sociedadFiList.Count > 0)
{
    ParameterExpression parameter = Expression.Parameter(typeof(SapAccount), "a");
    MemberExpression property = Expression.Property(parameter, nameof(SapAccount.Accountnumber));
    BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

    Expression? orExpression = null;
    foreach (string sociedadFi in sociedadFiList)
    {
        BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(sociedadFi, typeof(string)));
        orExpression = orExpression is null
            ? equalsExpression
            : Expression.OrElse(orExpression, equalsExpression);
    }

    if (orExpression is not null)
    {
        BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
        Expression<Func<SapAccount, bool>> lambda = Expression.Lambda<Func<SapAccount, bool>>(combinedExpression, parameter);
        query = query.Where(lambda);
    }
}

List<SapAccount> societyAccounts = await query.ToListAsync(cancellationToken);
```

### SQL Generado por la Solución

La solución genera SQL estándar compatible con todas las versiones:

```sql
SELECT * FROM cuenta
WHERE customertypecode = 3
  AND new_cuit IS NOT NULL
  AND new_cuit != ''
  AND accountnumber IS NOT NULL
  AND (
    accountnumber = 'VAL1'
    OR accountnumber = 'VAL2'
    OR accountnumber = 'VAL3'
  )
```

### Ventajas de esta Solución

1. **Compatibilidad universal**: Funciona con todas las versiones de SQL Server.
2. **Sin dependencias de características avanzadas**: No requiere `OPENJSON` ni niveles de compatibilidad específicos.
3. **Rendimiento predecible**: El plan de ejecución es más predecible y optimizable por SQL Server.
4. **Consistencia**: Sigue el mismo patrón usado en otros lugares del proyecto (`DocumentRepository`, `GetDashboardQueryHandler`).

### Desventajas

1. **Código más complejo**: Requiere construir expresiones manualmente.
2. **Mantenimiento**: Si cambia la estructura de la entidad, hay que actualizar las expresiones.
3. **Límite práctico**: Para listas muy grandes (miles de elementos), puede generar consultas SQL muy largas.

## Verificar Nivel de Compatibilidad de SQL Server

Para verificar el nivel de compatibilidad actual de la base de datos:

```sql
SELECT compatibility_level 
FROM sys.databases 
WHERE name = 'NombreDeTuBaseDeDatos';
```

Los niveles de compatibilidad comunes son:
- **100**: SQL Server 2008
- **110**: SQL Server 2012
- **120**: SQL Server 2014
- **130**: SQL Server 2016 (introduce OPENJSON)
- **140**: SQL Server 2017
- **150**: SQL Server 2019
- **160**: SQL Server 2022

## Cambios Necesarios si se Eleva el Nivel de Compatibilidad

Si se decide elevar el nivel de compatibilidad de SQL Server a 130 o superior para habilitar `OPENJSON`, se pueden simplificar las consultas. Sin embargo, **se recomienda mantener la solución actual** por las siguientes razones:

### Razones para Mantener la Solución Actual

1. **Compatibilidad hacia atrás**: Si alguna vez necesitas trabajar con bases de datos con nivel de compatibilidad inferior, el código seguirá funcionando.
2. **Rendimiento**: Las consultas OR explícitas suelen tener mejor rendimiento que `OPENJSON` para listas pequeñas/medianas.
3. **Simplicidad del plan de ejecución**: SQL Server puede optimizar mejor las condiciones OR explícitas.

### Si se Decide Usar OPENJSON

Si aún así se quiere usar `OPENJSON`, estos son los cambios necesarios:

#### 1. Verificar y Elevar el Nivel de Compatibilidad

```sql
-- Verificar nivel actual
SELECT compatibility_level 
FROM sys.databases 
WHERE name = 'NombreDeTuBaseDeDatos';

-- Elevar a nivel 130 (SQL Server 2016) o superior
ALTER DATABASE NombreDeTuBaseDeDatos 
SET COMPATIBILITY_LEVEL = 130;
```

#### 2. Modificar el QueryHandler

**Antes (Solución Actual):**
```csharp
// Construir expresión OR manualmente
if (sociedadFiList.Count > 0)
{
    ParameterExpression parameter = Expression.Parameter(typeof(SapAccount), "a");
    // ... construcción manual de expresiones OR
}
```

**Después (Usando OPENJSON):**
```csharp
// Usar Contains() directamente - EF Core generará OPENJSON automáticamente
IEnumerable<SapAccount> societyAccounts = await _sapAccountRepository.FindAsync(
    a => sociedadFiList.Contains(a.Accountnumber)
        && a.Customertypecode == 3
        && a.NewCuit != null
        && !string.IsNullOrWhiteSpace(a.NewCuit),
    cancellationToken);
```

#### 3. Consideraciones Adicionales

Si se usa `OPENJSON`, también se debe:

1. **Actualizar otros lugares del código**: Buscar y actualizar todos los lugares donde se construyen expresiones OR manualmente:
   - `DocumentRepository.cs` - métodos `GetByDatesAndSocietiesAsync` y `GetPendingBySocietiesAsync`
   - `GetDashboardQueryHandler.cs` - métodos `BuildSocietyCuitDocumentPredicate` y `BuildSocietyCuitPurchaseOrderPredicate`

2. **Probar exhaustivamente**: Asegurarse de que todas las consultas funcionan correctamente con `OPENJSON`.

3. **Monitorear rendimiento**: Comparar el rendimiento de las consultas antes y después del cambio.

4. **Documentar el cambio**: Actualizar esta documentación y cualquier otra documentación relevante.

#### 4. Ejemplo de SQL con OPENJSON (Nivel 130+)

Con nivel de compatibilidad 130 o superior, EF Core generaría:

```sql
SELECT * FROM cuenta
WHERE customertypecode = 3
  AND new_cuit IS NOT NULL
  AND new_cuit != ''
  AND accountnumber IN (
    SELECT value FROM OPENJSON('["VAL1","VAL2","VAL3"]')
  )
```

## Recomendación Final

**Se recomienda mantener la solución actual** (expresiones OR explícitas) porque:

1. ✅ Funciona con todas las versiones de SQL Server
2. ✅ No depende de características específicas de versión
3. ✅ Mejor rendimiento para listas pequeñas/medianas
4. ✅ Código más predecible y fácil de depurar
5. ✅ Consistente con el resto del proyecto

Solo considerar cambiar a `OPENJSON` si:
- Se garantiza que todas las bases de datos siempre tendrán nivel de compatibilidad 130+
- Se necesita optimizar para listas muy grandes (miles de elementos)
- Se realiza un análisis de rendimiento que demuestre beneficios claros

## Referencias

- [SQL Server Compatibility Levels](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-compatibility-level)
- [OPENJSON (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/functions/openjson-transact-sql)
- [Entity Framework Core - Querying](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Expression Trees in .NET](https://learn.microsoft.com/en-us/dotnet/csharp/expression-trees)
