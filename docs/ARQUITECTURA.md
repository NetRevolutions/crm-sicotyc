# ARQUITECTURA.md — CRM-Sicotyc

**Fecha de referencia:** 2026-10-03  
**Propósito:** documentar la arquitectura conocida y sus restricciones para mantenimiento y trabajo con Codex.  
**Alcance:** describe decisiones conocidas; los detalles que dependen del árbol Git deben verificarse antes de modificar código.

## 1. Visión general

CRM-Sicotyc es una aplicación web multiempresa. El backend publica una API REST, gestiona usuarios y Companies, aplica autorización por rol y empresa y persiste información en SQL Server. El frontend Angular consume la API. Se sigue **Clean Architecture**, con casos de uso mediante handlers explícitos y **sin MediatR**.

```text
CRM-Sicotyc/
├── AGENTS.md
├── docs/
│   ├── ARQUITECTURA.md
│   └── IDENTITY_IMPLEMENTACION.md
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

## 2. Tecnologías

| Componente | Tecnología/decisión |
|---|---|
| API | .NET 10, ASP.NET Core Web API, C# |
| Casos de uso | Commands y handlers explícitos, sin MediatR |
| ORM | EF Core 10.x |
| Base de datos | SQL Server |
| Consultas especializadas | Dapper, cuando corresponda a código existente |
| Identidad | ASP.NET Core Identity, claves `Guid`, JWT |
| Frontend | Angular, TypeScript, Signals, RxJS y Tailwind según configuración efectiva |
| Tests | xUnit, SQLite in-memory y SQL Server para concurrencia |

La versión exacta de Angular y de cada paquete debe consultarse en `package.json`, `.csproj` y archivos de bloqueo del repositorio.

## 3. Capas y dirección de dependencias

```text
Frontend Angular ─────HTTP/JSON─────> API
                                     │
                                     ▼
                                Application
                                  │     ▲
                                  ▼     │ implementaciones
                                Domain  Infrastructure
                                          │
                                          ▼
                                     SQL Server
```

El diagrama es conceptual: **Application define contratos** que Infrastructure implementa; Domain no debe depender de Infrastructure ni de API.

### 3.1. `Jarasoft.Sicotyc.Domain`

Entidades y reglas de negocio. Se conocen `Company` y `Ubigeo`. Mantener las entidades independientes de HTTP, ASP.NET Core Identity y EF Core siempre que la organización real del proyecto lo permita.

### 3.2. `Jarasoft.Sicotyc.Application`

Casos de uso organizados por funcionalidades (`Features`), comandos, handlers, resultados, DTO, excepciones y abstracciones. Ejemplos conocidos:

- `Features.Users.Commands.ChangeUserStatus.ChangeUserStatusHandler`
- `Features.Users.Commands.ChangeUserRole.ChangeUserRoleHandler`
- `Features.Users.Commands.DeleteUser.DeleteUserHandler`
- `Features.Companies.Commands.DeactivateCompanyUsers.DeactivateCompanyUsersHandler`
- `Features.Users.Services.AdministratorProtectionService`

Los handlers dependen de contratos como `ICurrentUser`, `IIdentityService`, `ICompanyRepository`, `IUnitOfWork` e `ICompanyAdministrationLock`; no deben acoplarse a `UserManager` o `SicotycDbContext`.

### 3.3. `Jarasoft.Sicotyc.Infrastructure`

Implementa acceso a datos, Identity, repositorios y transacciones. Elementos conocidos:

- `SicotycDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`.
- `IdentityService`, basado en `UserManager<ApplicationUser>` para operaciones Identity.
- `CompanyRepository` y `UnitOfWork`.
- Implementación SQL Server de `ICompanyAdministrationLock` mediante `sp_getapplock`.
- Configuraciones EF Core y migraciones.

### 3.4. `Jarasoft.Sicotyc.API`

Configura la composición de dependencias, autenticación, autorización, controladores y manejo de excepciones. Publica contratos HTTP sin exponer entidades de persistencia innecesariamente. Swagger/OpenAPI está habilitado en el backend conocido.

### 3.5. `Jarasoft.Sicotyc.Test`

Incluye pruebas unitarias y de integración. `CustomWebApplicationFactory` utiliza SQLite in-memory para pruebas HTTP y `SqlServerWebApplicationFactory` se utiliza para comprobar transacciones, bloqueo y concurrencia real.

### 3.6. `frontend/crm-sicotyc-web`

Angular con TypeScript y Signals. Se ha trabajado en grillas, filtros, paginación, modales y visibilidad del menú por rol. **La autorización efectiva corresponde al backend**; el frontend no debe tratar la visibilidad de un elemento como permiso de seguridad.

## 4. Modelo multiempresa e Identity

- `Company.Id` es `Guid`; `Company.Ruc` es obligatorio y único.
- `ApplicationUser.CompanyId` es `Guid` y referencia `Companies.Id` con eliminación restrictiva.
- `Company.IdDist` se vincula a `Ubigeos.IDDIST` (`char(6)`, FK restrictiva).
- El registro busca Company por RUC y la reutiliza o crea una nueva si no existe.
- El modelo Company incluye campos de enriquecimiento SUNAT. El job externo para completar esos datos **no está confirmado como implementado**.
- La administración de usuarios está restringida por `CompanyId` y rol. Consultar `docs/IDENTITY_IMPLEMENTACION.md` para la matriz detallada.

## 5. Flujo de un caso de uso protegido

```text
Petición HTTP + JWT
    │
    ▼
