<!--
Archivo de continuidad para Codex.
Historial basado en el trabajo documentado hasta 2026-10-03.
El repositorio y los resultados de pruebas actuales deben verificarse antes de actualizar estados.
No marcar pasos como completados únicamente porque aparezcan propuestos aquí.
-->

> **Instrucción de continuidad para Codex:** leer primero `../AGENTS.md` y `ARQUITECTURA.md`. B.5.6 fue confirmado por el usuario el 2026-10-04. **B.5.7 está aplazado por decisión del usuario** hasta contar con permisos de diagnóstico SQL. Conservar `CompanyAdministratorsStressTests`; no volver a ejecutar el estrés ni el diagnóstico por ahora. B.5.8 sigue pendiente.

---

# CRM-Sicotyc — Historial técnico de ASP.NET Core Identity (pasos 38–42F)

**Fecha de corte:** 2026-10-03  
**Propósito:** transferencia de contexto a Codex para continuar la implementación del backend.  
**Estado de referencia actualizado el 2026-10-04:** **42F.7.6.6.B.5.6 confirmado por el usuario**, quien reportó que todas las pruebas de recuperación pasaron. B.5.7 está aplazado por falta de permisos de diagnóstico; B.5.8 sigue pendiente.

> **Regla de interpretación.** `CONFIRMADO` significa que el usuario informó expresamente que la implementación, compilación o pruebas pasaron, o que proporcionó código existente. `DOCUMENTADO` significa que se conoce la decisión o el código, pero no se dispone de una verificación independiente del repositorio actual. `PROPUESTO` significa que se entregó código o un plan sin confirmación posterior de ejecución. `PENDIENTE` significa trabajo aún por implementar o verificar. Este archivo no sustituye la inspección del árbol Git actual.

## 1. Resumen ejecutivo y punto exacto de continuación

El módulo Identity se inició en el **paso 38** y evolucionó desde la integración de ASP.NET Core Identity y sus roles hasta la administración multiempresa de usuarios y la protección de invariantes bajo concurrencia real de SQL Server. Se dispone de autenticación, registro vinculado a Company, autorización por rol y empresa, administración individual de usuarios, desactivación masiva exclusiva del SuperAdministrator y un mecanismo de transacción con bloqueo por Company.

**Último trabajo confirmado:**

1. Se consolidó el handler de desactivación masiva en `Application.Features.Companies.Commands.DeactivateCompanyUsers`, descartando el namespace duplicado propuesto bajo `Features.Users.Commands.DeactivateCompanyUsers`.
2. Se revisó y modificó `IdentityService.DeactivateUsersByCompanyAsync` para comprobar cancelación, omitir usuarios ya inactivos y detenerse ante el primer error de `UserManager.UpdateAsync`.
3. El usuario confirmó **«Todo OK»** tras compilar y ejecutar las pruebas sugeridas. Esto no acredita por sí solo una prueba específica de rollback de datos.
4. Se implementaron las cuatro pruebas de recuperación para **42F.7.6.6.B.5.6** y el usuario confirmó el 2026-10-04 que todas pasaron sobre SQL Server. Codex verificó compilación y 93 pruebas sin SQL Server; la ejecución de recuperación fue reportada por el usuario.

**Al retomar 42F.7.6.6.B.5.7 con una cuenta autorizada:** recuperar el gráfico del deadlock y validar estrés con operaciones mixtas sobre la misma Company y Companies distintas, comprobando invariantes, aislamiento y liberación de bloqueos. Conservar las pruebas de recuperación B.5.6.

## 2. Contexto tecnológico y decisiones arquitectónicas

| Área | Decisión o estado conocido |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API, C# |
| Persistencia | EF Core 10.x, SQL Server; Dapper disponible para otras consultas |
| Identidad | ASP.NET Core Identity con `ApplicationUser`, `ApplicationRole` y claves `Guid` |
| Contexto | `SicotycDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` |
| Arquitectura | Clean Architecture: API, Application, Domain, Infrastructure |
| Casos de uso | Commands/handlers explícitos, **sin MediatR** |
| Autorización | JWT/Identity, roles y comprobaciones de pertenencia a Company dentro de los handlers |
| Pruebas | xUnit, `Jarasoft.Sicotyc.Test`, `CustomWebApplicationFactory`, SQLite in-memory y pruebas específicas sobre SQL Server real |
| Frontend | Angular + TypeScript + Signals; la visibilidad de opciones del menú depende del rol, pero el backend debe imponer siempre los permisos |

Estructura de referencia:

```text
CRM-Sicotyc/
├── backend/
│   ├── CRM-Sicotyc.slnx
│   ├── Jarasoft.Sicotyc.API/
│   ├── Jarasoft.Sicotyc.Application/
│   ├── Jarasoft.Sicotyc.Domain/
│   ├── Jarasoft.Sicotyc.Infrastructure/
│   └── Jarasoft.Sicotyc.Test/
└── frontend/
    └── crm-sicotyc-web/
```

**Responsabilidades:** Domain contiene las entidades de negocio; Application contiene contratos, comandos, handlers, DTO y reglas de aplicación; Infrastructure implementa EF Core, repositorios, Identity, transacciones y acceso SQL Server; API publica controladores, configura DI/autenticación/autorización y traduce errores a HTTP. No trasladar `UserManager` ni detalles EF a Application.

## 3. Cronología de implementación, pasos 38–42F

La numeración de algunos subpasos históricos no está completamente disponible. Se documentan solo los identificadores recuperables y se evita atribuir funcionalidades a números no verificados.

