# AGENTS.md — Instrucciones para Codex en CRM-Sicotyc

> Ámbito: todo el repositorio. Las instrucciones específicas de una carpeta pueden ampliarse mediante otro `AGENTS.md` más cercano, sin contradecir las reglas de seguridad y negocio aquí definidas.

## 1. Objetivo y fuentes de verdad

CRM-Sicotyc es una solución multiempresa con backend .NET y frontend Angular. Codex debe **continuar la implementación existente**, no rediseñarla ni reconstruir componentes ya desarrollados.

Antes de cualquier cambio:
1. Leer este archivo, `docs/ARQUITECTURA.md` y, para autenticación, autorización o usuarios, `docs/IDENTITY_IMPLEMENTACION.md`.
2. Inspeccionar los archivos reales, los contratos, los tests, las migraciones y `git status`. **El repositorio es la fuente de verdad sobre el código actual**; los documentos registran decisiones e historial y pueden requerir actualización.
3. Identificar el paso solicitado, los criterios de aceptación, los archivos afectados y los riesgos de regresión.
4. Si un dato no está confirmado, señalarlo y comprobarlo en el código; no inventar clases, firmas, endpoints, rutas, estados de pruebas ni pasos completados.

## 2. Stack y organización

- Backend: .NET 10, ASP.NET Core Web API, C#, EF Core 10.x, SQL Server, ASP.NET Core Identity, JWT y Dapper donde ya corresponda.
- Arquitectura: Clean Architecture con proyectos `Jarasoft.Sicotyc.API`, `.Application`, `.Domain`, `.Infrastructure` y `.Test`.
- Frontend: `frontend/crm-sicotyc-web`, Angular, TypeScript, Signals, RxJS y Tailwind según la configuración efectiva.
- Tests: xUnit; API/integración con SQLite in-memory y concurrencia transaccional con SQL Server real.
- **No introducir MediatR**: los casos de uso se implementan mediante comandos y handlers explícitos.
- Respetar convenciones, nombres y namespaces existentes; no crear capas, librerías o abstracciones por preferencia personal.

## 3. Límites de Clean Architecture

- **Domain:** entidades y reglas de dominio; sin dependencias de API, EF Core ni Identity.
- **Application:** casos de uso, comandos, handlers, DTO, interfaces, validaciones y reglas de autorización de aplicación; no usar directamente `DbContext` ni `UserManager`.
- **Infrastructure:** implementaciones de persistencia, repositorios, Identity, EF Core, migraciones, transacciones y bloqueo SQL Server.
- **API:** controladores, contratos HTTP, autenticación/autorización, composición de dependencias y manejo de excepciones.
- **Frontend:** presentación, estado y consumo de API. Ocultar opciones por rol no sustituye la autorización del backend.
- Mantener `CancellationToken` en operaciones asíncronas cuando los contratos lo admitan.

## 4. Reglas de negocio Identity que NO deben romperse

1. Todo usuario pertenece a una `Company`; el RUC es obligatorio en el registro. Buscar por RUC y evitar Companies duplicadas.
2. El registro exitoso devuelve `201 Created` con `userId`, `companyId` y `email`.
3. `Administrator` administra usuarios únicamente de su Company; no puede modificar un `SuperAdministrator` ni desactivarse/eliminarse a sí mismo.
4. Las operaciones **individuales** no pueden desactivar, eliminar ni quitar el rol al último `Administrator` activo de una Company.
5. `SuperAdministrator` puede administrar usuarios de otras Companies, pero no desactivarse a sí mismo.
6. Solo `SuperAdministrator` puede desactivar **masivamente** todos los usuarios de una Company distinta de la propia. Esta operación puede desactivar al último `Administrator` de la Company de destino.
7. `Operations`, `Billing` y otros roles no administrativos no pueden ejecutar administración de usuarios.
8. Revalidar pertenencia, permisos e invariantes después de adquirir el bloqueo transaccional. No confiar únicamente en comprobaciones anteriores a la transacción.
9. Verificar los nombres efectivos y las listas de roles en `ApplicationRoles`, `ApplicationRoleRules` e `IdentitySeeder`; no asumir que todos los nombres mencionados históricamente siguen vigentes.

## 5. Concurrencia, transacciones y persistencia