Controlador API
    │
    ▼
Command / Handler (Application)
    ├─ Valida autenticación, rol y Company
    ├─ Solicita AdministratorProtectionService.ExecuteAsync(companyId, ...)
    │    ├─ Abre transacción ReadCommitted
    │    ├─ Adquiere sp_getapplock exclusivo por Company
    │    ├─ Reconsulta y revalida datos bajo bloqueo
    │    ├─ Ejecuta IIdentityService / repositorios en el mismo DbContext
    │    ├─ Confirma si todo termina correctamente
    │    └─ Revierte ante excepción o cancelación
    └─ Devuelve resultado
    │
    ▼
Respuesta HTTP
```

El bloqueo utiliza `LockOwner = 'Transaction'`. Operaciones de una misma Company se serializan; operaciones de Companies diferentes no deberían bloquearse entre sí por el bloqueo de aplicación. Se cambió el aislamiento de `Serializable` a `ReadCommitted` tras observar un deadlock SQL Server 1205; el escenario de Companies diferentes pasó cinco veces consecutivas.

En B.5.6 se implementaron pruebas de escrituras Identity, rollback por excepción/cancelación, liberación del bloqueo y recuperación. El usuario confirmó el 2026-10-04 que las cuatro pruebas pasaron sobre SQL Server. Esto cubre los escenarios probados; no implica verificar todas las operaciones Identity posibles.

## 6. Invariantes y seguridad

- Toda operación administrativa comprueba autenticación, rol y pertenencia a Company.
- `Administrator` no administra otras Companies ni modifica `SuperAdministrator`.
- Las operaciones individuales no pueden dejar a una Company sin Administrator activo.
- La desactivación masiva es una excepción explícita: solo `SuperAdministrator` puede desactivar todos los usuarios de otra Company, incluso el último Administrator.
- Un usuario no puede desactivarse a sí mismo; las restricciones de autoeliminación están en los handlers correspondientes.
- Revalidar invariantes dentro de la transacción para prevenir condiciones de carrera.
- Las excepciones se traducen al contrato HTTP existente (`401`, `403`, `404`, `400`, según el caso y endpoint).

## 7. Persistencia, migraciones y configuración

Las entidades Identity usan claves `Guid`. Las migraciones de Companies, Ubigeos e Identity se han trabajado anteriormente, pero el inventario exacto y las migraciones aplicadas deben verificarse en el repositorio y la base de datos antes de generar otras.

Mantener los secretos y cadenas de conexión fuera de Git. No ejecutar operaciones destructivas en bases compartidas. Las pruebas SQLite y SQL Server tienen configuraciones distintas: **no registrar ambos proveedores EF Core en el mismo proveedor interno de servicios**.

## 8. Calidad y pruebas

Comprobar, como mínimo, compilación y pruebas relacionadas con cada cambio. Para modificaciones transaccionales, usar SQL Server real, scopes separados para operaciones concurrentes y datos de prueba aislados. No reutilizar `AuthenticatedIntegrationTestBase.SeedEnvironmentAsync` sobre SQL Server: su limpieza elimina usuarios, Companies y Ubigeos.

Escenarios confirmados de concurrencia B.5.1–B.5.5: dos desactivaciones individuales; desactivación y cambio de rol; eliminación y desactivación; Companies distintas; desactivación masiva frente a individual.

## 9. Decisiones pendientes de comprobación

- Estado efectivo del job de actualización SUNAT.
- Enumeración definitiva de roles en `ApplicationRoles` y `IdentitySeeder`.
- Rutas exactas de todos los endpoints de administración individual.
- Registro y scopes reales de todos los servicios Identity/transaccionales.
- B.5.7 aplazado por el usuario el 2026-10-04 hasta disponer de permisos de diagnóstico SQL. Conservar las pruebas; los timeouts y el deadlock reportados siguen sin resolver. B.5.6 fue confirmado por el usuario.
- B.5.7 (estrés) y B.5.8 (regresión y cierre).

Consultar `docs/IDENTITY_IMPLEMENTACION.md` antes de implementar o modificar cualquiera de estos puntos.