| Paso | Implementación o decisión conocida | Estado |
|---|---|---|
| **38** | Integración de ASP.NET Core Identity; `SicotycDbContext` hereda de `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`; configuración de servicios de Identity | **CONFIRMADO**; se corrigieron registros faltantes de `AddSignInManager` y `AddDefaultTokenProviders` |
| **39** | Roles de aplicación y `IdentitySeeder` para inicialización | **CONFIRMADO** |
| **40A** | Modelo Company: `Id` Guid, `Ruc` único, `NombreEmpresa`, `Email`, `IsActive`, `CreatedAt`, `Direccion`, `IdDist` vinculado a Ubigeos | **CONFIRMADO** |
| **40B** | `ApplicationUser.CompanyId` Guid, FK restrictiva a Company; registro exige RUC y nombre de empresa al crearla | **CONFIRMADO** |
| **40C** | Campos SUNAT en Company y regla de enriquecimiento asíncrono | **CONFIRMADO** el modelo/regla; **PENDIENTE DE VERIFICAR** implementación del job externo |
| **40D** | Flujo de registro integrado, respuesta `201 Created` | **CONFIRMADO** |
| **41A–41C** | Pruebas de registro/autenticación y correcciones | **CONFIRMADO** como conjunto; detalle exacto por letra no recuperable |
| **41D** | Flujo de Identity/autenticación concluido | **CONFIRMADO** |
| **41E–41F** | Pasos ejecutados, pruebas de autenticación y administración; `admin-test` omitido porque no existía el endpoint | **CONFIRMADO** como ejecución reportada; no asignar funciones más precisas sin revisar historial o Git |
| **42A–42E** | Evolución de roles, políticas, endpoints y administración de usuarios | **DOCUMENTADO** por código y pruebas posteriores; desglose exacto por letra no recuperable |
| **42F.3** | Reglas finales de Administrator, SuperAdministrator y usuarios sin privilegios administrativos | **CONFIRMADO** |
| **42F.6 / 42F.6.10** | Pruebas de administración de usuarios y correcciones, incluido `CompanyUsersAdministrationTests` | **CONFIRMADO** |
| **42F.7.6.6.B.4** | Transacción y bloqueo SQL Server por Company mediante `AdministratorProtectionService` e `ICompanyAdministrationLock` | **CONFIRMADO** |
| **42F.7.6.6.B.5.1** | Dos desactivaciones individuales simultáneas de Administrators de la misma Company | **CONFIRMADO**, prueba satisfactoria |
| **42F.7.6.6.B.5.2** | Desactivación individual concurrente con cambio de rol Administrator → Operations | **CONFIRMADO**, prueba satisfactoria |
| **42F.7.6.6.B.5.3** | Eliminación concurrente con desactivación individual | **CONFIRMADO**, prueba satisfactoria |
| **42F.7.6.6.B.5.4** | Operaciones concurrentes sobre Companies distintas; corrección del deadlock SQL Server 1205 | **CONFIRMADO**, cinco ejecuciones consecutivas satisfactorias tras cambio de aislamiento |
| **42F.7.6.6.B.5.5** | Desactivación masiva concurrente con desactivación individual; consolidación de handler y revisión de servicio | **CONFIRMADO** por el usuario tras correcciones y pruebas; conservar la distinción entre reporte y verificación independiente |
| **42F.7.6.6.B.5.6** | Rollback, cancelación, liberación de bloqueo y recuperación | **CONFIRMADO** por el usuario el 2026-10-04 |
| **42F.7.6.6.B.5.7** | Pruebas de estrés de solicitudes concurrentes | **APLAZADO POR EL USUARIO / FALLOS SIN RESOLVER** |
| **42F.7.6.6.B.5.8** | Refactorización, regresión completa y cierre | **PENDIENTE** |

## 4. Entidades, relaciones y persistencia

### 4.1. Identity

- `ApplicationUser` deriva del usuario Identity de clave `Guid`; incorpora `CompanyId` y `IsActive`. En el registro se utiliza también información personal como `FirstName`, cuya nulabilidad provocó una corrección en una prueba de `/api/auth/me`.
- `ApplicationRole` es el rol Identity con clave `Guid`.
- `SicotycDbContext` deriva de `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` y contiene el modelo de negocio, incluida Company y Ubigeos.
- `AspNetUsers.CompanyId` referencia a `Companies.Id`, con comportamiento restrictivo (`Restrict`) para evitar eliminaciones en cascada no deseadas.
- Las tablas estándar de Identity (`AspNetUsers`, `AspNetRoles` y tablas de relación/tokens/claims) se gestionan mediante EF Core y migraciones. La lista exacta de migraciones y su estado aplicado debe comprobarse en Git y la base de datos.

### 4.2. Company

Campos definidos durante 40A–40C:

| Campo | Tipo/regla documentada |
|---|---|
| `Id` | `Guid`, clave primaria |
| `Ruc` | `varchar(11)`, obligatorio, índice único |
| `NombreEmpresa` | `nvarchar(200)`, obligatorio |
| `Email` | `nvarchar(150)`, opcional; contacto de empresa |
| `Direccion` | `nvarchar(300)` |
| `IdDist` | `char(6)`, FK a `Ubigeos.IDDIST`, `Restrict` |
| `IsActive` | Booleano |
| `CreatedAt` | Fecha de creación |
| `ActualizadoDeSunat` | `bit`, valor inicial `false` |
| `CondicionContribuyente` | `nvarchar(50)` |
| `EstadoContribuyente` | `nvarchar(50)` |
| `FechaActualizacionSunat` | `datetime2`, nullable |
| `NombreComercial` | `nvarchar(200)` |

**Regla SUNAT:** si no existe la empresa para el RUC indicado, se crea con `ActualizadoDeSunat = false`. Se definió un proceso asíncrono externo para completar/actualizar información SUNAT, establecer `ActualizadoDeSunat = true` y actualizar `NombreEmpresa` utilizando `NombreComercial` según la regla acordada. **No hay confirmación de que el job ya esté implementado, programado ni operativo.** Revisar también si la regla de sustitución de razón social por nombre comercial es la deseada antes de automatizarla.

### 4.3. Registro multiempresa

