using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.DeleteUser;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Jarasoft.Sicotyc.Test.Integration.Concurrency;

public sealed class CompanyAdministratorsConcurrencyTests
{
    #region Tests


    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task MassDeactivationAndIndividualDeactivation_ShouldLeaveAllCompanyUsersInactive_WhenConcurrent()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        Guid companyAId = Guid.Empty;
        Guid companyBId = Guid.Empty;

        Guid superAdministratorId = Guid.Empty;
        Guid administratorB1Id = Guid.Empty;
        Guid administratorB2Id = Guid.Empty;

        try
        {
            // 1. Preparar los datos.
            using (var seedScope =
                factory.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

                var userManager = seedScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var idDist = await context.Ubigeos
                    .AsNoTracking()
                    .Select(x => x.IdDist)
                    .FirstOrDefaultAsync();

                if (idDist is null)
                {
                    throw new InvalidOperationException(
                        "La base de integración debe contener " +
                        "al menos un Ubigeo.");
                }

                var uniqueId = Guid.NewGuid()
                    .ToString("N");

                var companyA = new Company(
                    CreateTestRuc(),
                    $"Concurrency A {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyB = new Company(
                    CreateTestRuc(),
                    $"Concurrency B {uniqueId}",
                    "Dirección Test",
                    idDist);

                context.Companies.AddRange(
                    companyA,
                    companyB);

                await context.SaveChangesAsync();

                companyAId = companyA.Id;
                companyBId = companyB.Id;

                // SuperAdministrator externo.
                superAdministratorId =
                    await CreateUserAsync(
                        userManager,
                        companyAId,
                        $"super.{uniqueId}@test.com",
                        ApplicationRoles.SuperAdministrator);

                // Company B: dos administradores.
                administratorB1Id =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b1.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorB2Id =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b2.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);
            }

            // 2. Crear scopes independientes.
            using var massScope =
                factory.Services.CreateScope();

            using var individualScope =
                factory.Services.CreateScope();

            var massHandler =
                CreateDeactivateCompanyUsersHandler(
                    massScope,
                    superAdministratorId,
                    companyAId);

            var individualHandler =
                CreateHandler(
                    individualScope,
                    superAdministratorId,
                    companyAId);

            // 3. Ejecutar ambas operaciones.
            var massTask =
                ExecuteMassDeactivationAsync(
                    massHandler,
                    companyBId);

            var individualTask =
                ExecuteAsync(
                    individualHandler,
                    administratorB1Id);

            var results = await Task.WhenAll(
                    massTask,
                    individualTask)
                .WaitAsync(
                    TimeSpan.FromSeconds(30));

            // 4. La operación masiva
            //    debe completarse.
            Assert.Null(results[0]);

            // La operación individual puede
            // completarse o encontrar una
            // condición de negocio.
            if (results[1] is not null)
            {
                Assert.IsType<ValidationException>(
                    results[1]);
            }