- Usar `AdministratorProtectionService.ExecuteAsync` en operaciones que puedan comprometer la invariante de administradores o concurrir con la desactivación masiva.
- El bloqueo es **por Company** mediante SQL Server `sp_getapplock` con `LockOwner = 'Transaction'`.
- El nivel de aislamiento acordado es **`ReadCommitted`**. No volver a `Serializable` sin justificarlo y repetir las pruebas: se corrigió un deadlock SQL Server 1205 en operaciones de Companies distintas.
- `IUnitOfWork`, repositorios y `UserManager<ApplicationUser>` deben compartir el mismo `SicotycDbContext` y la transacción en el ámbito correspondiente.
- No abrir transacciones, crear `DbContext` o crear scopes independientes dentro de `IdentityService.DeactivateUsersByCompanyAsync`.
- En caso de fallo/cancelación, revertir la transacción; comprobar en la implementación que el rollback no dependa de un token ya cancelado.
- La desactivación masiva definitiva está en `Application.Features.Companies.Commands.DeactivateCompanyUsers`. **No duplicarla** en `Features.Users.Commands.DeactivateCompanyUsers`.
- `AffectedUsers` antes del commit no equivale a cambios persistidos si ocurre rollback.

## 6. Disciplina de implementación

- Trabajar en incrementos pequeños y verificables. Modificar únicamente los archivos necesarios.
- Conservar contratos públicos y respuestas HTTP salvo petición expresa o cambio respaldado por tests.
- Antes de agregar un método a una interfaz, buscar sus implementaciones y consumidores; mantener firmas consistentes.
- No introducir migraciones ni cambios destructivos sin revisar esquema, datos y consecuencias.
- No introducir secretos, contraseñas, tokens ni cadenas de conexión reales en Git.
- No ejecutar limpiezas masivas, `EnsureDeleted`, `RemoveRange` generalizado o reinicios de base contra SQL Server compartido.
- No declarar una prueba como superada sin haberla ejecutado y visto su resultado.
- Al finalizar: resumir cambios, archivos modificados, comandos ejecutados, resultados y pendientes. Actualizar `docs/IDENTITY_IMPLEMENTACION.md` si cambia el estado de un paso.

## 7. Pruebas y comandos de referencia

Desde `backend/`, ajustar rutas/filtros a los nombres reales del repositorio:

```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~CompanyAdministratorsConcurrencyTests"
dotnet test --filter "FullyQualifiedName~AdministratorProtectionRecoveryTests"
dotnet test --filter "Category=SqlServerIntegration"
```

Para las pruebas SQL Server, usar una base **exclusiva de integración** y configurar localmente `SICOTYC_TEST_SQLSERVER`. No guardar credenciales en los documentos. `AuthenticatedIntegrationTestBase.SeedEnvironmentAsync` borra datos para preparar SQLite; **nunca reutilizarlo contra SQL Server**.

Si no existe SQL Server de pruebas o faltan dependencias, explicar la limitación y dejar las pruebas como **no ejecutadas**.

## 8. Estado de continuidad obligatorio

- Último trabajo confirmado: consolidación de `DeactivateCompanyUsersHandler` bajo `Features.Companies`, ajuste de `IdentityService.DeactivateUsersByCompanyAsync` y pruebas anteriores reportadas satisfactorias.
- **Paso activo: `42F.7.6.6.B.5.6`**: rollback real de datos, cancelación y recuperación transaccional.
- Las cuatro pruebas iniciales de recuperación fueron **propuestas, no confirmadas como ejecutadas**; además, las pruebas de excepción/cancelación sin escrituras persistidas **no prueban rollback de datos**.
- Primero inspeccionar si existe `AdministratorProtectionRecoveryTests.cs`. Después crear/adaptar pruebas aisladas que escriban dentro de la transacción, provoquen excepción o cancelación y comprueben los valores persistidos desde otro scope/DbContext.
- Comprobar liberación de bloqueo y reutilización del servicio después del fallo.
- **Pendientes posteriores:** B.5.7 (estrés de concurrencia) y B.5.8 (refactorización, regresión y cierre).
- No marcar B.5.6, B.5.7 o B.5.8 como completados sin resultados verificables.

## 9. Forma de trabajar con el usuario

Responder en español. Presentar un paso a la vez cuando el cambio sea delicado, incluir código compatible con los contratos reales y esperar la confirmación del resultado antes de avanzar al siguiente subpaso.