1. Todo usuario debe pertenecer a una Company; RUC obligatorio.
2. Durante el registro se busca la Company por RUC.
3. Si existe, el usuario queda asociado a ella; no crear duplicados.
4. Si no existe, se requiere `NombreEmpresa` y se crea la Company pendiente de actualización SUNAT.
5. El correo de contacto de la Company es opcional.
6. El registro exitoso devuelve **HTTP 201 Created** con `userId`, `companyId` y `email`.

**No inferir** que el registro otorga automáticamente privilegios administrativos a todos los usuarios. Comprobar el flujo real de asignación del primer Administrator y el seeder antes de cambiarlo.

## 5. Roles, políticas de autorización y reglas de negocio

### 5.1. Roles identificados

Los nombres **confirmados por código/pruebas** son `SuperAdministrator`, `Administrator`, `Operations`, `Billing` y `User`. En conversaciones anteriores también se mencionaron `Coordinator` y `Facturador`; se resolvió una incidencia de «solo veo 5 roles» y se trabajó con un conjunto esperado de siete. **Antes de fijar una enumeración definitiva o traducir nombres, inspeccionar `ApplicationRoles`, `ApplicationRoleRules` y `IdentitySeeder` en el repositorio actual.** No asumir que `Billing` y `Facturador` sean dos roles distintos sin comprobarlo.

`ApplicationRoleRules` contiene listas de roles asignables por `Administrator` y por `SuperAdministrator`; los handlers consultan esas listas en vez de permitir cualquier nombre de rol.

### 5.2. Administrator

- Administra usuarios **solo de su Company**.
- Puede activar y desactivar usuarios de su Company con las restricciones correspondientes.
- No puede administrar usuarios de otras Companies; algunos handlers devuelven `NotFoundException` para no revelar su existencia.
- No puede modificar a un `SuperAdministrator`.
- No puede desactivarse ni eliminarse a sí mismo.
- Puede designar a otro usuario como `Administrator`, conforme a `ApplicationRoleRules`.
- No puede desactivar, eliminar ni quitar el rol al **último Administrator activo** mediante operaciones individuales.
- No puede realizar la desactivación masiva de una Company.

### 5.3. SuperAdministrator

- Puede administrar usuarios de distintas Companies, sujeto a las restricciones de cada operación.
- No puede desactivarse a sí mismo.
- Es el único autorizado para desactivar masivamente todos los usuarios de una Company distinta de la propia.
- La operación masiva **sí puede desactivar al último Administrator** de la Company de destino: es una excepción explícita a la invariante de las operaciones individuales.
- Puede designar un Administrator para recuperar o asegurar la administración de una Company.
- La regla general de «al menos un Administrator activo por Company» debe interpretarse junto con la excepción de desactivación masiva, no como una restricción absoluta sobre todos los estados posibles.

### 5.4. Otros roles

`Operations`, `Billing` y otros roles no administrativos no pueden administrar usuarios: respuesta esperada `403 Forbidden` para operaciones protegidas. La interfaz Angular puede ocultar opciones por rol, pero el control obligatorio reside en el backend.

### 5.5. Errores y contratos HTTP documentados

| Condición | Tratamiento documentado |
|---|---|
| Solicitud sin autenticación/token | `401 Unauthorized` |
| Rol sin permiso | `403 Forbidden` |
| Company inexistente en desactivación masiva | `404 Not Found` |
| Autodesactivación masiva de la propia Company | `400 Bad Request` |
| Intento de quitar el último Administrator activo | `ValidationException` → `400 Bad Request` según el contrato existente |
| Usuario ajeno a la Company | Se emplea `NotFoundException` en determinados handlers para preservar aislamiento |

No extrapolar todos estos códigos a endpoints no revisados; confirmar las rutas y el middleware de excepciones reales.

## 6. Interfaces y servicios relevantes

### 6.1. Contratos de Application

| Contrato/servicio | Responsabilidad conocida |
|---|---|
| `ICurrentUser` | Identidad del solicitante: `IsAuthenticated`, `UserId`, `CompanyId`, `Roles`, `IsSuperAdministrator` |
| `IIdentityService` | Buscar usuarios, gestionar estado y roles, eliminar usuarios, contar Administrators activos y desactivar usuarios de una Company |
| `ICompanyRepository` | Consultar existencia y datos de Company; `ExistsByIdAsync(Guid, CancellationToken)` está confirmado en el handler de desactivación masiva |
| `IUnitOfWork` | Unidad de trabajo/transacción compartida por operaciones protegidas |
| `ICompanyAdministrationLock` | Exclusión mutua por Company mediante SQL Server |
| `AdministratorProtectionService` | Abre transacción, adquiere bloqueo, ejecuta callback, confirma o revierte y libera el bloqueo |
| `ApplicationRoles` | Constantes de roles |
| `ApplicationRoleRules` | Roles asignables según privilegios del solicitante |

Operaciones de `IIdentityService` identificadas en el código revisado:

```csharp
GetUserByIdAsync(userId, cancellationToken);
SetUserActiveStatusAsync(userId, isActive, cancellationToken);
SetUserRoleAsync(userId, role, cancellationToken);
DeleteUserAsync(userId, cancellationToken);
CountActiveUsersInRoleAsync(companyId, role, cancellationToken);
DeactivateUsersByCompanyAsync(companyId, cancellationToken);
```

Estas llamadas expresan los métodos conocidos; **no son una reproducción íntegra de la interfaz ni garantizan el tipo exacto de todos los retornos**. Consultar la interfaz antes de generar código.

**Corrección de contrato:** se produjo `CS0535` porque `ICompanyRepository` declaraba `GetByIdAsync(Guid, CancellationToken)` sin implementación equivalente en `CompanyRepository`. Se propuso alinear las firmas o conservar ambas sobrecargas si había consumidores anteriores. El usuario confirmó posteriormente que la solución compilaba; inspeccionar la versión actual antes de añadir otra sobrecarga. El handler de desactivación masiva consolidado usa `ExistsByIdAsync`, no necesita `GetByIdAsync`.