            // 5. Verificar el estado persistido.
            using var verificationScope =
                factory.Services.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            var administratorB1 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorB1Id.ToString());

            var administratorB2 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorB2Id.ToString());

            Assert.NotNull(administratorB1);
            Assert.NotNull(administratorB2);

            Assert.False(
                administratorB1!.IsActive);

            Assert.False(
                administratorB2!.IsActive);

            // No debe quedar ningún
            // usuario activo en Company B.
            var activeUsers =
                await verificationContext.Users
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.CompanyId == companyBId &&
                        x.IsActive);

            Assert.Equal(
                0,
                activeUsers);

            // El SuperAdministrator externo
            // no debe resultar afectado.
            var superAdministrator =
                await verificationUserManager
                    .FindByIdAsync(
                        superAdministratorId.ToString());

            Assert.NotNull(
                superAdministrator);

            Assert.True(
                superAdministrator!.IsActive);
        }
        finally
        {
            // 6. Limpiar únicamente los
            //    datos de esta prueba.
            using var cleanupScope =
                factory.Services.CreateScope();

            var context = cleanupScope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

            var userManager = cleanupScope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

            foreach (var userId in new[]
            {
            administratorB1Id,
            administratorB2Id,
            superAdministratorId
        })
            {
                if (userId == Guid.Empty)
                {
                    continue;
                }

                var user = await userManager
                    .FindByIdAsync(
                        userId.ToString());

                if (user is not null)
                {
                    var deleteResult =
                        await userManager.DeleteAsync(
                            user);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }

            foreach (var companyId in new[]
            {
            companyAId,
            companyBId
        })
            {
                if (companyId == Guid.Empty)
                {
                    continue;
                }

                var company =
                    await context.Companies
                        .FindAsync(companyId);

                if (company is not null)
                {
                    context.Companies.Remove(
                        company);
                }
            }

            await context.SaveChangesAsync();
        }
    }


    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ChangeUserStatus_ShouldAllowConcurrentOperations_ForDifferentCompanies()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        Guid companyAId = Guid.Empty;
        Guid companyBId = Guid.Empty;
        Guid companyCId = Guid.Empty;

        Guid superAdministratorId = Guid.Empty;

        Guid administratorB1Id = Guid.Empty;
        Guid administratorB2Id = Guid.Empty;

        Guid administratorC1Id = Guid.Empty;
        Guid administratorC2Id = Guid.Empty;

        try
        {
            // 1. Crear tres Companies independientes.
            using (var seedScope =
                factory.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

                var userManager = seedScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var idDist = await context.Ubigeos
                    .AsNoTracking()
                    .Select(x => x.IdDist)
                    .FirstOrDefaultAsync();

                if (idDist is null)
                {
                    throw new InvalidOperationException(
                        "La base de integración debe contener " +
                        "al menos un Ubigeo.");
                }

                var uniqueId = Guid.NewGuid()
                    .ToString("N");

                var companyA = new Company(
                    CreateTestRuc(),
                    $"Concurrency A {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyB = new Company(
                    CreateTestRuc(),
                    $"Concurrency B {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyC = new Company(
                    CreateTestRuc(),
                    $"Concurrency C {uniqueId}",
                    "Dirección Test",
                    idDist);

                context.Companies.AddRange(
                    companyA,
                    companyB,
                    companyC);

                await context.SaveChangesAsync();

                companyAId = companyA.Id;
                companyBId = companyB.Id;
                companyCId = companyC.Id;

                // El solicitante pertenece a Company A.
                superAdministratorId =
                    await CreateUserAsync(
                        userManager,
                        companyAId,
                        $"super.{uniqueId}@test.com",
                        ApplicationRoles.SuperAdministrator);

                // Company B: dos administradores activos.
                administratorB1Id =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b1.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorB2Id =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b2.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                // Company C: dos administradores activos.
                administratorC1Id =
                    await CreateUserAsync(
                        userManager,
                        companyCId,
                        $"admin.c1.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorC2Id =
                    await CreateUserAsync(
                        userManager,
                        companyCId,
                        $"admin.c2.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);
            }

            // 2. Crear scopes independientes.
            using var companyBScope =
                factory.Services.CreateScope();

            using var companyCScope =
                factory.Services.CreateScope();

            var companyBHandler = CreateHandler(
                companyBScope,
                superAdministratorId,
                companyAId);

            var companyCHandler = CreateHandler(
                companyCScope,
                superAdministratorId,
                companyAId);

            // 3. Ejecutar las desactivaciones
            //    de forma concurrente.
            var companyBTask = ExecuteAsync(
                companyBHandler,
                administratorB1Id);

            var companyCTask = ExecuteAsync(
                companyCHandler,
                administratorC1Id);

            var results = await Task.WhenAll(
                    companyBTask,
                    companyCTask)
                .WaitAsync(TimeSpan.FromSeconds(30));

            // 4. Ambas operaciones deben completarse.
            var unexpectedFailures = results
                .Where(x => x is not null)
                .ToArray();

            Assert.True(
                unexpectedFailures.Length == 0,
                "Se encontraron excepciones: " +
                string.Join(
                    Environment.NewLine,
                    unexpectedFailures.Select(
                        x => x!.ToString())));

            // 5. Consultar el estado final
            //    desde un tercer DbContext.
            using var verificationScope =
                factory.Services.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            // Company B.
            var companyBUsers =
                await verificationContext.Users
                    .AsNoTracking()
                    .Where(x =>
                        x.CompanyId == companyBId &&
                        x.IsActive)
                    .ToListAsync();

            var companyBActiveAdministrators = 0;

            foreach (var user in companyBUsers)
            {
                if (await verificationUserManager
                    .IsInRoleAsync(
                        user,
                        ApplicationRoles.Administrator))
                {
                    companyBActiveAdministrators++;
                }
            }

            // Company C.
            var companyCUsers =
                await verificationContext.Users
                    .AsNoTracking()
                    .Where(x =>
                        x.CompanyId == companyCId &&
                        x.IsActive)
                    .ToListAsync();

            var companyCActiveAdministrators = 0;

            foreach (var user in companyCUsers)
            {
                if (await verificationUserManager
                    .IsInRoleAsync(
                        user,
                        ApplicationRoles.Administrator))
                {
                    companyCActiveAdministrators++;
                }
            }

            // 6. Cada Company debe conservar
            //    exactamente un Administrator activo.
            Assert.Equal(
                1,
                companyBActiveAdministrators);

            Assert.Equal(
                1,
                companyCActiveAdministrators);

            // 7. Verificar los estados individuales.
            var administratorB1 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorB1Id.ToString());

            var administratorB2 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorB2Id.ToString());

            var administratorC1 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorC1Id.ToString());

            var administratorC2 =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorC2Id.ToString());

            Assert.NotNull(administratorB1);
            Assert.NotNull(administratorB2);
            Assert.NotNull(administratorC1);
            Assert.NotNull(administratorC2);

            Assert.False(administratorB1!.IsActive);
            Assert.True(administratorB2!.IsActive);

            Assert.False(administratorC1!.IsActive);
            Assert.True(administratorC2!.IsActive);
        }
        finally
        {
            // 8. Eliminar únicamente los datos
            //    creados por esta prueba.
            using var cleanupScope =
                factory.Services.CreateScope();

            var context = cleanupScope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

            var userManager = cleanupScope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

            foreach (var userId in new[]
            {
            administratorB1Id,
            administratorB2Id,
            administratorC1Id,
            administratorC2Id,
            superAdministratorId
        })
            {
                if (userId == Guid.Empty)
                {
                    continue;
                }

                var user = await userManager
                    .FindByIdAsync(
                        userId.ToString());

                if (user is not null)
                {
                    var deleteResult =
                        await userManager.DeleteAsync(user);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }

            foreach (var companyId in new[]
            {
            companyAId,
            companyBId,
            companyCId
        })
            {
                if (companyId == Guid.Empty)
                {
                    continue;
                }

                var company = await context.Companies
                    .FindAsync(companyId);

                if (company is not null)
                {
                    context.Companies.Remove(company);
                }
            }

            await context.SaveChangesAsync();
        }
    }


    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task DeleteAndDeactivate_ShouldPreserveLastAdministrator_WhenConcurrent()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        Guid companyAId = Guid.Empty;
        Guid companyBId = Guid.Empty;

        Guid superAdministratorId = Guid.Empty;
        Guid administratorAId = Guid.Empty;
        Guid administratorBId = Guid.Empty;

        try
        {
            // 1. Crear los datos exclusivos
            //    de esta prueba.
            using (var seedScope =
                factory.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

                var userManager = seedScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var idDist = await context.Ubigeos
                    .AsNoTracking()
                    .Select(x => x.IdDist)
                    .FirstOrDefaultAsync();

                if (idDist is null)
                {
                    throw new InvalidOperationException(
                        "La base de integración debe contener " +
                        "al menos un Ubigeo.");
                }

                var uniqueId = Guid.NewGuid()
                    .ToString("N");

                var companyA = new Company(
                    CreateTestRuc(),
                    $"Concurrency A {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyB = new Company(
                    CreateTestRuc(),
                    $"Concurrency B {uniqueId}",
                    "Dirección Test",
                    idDist);

                context.Companies.AddRange(
                    companyA,
                    companyB);

                await context.SaveChangesAsync();

                companyAId = companyA.Id;
                companyBId = companyB.Id;

                // SuperAdministrator externo:
                // pertenece a Company A.
                superAdministratorId =
                    await CreateUserAsync(
                        userManager,
                        companyAId,
                        $"super.{uniqueId}@test.com",
                        ApplicationRoles.SuperAdministrator);

                // Los dos administradores
                // pertenecen a Company B.
                administratorAId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.a.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorBId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);
            }

            // 2. Crear scopes independientes.
            using var deleteScope =
                factory.Services.CreateScope();

            using var statusScope =
                factory.Services.CreateScope();

            // 3. Preparar los handlers.
            var deleteHandler = CreateDeleteHandler(
                deleteScope,
                superAdministratorId,
                companyAId);

            var statusHandler = CreateHandler(
                statusScope,
                superAdministratorId,
                companyAId);

            // 4. Ejecutar las dos operaciones
            //    concurrentemente.
            var deleteTask = ExecuteDeleteAsync(
                deleteHandler,
                administratorAId);

            var statusTask = ExecuteAsync(
                statusHandler,
                administratorBId);

            var results = await Task.WhenAll(
                    deleteTask,
                    statusTask)
                .WaitAsync(TimeSpan.FromSeconds(30));

            // 5. Verificar los resultados.
            var successfulOperations =
                results.Count(x => x is null);

            var validationFailures =
                results.Count(x =>
                    x is ValidationException);

            var unexpectedFailures = results
                .Where(x =>
                    x is not null &&
                    x is not ValidationException)
                .ToArray();

            Assert.True(
                unexpectedFailures.Length == 0,
                "Se encontraron excepciones inesperadas: " +
                string.Join(
                    Environment.NewLine,
                    unexpectedFailures.Select(
                        x => x!.ToString())));

            Assert.Equal(
                1,
                successfulOperations);

            Assert.Equal(
                1,
                validationFailures);

            // 6. Verificar el estado final
            //    desde un tercer DbContext.
            using var verificationScope =
                factory.Services.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            var users = await verificationContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyBId &&
                    x.IsActive)
                .ToListAsync();

            var activeAdministrators = 0;

            foreach (var user in users)
            {
                if (await verificationUserManager
                    .IsInRoleAsync(
                        user,
                        ApplicationRoles.Administrator))
                {
                    activeAdministrators++;
                }
            }

            Assert.Equal(
                1,
                activeAdministrators);

            // 7. Verificar también el resultado
            //    específico de cada operación.
            var administratorA =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorAId.ToString());

            var administratorB =
                await verificationUserManager
                    .FindByIdAsync(
                        administratorBId.ToString());

            if (results[0] is null)
            {
                // La eliminación se completó.
                Assert.Null(administratorA);

                // La desactivación fue rechazada.
                Assert.NotNull(administratorB);
                Assert.True(administratorB!.IsActive);
            }
            else
            {
                // La eliminación fue rechazada.
                Assert.NotNull(administratorA);
                Assert.True(administratorA!.IsActive);

                // La desactivación se completó.
                Assert.NotNull(administratorB);
                Assert.False(administratorB!.IsActive);
            }
        }
        finally
        {
            // 8. Eliminar únicamente los datos
            //    creados por esta prueba.
            using var cleanupScope =
                factory.Services.CreateScope();

            var context = cleanupScope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

            var userManager = cleanupScope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

            foreach (var userId in new[]
            {
            administratorAId,
            administratorBId,
            superAdministratorId
        })
            {
                if (userId == Guid.Empty)
                {
                    continue;
                }

                var user = await userManager
                    .FindByIdAsync(
                        userId.ToString());

                if (user is not null)
                {
                    var deleteResult =
                        await userManager.DeleteAsync(user);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }

            foreach (var companyId in new[]
            {
            companyAId,
            companyBId
        })
            {
                if (companyId == Guid.Empty)
                {
                    continue;
                }

                var company = await context.Companies
                    .FindAsync(companyId);

                if (company is not null)
                {
                    context.Companies.Remove(company);
                }
            }

            await context.SaveChangesAsync();
        }
    }


    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ChangeStatusAndRole_ShouldPreserveLastAdministrator_WhenConcurrent()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        Guid companyAId = Guid.Empty;
        Guid companyBId = Guid.Empty;

        Guid superAdministratorId = Guid.Empty;
        Guid administratorAId = Guid.Empty;
        Guid administratorBId = Guid.Empty;

        try
        {
            using (var seedScope =
                factory.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

                var userManager = seedScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var idDist = await context.Ubigeos
                    .AsNoTracking()
                    .Select(x => x.IdDist)
                    .FirstOrDefaultAsync();

                if (idDist is null)
                {
                    throw new InvalidOperationException(
                        "La base de integración debe contener " +
                        "al menos un Ubigeo.");
                }

                var uniqueId = Guid.NewGuid()
                    .ToString("N");

                var companyA = new Company(
                    CreateTestRuc(),
                    $"Concurrency A {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyB = new Company(
                    CreateTestRuc(),
                    $"Concurrency B {uniqueId}",
                    "Dirección Test",
                    idDist);

                context.Companies.AddRange(
                    companyA,
                    companyB);

                await context.SaveChangesAsync();

                companyAId = companyA.Id;
                companyBId = companyB.Id;

                superAdministratorId =
                    await CreateUserAsync(
                        userManager,
                        companyAId,
                        $"super.{uniqueId}@test.com",
                        ApplicationRoles.SuperAdministrator);

                administratorAId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.a.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorBId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);
            }

            // Cada handler utiliza su propio scope,
            // DbContext y transacción.
            using var statusScope =
                factory.Services.CreateScope();

            using var roleScope =
                factory.Services.CreateScope();

            var statusHandler = CreateHandler(
                statusScope,
                superAdministratorId,
                companyAId);

            var roleHandler = CreateRoleHandler(
                roleScope,
                superAdministratorId,
                companyAId);

            // Act: ejecutar las dos operaciones
            // de forma concurrente.
            var statusTask = ExecuteAsync(
                statusHandler,
                administratorAId);

            var roleTask = ExecuteRoleAsync(
                roleHandler,
                administratorBId);

            var results = await Task.WhenAll(
                    statusTask,
                    roleTask)
                .WaitAsync(TimeSpan.FromSeconds(30));

            // Assert: una operación debe completarse
            // y la otra debe ser rechazada.
            var successfulOperations =
                results.Count(x => x is null);

            var validationFailures =
                results.Count(x =>
                    x is ValidationException);

            var unexpectedFailures = results
                .Where(x =>
                    x is not null &&
                    x is not ValidationException)
                .ToArray();

            Assert.True(
                unexpectedFailures.Length == 0,
                "Se encontraron excepciones inesperadas: " +
                string.Join(
                    Environment.NewLine,
                    unexpectedFailures.Select(
                        x => x!.ToString())));

            Assert.Equal(
                1,
                successfulOperations);

            Assert.Equal(
                1,
                validationFailures);

            // Verificar el estado real persistido
            // utilizando un tercer scope.
            using var verificationScope =
                factory.Services.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            var users = await verificationContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyBId &&
                    x.IsActive)
                .ToListAsync();

            var activeAdministrators = 0;

            foreach (var user in users)
            {
                if (await verificationUserManager
                    .IsInRoleAsync(
                        user,
                        ApplicationRoles.Administrator))
                {
                    activeAdministrators++;
                }
            }

            Assert.Equal(
                1,
                activeAdministrators);
        }
        finally
        {
            using var cleanupScope =
                factory.Services.CreateScope();

            var context = cleanupScope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

            var userManager = cleanupScope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

            foreach (var userId in new[]
            {
            administratorAId,
            administratorBId,
            superAdministratorId
        })
            {
                if (userId == Guid.Empty)
                {
                    continue;
                }

                var user = await userManager
                    .FindByIdAsync(
                        userId.ToString());

                if (user is not null)
                {
                    var deleteResult =
                        await userManager.DeleteAsync(user);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }

            foreach (var companyId in new[]
            {
            companyAId,
            companyBId
        })
            {
                if (companyId == Guid.Empty)
                {
                    continue;
                }

                var company = await context.Companies
                    .FindAsync(companyId);

                if (company is not null)
                {
                    context.Companies.Remove(company);
                }
            }

            await context.SaveChangesAsync();
        }
    }


    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ChangeUserStatus_ShouldPreserveLastAdministrator_WhenTwoDeactivationsAreConcurrent()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        Guid companyAId = Guid.Empty;
        Guid companyBId = Guid.Empty;

        Guid superAdministratorId = Guid.Empty;
        Guid administratorAId = Guid.Empty;
        Guid administratorBId = Guid.Empty;

        try
        {
            // Crear datos exclusivos de esta prueba.
            using (var seedScope =
                factory.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

                var userManager = seedScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var idDist = await context.Ubigeos
                    .AsNoTracking()
                    .Select(x => x.IdDist)
                    .FirstOrDefaultAsync();

                if (idDist is null)
                {
                    throw new InvalidOperationException(
                        "La base de integración debe contener " +
                        "al menos un Ubigeo.");
                }

                var uniqueId = Guid.NewGuid()
                    .ToString("N");

                var companyA = new Company(
                    CreateTestRuc(),
                    $"Concurrency A {uniqueId}",
                    "Dirección Test",
                    idDist);

                var companyB = new Company(
                    CreateTestRuc(),
                    $"Concurrency B {uniqueId}",
                    "Dirección Test",
                    idDist);

                context.Companies.AddRange(
                    companyA,
                    companyB);

                await context.SaveChangesAsync();

                companyAId = companyA.Id;
                companyBId = companyB.Id;

                superAdministratorId =
                    await CreateUserAsync(
                        userManager,
                        companyAId,
                        $"super.{uniqueId}@test.com",
                        ApplicationRoles.SuperAdministrator);

                administratorAId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.a.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);

                administratorBId =
                    await CreateUserAsync(
                        userManager,
                        companyBId,
                        $"admin.b.{uniqueId}@test.com",
                        ApplicationRoles.Administrator);
            }

            // Dos scopes: dos DbContext y
            // dos transacciones independientes.
            using var firstScope =
                factory.Services.CreateScope();

            using var secondScope =
                factory.Services.CreateScope();

            var firstHandler = CreateHandler(
                firstScope,
                superAdministratorId,
                companyAId);

            var secondHandler = CreateHandler(
                secondScope,
                superAdministratorId,
                companyAId);

            // Act
            var firstTask = ExecuteAsync(
                firstHandler,
                administratorAId);

            var secondTask = ExecuteAsync(
                secondHandler,
                administratorBId);

            var results = await Task.WhenAll(
                    firstTask,
                    secondTask)
                .WaitAsync(TimeSpan.FromSeconds(30));

            // Assert: una operación debe tener éxito
            // y la otra debe rechazarse.
            var successfulOperations =
                results.Count(x => x is null);

            var validationFailures =
                results.Count(x =>
                    x is Jarasoft.Sicotyc.Application.Exceptions
                        .ValidationException);

            var unexpectedFailures =
                results
                    .Where(x =>
                        x is not null &&
                        x is not Jarasoft.Sicotyc.Application.Exceptions
                            .ValidationException)
                    .ToArray();

            Assert.True(
                unexpectedFailures.Length == 0,
                "Se encontraron excepciones inesperadas: " +
                string.Join(
                    Environment.NewLine,
                    unexpectedFailures.Select(
                        x => x!.ToString())));

            Assert.Equal(
                1,
                successfulOperations);

            Assert.Equal(
                1,
                validationFailures);

            // Consultar el estado final con un
            // tercer DbContext independiente.
            using var verificationScope =
                factory.Services.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            var users = await verificationContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyBId &&
                    x.IsActive)
                .ToListAsync();

            var activeAdministrators = 0;

            foreach (var user in users)
            {
                if (await verificationUserManager
                    .IsInRoleAsync(
                        user,
                        ApplicationRoles.Administrator))
                {
                    activeAdministrators++;
                }
            }

            Assert.Equal(
                1,
                activeAdministrators);
        }
        finally
        {
            // Limpiar únicamente los registros
            // creados por esta prueba.
            using var cleanupScope =
                factory.Services.CreateScope();

            var context = cleanupScope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

            var userManager = cleanupScope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

            foreach (var userId in new[]
            {
                administratorAId,
                administratorBId,
                superAdministratorId
            })
            {
                if (userId == Guid.Empty)
                {
                    continue;
                }

                var user = await userManager
                    .FindByIdAsync(
                        userId.ToString());

                if (user is not null)
                {
                    var deleteResult =
                        await userManager.DeleteAsync(user);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }

            foreach (var companyId in new[]
            {
                companyAId,
                companyBId
            })
            {
                if (companyId == Guid.Empty)
                {
                    continue;
                }

                var company = await context.Companies
                    .FindAsync(companyId);

                if (company is not null)
                {
                    context.Companies.Remove(company);
                }
            }

            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Private methods
    private static async Task<Exception?>
        ExecuteMassDeactivationAsync(
            DeactivateCompanyUsersHandler handler,
            Guid companyId)
    {
        try
        {
            await handler.HandleAsync(
                new DeactivateCompanyUsersCommand(
                    companyId));

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static DeactivateCompanyUsersHandler
        CreateDeactivateCompanyUsersHandler(
            IServiceScope scope,
            Guid callerId,
            Guid callerCompanyId)
    {
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        currentUser
            .Setup(x => x.IsSuperAdministrator)
            .Returns(true);

        currentUser
            .Setup(x => x.Roles)
            .Returns(new[]
            {
            ApplicationRoles.SuperAdministrator
            });

        currentUser
            .Setup(x => x.UserId)
            .Returns(callerId);

        currentUser
            .Setup(x => x.CompanyId)
            .Returns(callerCompanyId);

        var identityService = scope.ServiceProvider
            .GetRequiredService<IIdentityService>();

        var companyRepository = scope.ServiceProvider
            .GetRequiredService<ICompanyRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var companyLock = scope.ServiceProvider
            .GetRequiredService<ICompanyAdministrationLock>();

        var protectionService =
            new AdministratorProtectionService(
                unitOfWork,
                companyLock);

        return new DeactivateCompanyUsersHandler(
            currentUser.Object,
            identityService,
            companyRepository,
            protectionService);
    }

    private static async Task<Exception?> ExecuteDeleteAsync(
        DeleteUserHandler handler,
        Guid userId)
    {
        try
        {
            await handler.HandleAsync(
                new DeleteUserCommand(userId));

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }


    private static DeleteUserHandler CreateDeleteHandler(
        IServiceScope scope,
        Guid callerId,
        Guid callerCompanyId)
    {
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        currentUser
            .Setup(x => x.IsSuperAdministrator)
            .Returns(true);

        currentUser
            .Setup(x => x.Roles)
            .Returns(new[]
            {
            ApplicationRoles.SuperAdministrator
            });

        currentUser
            .Setup(x => x.UserId)
            .Returns(callerId);

        currentUser
            .Setup(x => x.CompanyId)
            .Returns(callerCompanyId);

        var identityService = scope.ServiceProvider
            .GetRequiredService<IIdentityService>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var companyLock = scope.ServiceProvider
            .GetRequiredService<ICompanyAdministrationLock>();

        var protectionService =
            new AdministratorProtectionService(
                unitOfWork,
                companyLock);

        return new DeleteUserHandler(
            currentUser.Object,
            identityService,
            protectionService);
    }

    private static async Task<Exception?> ExecuteRoleAsync(
        ChangeUserRoleHandler handler,
        Guid userId)
    {
        try
        {
            await handler.HandleAsync(
                new ChangeUserRoleCommand(
                    userId,
                    ApplicationRoles.Operations));

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }


    private static ChangeUserRoleHandler CreateRoleHandler(
        IServiceScope scope,
        Guid callerId,
        Guid callerCompanyId)
    {
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        currentUser
            .Setup(x => x.IsSuperAdministrator)
            .Returns(true);

        currentUser
            .Setup(x => x.Roles)
            .Returns(new[]
            {
            ApplicationRoles.SuperAdministrator
            });

        currentUser
            .Setup(x => x.UserId)
            .Returns(callerId);

        currentUser
            .Setup(x => x.CompanyId)
            .Returns(callerCompanyId);

        var identityService = scope.ServiceProvider
            .GetRequiredService<IIdentityService>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var companyLock = scope.ServiceProvider
            .GetRequiredService<ICompanyAdministrationLock>();

        var protectionService =
            new AdministratorProtectionService(
                unitOfWork,
                companyLock);

        return new ChangeUserRoleHandler(
            currentUser.Object,
            identityService,
            protectionService);
    }

    private static ChangeUserStatusHandler CreateHandler(
    IServiceScope scope,
    Guid callerId,
    Guid callerCompanyId)
    {
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        currentUser
            .Setup(x => x.IsSuperAdministrator)
            .Returns(true);

        // CORRECCIÓN: configurar los roles del solicitante.
        currentUser
            .Setup(x => x.Roles)
            .Returns(new[]
            {
            ApplicationRoles.SuperAdministrator
            });

        currentUser
            .Setup(x => x.UserId)
            .Returns(callerId);

        currentUser
            .Setup(x => x.CompanyId)
            .Returns(callerCompanyId);

        var identityService = scope.ServiceProvider
            .GetRequiredService<IIdentityService>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var companyLock = scope.ServiceProvider
            .GetRequiredService<ICompanyAdministrationLock>();

        var protectionService =
            new AdministratorProtectionService(
                unitOfWork,
                companyLock);

        return new ChangeUserStatusHandler(
            currentUser.Object,
            identityService,
            protectionService);
    }

    private static async Task<Exception?> ExecuteAsync(
        ChangeUserStatusHandler handler,
        Guid userId)
    {
        try
        {
            await handler.HandleAsync(
                new ChangeUserStatusCommand(
                    userId,
                    false));

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static async Task<Guid> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        Guid companyId,
        string email,
        string role)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            FirstName = "Concurrency",
            LastName = "Test",
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager
            .CreateAsync(
                user,
                "Test123!");

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    createResult.Errors.Select(
                        x => x.Description)));
        }

        var roleResult = await userManager
            .AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    roleResult.Errors.Select(
                        x => x.Description)));
        }

        return user.Id;
    }

    private static string CreateTestRuc()
    {
        var number = Random.Shared.Next(
            100_000_000,
            1_000_000_000);

        return $"20{number}";
    }
    #endregion
}
