using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Test.Integration.Concurrency;

[Trait("Category", "SqlServerIntegration")]
public sealed class AdministratorProtectionRecoveryTests : IAsyncLifetime
{
    private SqlServerWebApplicationFactory? _factory;
    private Guid _companyId;
    private readonly List<Guid> _userIds = [];

    private SqlServerWebApplicationFactory Factory => _factory
        ?? throw new InvalidOperationException("La fábrica de pruebas no está inicializada.");

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("SICOTYC_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER con una base exclusiva de integración.");
        }

        _factory = new SqlServerWebApplicationFactory(connectionString);
        try
        {
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var idDist = await context.Ubigeos.AsNoTracking()
                .Select(x => x.IdDist).FirstOrDefaultAsync()
                ?? throw new InvalidOperationException(
                    "La base de integración debe contener al menos un Ubigeo.");

            var uniqueId = Guid.NewGuid().ToString("N");
            var company = new Company(
                $"20{Random.Shared.Next(100_000_000, 1_000_000_000)}",
                $"Recovery {uniqueId}", "Dirección Test", idDist);
            _companyId = company.Id;
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            for (var index = 0; index < 2; index++)
            {
                var email = $"recovery.{index}.{uniqueId}@test.com";
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    CompanyId = _companyId,
                    FirstName = "Recovery",
                    LastName = "Test",
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true
                };
                _userIds.Add(user.Id);
                var result = await userManager.CreateAsync(user, "Test123!");
                Assert.True(result.Succeeded,
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (_factory is null)
        {
            return;
        }

        try
        {
            if (_companyId != Guid.Empty)
            {
                using var scope = Factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var users = await context.Users
                    .Where(x => x.CompanyId == _companyId).ToListAsync();
                foreach (var user in users)
                {
                    var result = await userManager.DeleteAsync(user);
                    Assert.True(result.Succeeded,
                        string.Join("; ", result.Errors.Select(x => x.Description)));
                }

                // Limpiar exclusivamente la Company creada por esta prueba.
                await context.Companies.Where(x => x.Id == _companyId).ExecuteDeleteAsync();
            }
        }
        finally
        {
            _factory.Dispose();
            _factory = null;
        }
    }

    private async Task DeactivateAndVerifyWritesAsync(
        IServiceScope scope, CancellationToken cancellationToken)
    {
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        Assert.NotNull(context.Database.CurrentTransaction);
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var result = await identity.DeactivateUsersByCompanyAsync(_companyId, cancellationToken);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors));
        Assert.Equal(2, result.AffectedUsers);

        // Consultar SQL, sin usar entidades rastreadas: Identity ya escribió
        // los cambios antes de provocar el fallo de la transacción externa.
        var activeUsers = await context.Users.AsNoTracking()
            .CountAsync(x => x.CompanyId == _companyId && x.IsActive, cancellationToken);
        Assert.Equal(0, activeUsers);
    }

    private async Task AssertPersistedStateAsync(bool isActive, string firstName = "Recovery")
    {
        // Un DbContext nuevo evita confundir el estado del ChangeTracker
        // del intento fallido con los datos realmente persistidos.
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var users = await context.Users.AsNoTracking()
            .Where(x => x.CompanyId == _companyId).OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(_userIds.OrderBy(x => x), users.Select(x => x.Id).OrderBy(x => x));
        Assert.All(users, user =>
        {
            Assert.Equal(isActive, user.IsActive);
            Assert.Equal(firstName, user.FirstName);
        });
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRollback_WhenOperationThrows()
    {
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();
        var failure = new InvalidOperationException("Error después de las escrituras Identity.");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(_companyId, async ct =>
            {
                await DeactivateAndVerifyWritesAsync(scope, ct);
                throw failure;
            }));

        Assert.Same(failure, actual);
        Assert.Null(scope.ServiceProvider.GetRequiredService<SicotycDbContext>()
            .Database.CurrentTransaction);
        await AssertPersistedStateAsync(true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRollback_WhenCancelled()
    {
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();
        using var cancellation = new CancellationTokenSource();

        var actual = await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ExecuteAsync(_companyId, async ct =>
            {
                Assert.Equal(cancellation.Token, ct);
                await DeactivateAndVerifyWritesAsync(scope, ct);
                cancellation.Cancel();
                ct.ThrowIfCancellationRequested();
            }, cancellation.Token));

        Assert.Equal(cancellation.Token, actual.CancellationToken);
        Assert.Null(scope.ServiceProvider.GetRequiredService<SicotycDbContext>()
            .Database.CurrentTransaction);
        await AssertPersistedStateAsync(true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReleaseLock_AfterRollback()
    {
        using var firstScope = Factory.Services.CreateScope();
        var firstContext = firstScope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var firstService = firstScope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();
        await firstContext.Database.OpenConnectionAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => firstService.ExecuteAsync(_companyId, async ct =>
            {
                await DeactivateAndVerifyWritesAsync(firstScope, ct);
                throw new InvalidOperationException("Error de prueba.");
            }));
        await AssertPersistedStateAsync(true);

        // Mantener el primer scope y su conexión abiertos demuestra que
        // el bloqueo fue liberado por el rollback, no por disponerlos.
        using var secondScope = Factory.Services.CreateScope();
        var secondService = secondScope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await secondService.ExecuteAsync(_companyId,
            ct => DeactivateAndVerifyWritesAsync(secondScope, ct), timeout.Token);

        Assert.Null(firstContext.Database.CurrentTransaction);
        await AssertPersistedStateAsync(false);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowNewTransaction_AfterFailure()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(_companyId, async ct =>
            {
                await DeactivateAndVerifyWritesAsync(scope, ct);
                throw new InvalidOperationException("Error de prueba.");
            }));
        Assert.Null(context.Database.CurrentTransaction);
        await AssertPersistedStateAsync(true);

        // Reutilizar servicio, UnitOfWork y DbContext. SQL directo permite
        // verificar su recuperación sin reutilizar entidades rastreadas
        // cuyo estado en memoria no se revierte automáticamente con rollback.
        await service.ExecuteAsync(_companyId, async ct =>
        {
            Assert.NotNull(context.Database.CurrentTransaction);
            var affected = await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE AspNetUsers SET FirstName = {"Recovered"} WHERE CompanyId = {_companyId}", ct);
            Assert.Equal(2, affected);
        });

        Assert.Null(context.Database.CurrentTransaction);
        await AssertPersistedStateAsync(true, "Recovered");
    }
}