### 6.2. `AdministratorProtectionService` y bloqueo SQL Server

Patrón documentado:

```csharp
await administratorProtection.ExecuteAsync(
    companyId,
    async ct =>
    {
        // Reconsultar y revalidar bajo el bloqueo.
        // Ejecutar las modificaciones con el mismo DbContext.
    },
    cancellationToken);
```

Características conocidas:

- Transacción explícita compartida con las operaciones EF/Identity del ámbito (`Scoped`).
- Bloqueo exclusivo **por Company** con `sp_getapplock` y `LockOwner = 'Transaction'`.
- El bloqueo se mantiene hasta finalizar la transacción; las operaciones sobre la misma Company se serializan.
- En caso de excepción se hace rollback; para evitar que un token cancelado impida revertir, se documentó el uso de `CancellationToken.None` en el rollback.
- **Aislamiento final confirmado: `ReadCommitted`**, no `Serializable`. El cambio resolvió el deadlock 1205 observado entre operaciones de Companies diferentes y B.5.4 pasó cinco veces consecutivas.
- Las comprobaciones sensibles deben repetirse **después** de adquirir el bloqueo para evitar condiciones de carrera.

**Condición de atomicidad todavía pendiente de prueba directa:** `IUnitOfWork`, repositorios y `UserManager<ApplicationUser>` deben compartir la misma instancia de `SicotycDbContext` y la misma transacción. No crear contextos, ámbitos ni transacciones independientes dentro de `IdentityService.DeactivateUsersByCompanyAsync`.

### 6.3. `DeactivateCompanyUsersHandler` — implementación consolidada

**Namespace definitivo:**

```text
Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers
```

**Constructor confirmado:**

```csharp
DeactivateCompanyUsersHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    ICompanyRepository companyRepository,
    AdministratorProtectionService administratorProtection)
```

**Firma confirmada:**

```csharp
Task<DeactivateCompanyUsersResult> HandleAsync(
    DeactivateCompanyUsersCommand command,
    CancellationToken cancellationToken = default)
```

Comportamiento: valida autenticación y `SuperAdministrator`; rechaza la propia Company; verifica existencia mediante `ExistsByIdAsync`; entra en `administratorProtection.ExecuteAsync`; revalida restricciones y llama a `identityService.DeactivateUsersByCompanyAsync(companyId, ct)`; si `Succeeded` es falso lanza `ValidationException` para provocar rollback; tras el commit devuelve `DeactivateCompanyUsersResult(companyId, affectedUsers)`. La respuesta HTTP expone `CompanyId` y `DeactivatedUsers`.

Se propuso mejorar el handler con `ArgumentNullException.ThrowIfNull`, validación de `Guid.Empty`, comprobación de cancelación y traslado de la verificación de existencia al interior del bloqueo. **No está probado mediante inspección del repositorio si cada uno de estos ajustes quedó incorporado**; conservar el contrato y comparar con el archivo real.

**No duplicar** el handler ni el comando en `Application.Features.Users.Commands.DeactivateCompanyUsers`: esa propuesta produjo `CS0104` (referencia ambigua) y luego `CS0234` (namespace inexistente). Se resolvió usando el handler existente bajo `Features.Companies`.

### 6.4. `IdentityService.DeactivateUsersByCompanyAsync` — último cambio confirmado

Firma:

```csharp
Task<IdentityBulkOperationResult> DeactivateUsersByCompanyAsync(
    Guid companyId,
    CancellationToken cancellationToken = default)
```

La versión inicial consultaba usuarios activos de la Company, asignaba `IsActive = false` y llamaba a `userManager.UpdateAsync(user)` por cada uno; acumulaba errores y continuaba aun después de un fallo.

La versión recomendada y posteriormente reportada como satisfactoria:

1. Ejecuta `cancellationToken.ThrowIfCancellationRequested()` al inicio.
2. Rechaza `Guid.Empty` con `IdentityBulkOperationResult(false, 0, errors)`.
3. Consulta únicamente usuarios con `CompanyId == companyId && IsActive` mediante `ToListAsync(cancellationToken)`.
4. Si no hay usuarios activos, devuelve éxito con cero afectados (idempotencia).
5. Comprueba cancelación **en cada iteración**.
6. Establece `user.IsActive = false` y llama a `UserManager.UpdateAsync(user)`.
7. **Ante el primer error**, devuelve `Succeeded = false`, los errores de Identity y el número de actualizaciones que habían tenido éxito; el handler lanza y el servicio protector debe revertir toda la transacción.
8. Comprueba cancelación antes de devolver éxito.

`IdentityBulkOperationResult` expone al menos `Succeeded`, `AffectedUsers` y `Errors`. `AffectedUsers` antes del commit indica actualizaciones realizadas durante el intento; **si hay rollback, no representa modificaciones persistidas**.

**Limitación importante:** `UserManager.UpdateAsync` no recibe un `CancellationToken` en esa llamada; la cancelación se comprueba antes de cada actualización y al final, no puede interrumpir necesariamente una actualización ya iniciada. Las pruebas de B.5.6 deben contemplarlo.

### 6.5. Administración individual

- `ChangeUserStatusHandler`: comprueba autenticación, privilegios, Company, protección del SuperAdministrator y autodesactivación. Las desactivaciones se ejecutan dentro de `AdministratorProtectionService`; el usuario objetivo se consulta de nuevo y se revalidan permisos y pertenencia antes de cambiar estado. Las activaciones tienen un camino separado porque no reducen el número de Administrators activos; revisar si se requiere una política de bloqueo adicional para otros invariantes.
- `ChangeUserRoleHandler`: valida roles asignables, reconsulta bajo bloqueo, evita retirar el rol al último Administrator activo y llama a `SetUserRoleAsync`.
- `DeleteUserHandler`: reconsulta y revalida bajo bloqueo, impide autoeliminación, protege al último Administrator activo y llama a `DeleteUserAsync`.

## 7. Endpoints y contratos HTTP identificados

| Endpoint | Evidencia/estado |
|---|---|
| `POST /api/auth/register` | Registro vinculado a Company; éxito `201 Created` con `userId`, `companyId`, `email` — **confirmado** |
| `POST /api/auth/login` | Utilizado por `AuthenticatedIntegrationTestBase` para obtener `AccessToken` — **confirmado** |
| `GET /api/auth/me` | Probado con JWT válido; se corrigió una incidencia con `FirstName` nulo — **confirmado** |
| `PATCH /api/companies/{companyId}/users/deactivate` | Desactivación masiva; pruebas HTTP verifican `200 OK`, `CompanyId`, `DeactivatedUsers`, aislamiento y autorización — **confirmado** |
| `GET /api/auth/admin-test` | **No implementado** en la etapa documentada; se omitió su prueba; no presentarlo como endpoint existente |
| Otros endpoints de administración de usuarios | Hay handlers y pruebas de creación, cambio de estado, rol y eliminación, pero **las rutas HTTP exactas no están suficientemente documentadas aquí**; inspeccionar `UsersController` y controladores asociados |

La respuesta de desactivación masiva está cubierta por pruebas para ausencia de token (`401`), rol `Operations` (`403`), rol `Administrator` (`403`), SuperAdministrator contra su propia Company (`400`), Company inexistente (`404`), protección de usuarios de otras Companies, desactivación del último Administrator y desactivación de cuatro usuarios con recuento `4`.

## 8. Pruebas y entorno de integración

### 8.1. Infraestructura de pruebas

- Proyecto: `Jarasoft.Sicotyc.Test`, xUnit.
- `CustomWebApplicationFactory` usa SQLite in-memory para pruebas de API/Identity y configuración específica de EF.
- `AuthenticatedIntegrationTestBase` prepara dos Companies, usuarios/roles, autentica mediante `POST /api/auth/login` y permite llamadas con bearer token.
- `SqlServerWebApplicationFactory(connectionString)` se usa para pruebas de concurrencia que requieren semántica real de SQL Server, transacciones y `sp_getapplock`.
- Variable de entorno usada en PowerShell:

```powershell
$env:SICOTYC_TEST_SQLSERVER = "Server=localhost\SQLEXPRESS;Database=Sicotyc_IntegrationTests;Trusted_Connection=True;TrustServerCertificate=True;"
```

- Las pruebas SQL Server se identifican con `[Trait("Category", "SqlServerIntegration")]`.
- **Precaución:** no ejecutar `AuthenticatedIntegrationTestBase.SeedEnvironmentAsync` contra la base SQL Server de integración; el seeding de SQLite elimina usuarios, Companies y Ubigeos y podría destruir datos de otras pruebas.
- Para concurrencia, usar ámbitos de DI independientes por operación y evitar compartir un `DbContext` entre tareas simultáneas.

### 8.2. Cobertura histórica confirmada

- `UbigeosControllerTests` y otras pruebas previas pasaron tras corregir la configuración EF.
- `LoginControllerTests.Me_ShouldReturnOk_WhenTokenIsValid`: corrección por `FirstName` nulo.
- `UsersControllerTests.CreateUser_ShouldCreateUserInAdministratorsCompany`: corrección de expectativas del rol `Operations`.
- `CompanyUsersAdministrationTests`: cobertura HTTP de desactivación masiva, permisos y aislamiento.
- `CompanyAdministratorsConcurrencyTests`: escenarios B.5.1–B.5.5 reportados satisfactorios.

### 8.3. Escenarios B.5.1–B.5.5

| Prueba | Escenario | Invariante/resultado | Estado |
|---|---|---|---|
| B.5.1 | Dos desactivaciones individuales simultáneas de los dos últimos Administrators de una Company | Una tiene éxito, otra falla con `ValidationException`; queda exactamente un Administrator activo | **CONFIRMADO** |
| B.5.2 | Desactivación individual y cambio de rol Administrator → Operations concurrentes | Una operación tiene éxito, la otra falla; queda un Administrator activo | **CONFIRMADO** |
| B.5.3 | Eliminación y desactivación individual concurrentes | Una operación tiene éxito, la otra falla; queda un Administrator activo | **CONFIRMADO** |
| B.5.4 | Desactivaciones en dos Companies distintas | Ambas tienen éxito; cada Company conserva un Administrator activo | **CONFIRMADO**, cinco ejecuciones seguidas tras corregir aislamiento |
| B.5.5 | Desactivación masiva de Company B y desactivación individual de un usuario de B | La masiva termina correctamente; la individual puede terminar o fallar por validación; todos los usuarios de B quedan inactivos y el SuperAdministrator de A sigue activo | **CONFIRMADO** por reporte del usuario tras correcciones |

### 8.4. B.5.6: implementación confirmada por el usuario

Se creó `backend/Jarasoft.Sicotyc.Test/Integration/Concurrency/AdministratorProtectionRecoveryTests.cs`
con los cuatro métodos enumerados abajo. Las pruebas de excepción y cancelación realizan
la desactivación de dos usuarios mediante `IIdentityService.DeactivateUsersByCompanyAsync`
(que llama a `UserManager.UpdateAsync`), verifican las escrituras con una consulta sin tracking
dentro de la transacción y comprueban el rollback desde un scope/DbContext nuevo.
La prueba de liberación mantiene abierta la primera conexión y verifica que otra operación
pueda adquirir el bloqueo y confirmar cambios. La recuperación reutiliza la misma instancia
scoped del servicio, `UnitOfWork` y `DbContext`, y confirma una escritura SQL parametrizada.
Cada prueba crea su Company y usuarios; la limpieza elimina solo esos datos y reutiliza
un Ubigeo existente, sin borrarlo. No se modificaron servicios ni reglas de negocio.

**Validación local:** `dotnet build backend/CRM-Sicotyc.slnx --no-restore --verbosity minimal`
terminó con cero errores y una advertencia existente `CS9113` en `GlobalExceptionHandler`.
El listado de pruebas (`dotnet test backend/Jarasoft.Sicotyc.Test/Jarasoft.Sicotyc.Test.csproj
--no-build --list-tests --filter FullyQualifiedName~AdministratorProtectionRecoveryTests
--verbosity minimal`) confirmó el descubrimiento de los cuatro métodos; no los ejecutó.
La regresión sin SQL Server (`dotnet test backend/Jarasoft.Sicotyc.Test/Jarasoft.Sicotyc.Test.csproj
--no-restore --filter 'Category!=SqlServerIntegration' --verbosity minimal`) terminó con
**93 pruebas satisfactorias, cero fallidas y cero omitidas**. En la primera ejecución fallaron
dos expectativas antiguas de `DeactivateCompanyUsersHandlerTests` que exigían `Serializable`;
se actualizaron a `ReadCommitted`, conforme al código y la decisión de B.5.4, y se repitió
la regresión satisfactoriamente. No se cambió el aislamiento del servicio.
Las pruebas SQL Server no se ejecutaron desde Codex porque `SICOTYC_TEST_SQLSERVER`
no está configurada en su sesión. El usuario configuró su terminal y el 2026-10-04
confirmó: **«ya pasaron todas las pruebas»**, referido a las cuatro pruebas de recuperación.
**B.5.6 queda confirmado por ese reporte.** No se dispone del log ni de un recuento de
la regresión SQL Server completa; esa evidencia queda pendiente para el cierre B.5.8.
La base exclusiva de integración debe tener al menos un Ubigeo.

#### Antecedente de la propuesta inicial

Se propuso `Jarasoft.Sicotyc.Test/Integration/Concurrency/AdministratorProtectionRecoveryTests.cs` con cuatro métodos:

```text
ExecuteAsync_ShouldRollback_WhenOperationThrows
ExecuteAsync_ShouldRollback_WhenCancelled
ExecuteAsync_ShouldReleaseLock_AfterRollback
ExecuteAsync_ShouldAllowNewTransaction_AfterFailure
```

En la propuesta inicial estos métodos no estaban confirmados como implementados ni ejecutados. Los dos primeros ejemplos provocaban una excepción/cancelación en el callback sin modificar datos persistidos y no demostraban rollback de datos. La implementación actual descrita arriba reemplaza ese enfoque; sus resultados SQL Server siguen pendientes de ejecución.

La propuesta también presupone que `AdministratorProtectionService` está registrado en DI y que `SqlServerWebApplicationFactory` acepta la cadena de conexión; si el proyecto construye el servicio manualmente, reutilizar el patrón real de `CompanyAdministratorsConcurrencyTests`.

## 9. Incidencias y correcciones históricas

| Incidencia | Diagnóstico y resolución documentados |
|---|---|
| Registro de Identity incompleto | Se añadieron/corrigieron `AddSignInManager` y `AddDefaultTokenProviders` |
| `ApplicationRoles` no reconocido | Se corrigió la referencia/namespace en el trabajo del paso 42D |
| Solo se veían cinco roles | Incidencia resuelta en la etapa de roles; comprobar el conjunto final real en `ApplicationRoles`/seeder |
| Registro devolvía `200` | Se ajustó a `201 Created` |
| `/api/auth/me` fallaba con `FirstName` nulo | Se corrigió el tratamiento/expectativa en la prueba correspondiente |
| Creación de usuario por Administrator | Se corrigió la expectativa del rol `Operations` en la prueba |
| `IUnitOfWork.SaveChangesAsync` | Se reportó error de firma/contrato en 46F.6.10 (identificador histórico referido); estado final exacto del método debe verificarse en código |
| EF Core SQL Server y SQLite registrados simultáneamente | `InvalidOperationException`: solo un proveedor EF por service provider; se corrigió el reemplazo/configuración del DbContext de pruebas; las pruebas pasaron |
| EF Tools 10.0.7 frente a paquetes 10.0.12 | Se detectó desajuste de versiones; se ajustaron herramientas/paquetes de diseño durante las migraciones |
| Deadlock SQL Server `1205` en B.5.4 | Se cambió el aislamiento de `AdministratorProtectionService` de `Serializable` a `ReadCommitted`; se ajustaron pruebas y B.5.4 pasó cinco veces consecutivas |
| `CS0535` en `CompanyRepository` | La interfaz exigía `GetByIdAsync(Guid, CancellationToken)` y la clase no coincidía; se propuso alinear firmas; compilación posterior reportada satisfactoria |
| `CS0104` para `DeactivateCompanyUsersHandler` | Referencia ambigua entre `Features.Companies` y una segunda implementación propuesta en `Features.Users`; se decidió conservar la implementación original en `Features.Companies` |
| `CS0234` para `Features.Users.Commands.DeactivateCompanyUsers` | El namespace no existía; se corrigieron imports/alias hacia `Features.Companies.Commands.DeactivateCompanyUsers` |
| Servicio de desactivación masiva continuaba tras error | Se ajustó para detenerse al primer fallo de `UserManager.UpdateAsync` y comprobar cancelación durante el bucle |

**Nota:** los identificadores históricos aislados cuyo detalle no está disponible no deben convertirse en tareas ficticias ni utilizarse para deducir implementaciones.

## 10. Trabajo pendiente y criterios de aceptación

### Criterios de B.5.6 — recuperación confirmada por el usuario

1. Inspeccionar el repositorio y comprobar si existe `AdministratorProtectionRecoveryTests.cs`; no duplicarlo si ya existe.
2. Reutilizar `SqlServerWebApplicationFactory`, los helpers de creación de Company/usuario y el patrón de DI de `CompanyAdministratorsConcurrencyTests`.
3. **Prueba de rollback por excepción:** crear datos exclusivos de la prueba; abrir transacción protegida; persistir una modificación con el `SicotycDbContext` compartido; lanzar excepción; consultar desde otro ámbito/contexto y verificar que los valores originales permanecen.
4. **Prueba de rollback por cancelación:** modificar y guardar dentro de la transacción; cancelar el token y propagar `OperationCanceledException`; verificar desde otro ámbito que la modificación se revirtió.
5. **Liberación de bloqueo:** tras rollback, un segundo ámbito debe poder adquirir el mismo `companyId` sin quedarse bloqueado.
6. **Recuperación del servicio:** después de una operación fallida, una nueva transacción debe poder completarse; confirmar si el diseño admite reutilizar la misma instancia scoped y probar ese contrato si corresponde.
7. Verificar que `UserManager<ApplicationUser>` y `IUnitOfWork` resuelven el **mismo `SicotycDbContext`** dentro del ámbito y que `UserManager.UpdateAsync` participa en la transacción externa. Preferiblemente añadir una prueba que falle después de haber actualizado al menos un usuario mediante Identity y verifique que todos siguen en su estado anterior.
8. Aislar datos por prueba, limpiar solo los registros propios y evitar interferencias entre ejecuciones paralelas. No depender de `Companies.FirstAsync()` ni de setters públicos supuestos; inspeccionar el modelo real o usar SQL parametrizado sobre datos creados para la prueba.
9. Ejecutar `dotnet build`, las pruebas de recuperación, las de concurrencia y la regresión SQL Server. Registrar nombres exactos de pruebas y resultados antes de declarar B.5.6 completado.

Comandos de referencia:

```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~AdministratorProtectionRecoveryTests"
dotnet test --filter "FullyQualifiedName~CompanyAdministratorsConcurrencyTests"
dotnet test --filter "Category=SqlServerIntegration"
```

### Después de B.5.6

**Incidencia de B.5.7 reportada por el usuario (2026-10-04):** una operación
`Deactivate` falló al adquirir el bloqueo con código `-1`. Esto confirma que venció
`@LockTimeout = 10000`; no demuestra por sí solo un deadlock ni su causa.
La inspección detectó que `IdentityService.CountActiveUsersInRoleAsync` utilizaba
`GetUsersInRoleAsync` para cargar usuarios de todas las Companies y filtrar en memoria.
Se reemplazó por un `CountAsync` en SQL filtrado por Company, actividad y rol normalizado,
usando el `SicotycDbContext` scoped existente. Esto reduce el alcance lógico de la consulta
y propaga el CancellationToken; la resolución del timeout requiere repetir SQL Server.
No se alteraron reglas, interfaces, `ReadCommitted`, tiempos de espera ni número de operaciones.
La expectativa del rechazo en estrés se ajustó al mensaje efectivo de cada handler:
el mensaje de desactivación no contiene la palabra «último».

Se añadieron `IdentityServiceTests.CountActiveUsersInRoleAsync_ShouldFilterCompanyStatusAndNormalizedRole`
y `IdentityServiceTests.CountActiveUsersInRoleAsync_ShouldPropagateCancellation` sobre SQLite.
La primera también comprueba lectura dentro de la transacción y recuento posterior al rollback.
`dotnet test backend/Jarasoft.Sicotyc.Test/Jarasoft.Sicotyc.Test.csproj --no-restore
--filter 'Category!=SqlServerIntegration' --verbosity minimal` compiló correctamente
(advertencia existente CS9113) y finalizó con **95 pruebas satisfactorias, cero fallidas**.
El timeout SQL Server sigue pendiente de revalidación; no se afirma que haya quedado resuelto.

**Seguimiento (2026-10-04):** el usuario reportó que los errores de bloqueo continúan
después de ajustar el recuento. Se añadió diagnóstico exclusivamente en
`CompanyAdministratorsStressTests`, manteniendo carga, aislamiento y esperas.
Cada operación registra Company, usuario de prueba, tipo, duración y excepción.
Una conexión observadora consulta cada dos segundos las DMV de sesiones, requests
y locks durante la carga, filtradas por el ApplicationName único del caso.
La salida conserva tres snapshots recientes con SPID, transacciones abiertas,
sesión bloqueadora, tipo/recurso de espera, fragmento de comando y APPLOCK concedido/en espera.
También registra threads y trabajos pendientes del ThreadPool. No registra cadenas
de conexión ni parámetros SQL. Si faltan permisos de lectura DMV, registra el código
del error y conserva el resultado original de las operaciones. No se añaden reintentos
ni se aceptan los timeouts como resultado válido. La ejecución SQL Server sigue pendiente.

Para capturar el diagnóstico, ejecutar desde `backend` en la terminal configurada:

```powershell
dotnet test --filter "FullyQualifiedName~CompanyAdministratorsStressTests" --logger "console;verbosity=detailed"
```

Compartir el caso fallido (companyCount), los bloques `SQL SNAPSHOT`, `APPLOCK`,
`RESULTADO` y el `Error Message` completo. Esos datos permitirán distinguir la
cola normal del APPLOCK de un bloqueo SQL que mantiene ocupada la transacción.

El filtro histórico `FullyQualifiedName~CompanyAdministratorsConcurrencyTests` ejecuta
solo esa clase; no incluye `CompanyAdministratorsStressTests` ni
`AdministratorProtectionRecoveryTests`. Para todas las pruebas SQL Server usar
`Category=SqlServerIntegration`; para toda la suite usar `dotnet test` sin filtro,
desde una terminal con la conexión de integración configurada.

**Validación local de B.5.7 (2026-10-04):**
`dotnet build backend/CRM-Sicotyc.slnx --no-restore --verbosity minimal` terminó con
cero errores y cero advertencias. `dotnet test backend/Jarasoft.Sicotyc.Test/Jarasoft.Sicotyc.Test.csproj
--no-build --list-tests --filter FullyQualifiedName~CompanyAdministratorsStressTests
--verbosity minimal` detectó los dos casos; este comando no los ejecuta.
Las pruebas de estrés SQL Server no se ejecutaron desde esta sesión, que no hereda
la variable configurada en la terminal del usuario. Ejecutar desde `backend`:

```powershell
dotnet test --filter "FullyQualifiedName~CompanyAdministratorsStressTests"
```

- **B.5.7 — IMPLEMENTADO / PENDIENTE DE VALIDACIÓN SQL SERVER:** `CompanyAdministratorsStressTests.MixedOperations_ShouldPreserveLastAdministrator_UnderRepeatedConcurrentLoad` tiene dos casos: una Company con ocho administradores (ocho operaciones) y cuatro Companies con ocho administradores cada una (32 operaciones). Cada caso ejecuta tres rondas con datos nuevos. Se mezclan desactivación, cambio de rol y eliminación de usuarios distintos, con un scope por operación y señal común de inicio. Se exige un rechazo por el último administrador y siete éxitos por Company; se comprueban estados y roles persistidos desde otro scope, aislamiento del SuperAdministrator de control y adquisición posterior del bloqueo. Solo se admiten errores de validación del último administrador; deadlocks, timeouts y cancelaciones fallan la prueba. La limpieza elimina exclusivamente las Companies y usuarios propios de cada ronda. No se modificaron servicios, reglas ni aislamiento.
- **B.5.8 — PENDIENTE:** refactorización prudente, ejecución de regresión completa y cierre documentado del bloque de concurrencia.
- **SUNAT — ESTADO NO CONFIRMADO:** comprobar si existe y funciona el job asíncrono definido en 40C.
- **Roles — REVISIÓN DE CONSISTENCIA:** confirmar el catálogo exacto de siete roles en código, migraciones y seeder; no añadir nombres por inferencia.
- **Endpoints de administración — INVENTARIO:** extraer rutas, métodos HTTP y contratos directamente de controladores y pruebas actuales.

## 11. Instrucciones operativas para Codex

**Diagnóstico B.5.7 aportado por el usuario (2026-10-04):** en la primera ronda
con cuatro Companies y 32 operaciones se registraron 26 éxitos, un rechazo
esperado por último Administrator, cuatro timeouts de APPLOCK (`-1`) y un
deadlock SQL durante `ChangeRole`. La lectura de diagnóstico falló con SQL 300;
no hay snapshots para identificar las consultas y recursos del ciclo. La
optimización del conteo no resolvió el fallo. B.5.7 continúa pendiente.
Al retomar el paso, recuperar el gráfico desde `system_health` mediante
`support/diagnostics/SqlServerDeadlockDiagnostics.sql`, de solo lectura, usando
una cuenta con acceso existente a diagnósticos. El script filtra los eventos
de `Sicotyc.Stress.` y consulta buffer y archivos retenidos. No se ejecutó
desde Codex; no se cambian timeout, aislamiento ni reglas de negocio en este
subpaso. Si no quedan eventos retenidos al retomarlo, repetir el caso y consultar
enseguida. El usuario indicó que su cuenta no tiene permisos y solicitó aplazar
estas pruebas y el diagnóstico hasta contar con una cuenta autorizada. Se
conservan los archivos; no se deshabilitan tests ni se consideran superados.

1. **Primero inspeccionar el código actual**: no recrear handlers, DTO, repositorios ni servicios que ya existen. Priorizar `AdministratorProtectionService`, `ICompanyAdministrationLock`, `IUnitOfWork`, `IdentityService`, `IIdentityService`, `DeactivateCompanyUsersHandler`, `CompanyAdministratorsConcurrencyTests` y `SqlServerWebApplicationFactory`.
2. Conservar **.NET 10, Clean Architecture, Identity con Guid, SQL Server y handlers sin MediatR**.
3. Mantener el handler de desactivación masiva en `Features.Companies.Commands.DeactivateCompanyUsers`; no reintroducir el namespace duplicado bajo `Features.Users`.
4. Mantener `ReadCommitted` y `sp_getapplock` transaccional por Company salvo que nuevas pruebas y diagnóstico SQL justifiquen un cambio; no «resolver» deadlocks con reintentos automáticos sin analizar la semántica de las operaciones.
5. Toda operación individual que pueda reducir el número de Administrators activos debe comprobar la invariante **dentro del bloqueo**, después de reconsultar al usuario.
6. Respetar la excepción de desactivación masiva exclusiva del SuperAdministrator sobre otra Company, incluida la posibilidad de dejarla sin Administrators activos.
7. Evitar cambios destructivos en bases de datos compartidas; ejecutar las pruebas de SQL Server exclusivamente en la base de integración configurada.
8. No declarar completado B.5.6 hasta contar con evidencia de **rollback real de datos**, cancelación, liberación de bloqueo y recuperación, además de la regresión satisfactoria.
9. Si un contrato, ruta, clase, firma, migración o subpaso no coincide con este documento, tomar el repositorio actual como fuente de verdad, registrar la diferencia y solicitar aclaración cuando afecte reglas de negocio.

## 12. Registro de certeza y limitaciones

Este documento integra el historial recuperable de las conversaciones de implementación, los fragmentos de código compartidos por el usuario y los archivos de handlers y pruebas disponibles en el contexto del proyecto. **No equivale a una auditoría de todos los commits, archivos, migraciones ni resultados de CI.**

Quedan sin verificación documental completa: el desglose exacto de cada subpaso 41 y 42A–42E; el catálogo definitivo de los siete roles; el estado operativo del job SUNAT; las rutas exactas de todos los endpoints de administración; el log de SQL Server de B.5.6 (confirmado por el usuario); y los resultados SQL Server de B.5.7 y de la regresión completa.

**Marcador de reanudación para Codex:** `B.5.7 aplazado por el usuario hasta disponer de permisos de diagnóstico SQL; conservar pruebas y fallos pendientes. B.5.6 confirmado por el usuario; B.5.8 pendiente.`
