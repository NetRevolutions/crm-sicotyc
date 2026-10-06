using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Exceptions;
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
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace Jarasoft.Sicotyc.Test.Integration.Concurrency;

[Trait("Category", "SqlServerIntegration")]
public sealed class CompanyAdministratorsStressTests(ITestOutputHelper output)
{
    private const int Rounds = 3;

    private enum Operation { Deactivate, ChangeRole, Delete }
    private sealed record Target(Guid CompanyId, Guid UserId, Operation Operation);
    private sealed record Result(Target Target, Exception? Error, TimeSpan Duration);

    [Theory]
    [InlineData(1, 8)]
    [InlineData(4, 8)]
    public async Task MixedOperations_ShouldPreserveLastAdministrator_UnderRepeatedConcurrentLoad(
        int companyCount, int administratorsPerCompany)
    {
        var connectionString = Environment.GetEnvironmentVariable("SICOTYC_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure SICOTYC_TEST_SQLSERVER.");
        }

        // Identificar exclusivamente las sesiones de este caso, sin imprimir
        // la cadena de conexión ni cambiar el comportamiento de los bloqueos.
        var applicationName = $"Sicotyc.Stress.{Guid.NewGuid():N}";
        var testConnection = new SqlConnectionStringBuilder(connectionString)
        {
            ApplicationName = applicationName
        };
        using var factory = new SqlServerWebApplicationFactory(testConnection.ConnectionString);
        for (var round = 0; round < Rounds; round++)
        {
            var companyIds = new List<Guid>();
            try
            {
                using var seedScope = factory.Services.CreateScope();
                var context = seedScope.ServiceProvider.GetRequiredService<SicotycDbContext>();
                var manager = seedScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var idDist = await context.Ubigeos.AsNoTracking()
                    .Select(x => x.IdDist).FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException(
                        "La base de integración debe contener al menos un Ubigeo.");

                var callerCompanyId = await CreateCompanyAsync(context, idDist, companyIds);
                var callerId = await CreateUserAsync(manager, callerCompanyId,
                    ApplicationRoles.SuperAdministrator);
                var targets = new List<Target>();
                for (var company = 0; company < companyCount; company++)
                {
                    var companyId = await CreateCompanyAsync(context, idDist, companyIds);
                    for (var user = 0; user < administratorsPerCompany; user++)
                    {
                        var userId = await CreateUserAsync(manager, companyId,
                            ApplicationRoles.Administrator);
                        targets.Add(new Target(companyId, userId, (Operation)(user % 3)));
                    }
                }

                // Cada operación tiene un scope independiente y espera la misma
                // señal antes de llamar al handler. No se comparte ningún DbContext.
                var start = new TaskCompletionSource(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                using var monitorCancellation = new CancellationTokenSource();
                var snapshots = new ConcurrentQueue<string>();
                var monitor = MonitorSqlAsync(connectionString, applicationName,
                    snapshots, monitorCancellation.Token);
                var tasks = targets.Select(target => ExecuteAsync(factory, target,
                    callerId, callerCompanyId, start.Task, deadline.Token)).ToArray();
                Result[] results;
                try
                {
                    start.SetResult();
                    results = await Task.WhenAll(tasks);
                }
                finally
                {
                    await monitorCancellation.CancelAsync();
                    await monitor;
                    output.WriteLine($"DIAGNOSTICO: ronda={round + 1}, Companies={companyCount}, " +
                        $"operaciones={targets.Count}, threads={ThreadPool.ThreadCount}, " +
                        $"trabajosPendientes={ThreadPool.PendingWorkItemCount}");
                    foreach (var snapshot in snapshots)
                    {
                        output.WriteLine(snapshot);
                    }
                }

                foreach (var result in results)
                {
                    output.WriteLine($"RESULTADO: Company={result.Target.CompanyId}, " +
                        $"User={result.Target.UserId}, operacion={result.Target.Operation}, " +
                        $"duracionMs={result.Duration.TotalMilliseconds:F0}, " +
                        $"estado={result.Error?.GetType().Name ?? "OK"}, " +
                        $"mensaje={result.Error?.Message ?? ""}");
                }

                Assert.All(results, result => Assert.True(
                    result.Error is null || result.Error is ValidationException,
                    $"Company {result.Target.CompanyId}, {result.Target.Operation}, " +
                    $"duracionMs={result.Duration.TotalMilliseconds:F0}: {result.Error}"));

                foreach (var group in results.GroupBy(x => x.Target.CompanyId))
                {
                    Assert.Equal(administratorsPerCompany - 1,
                        group.Count(x => x.Error is null));
                    var rejected = Assert.Single(group, x => x.Error is not null);
                    var error = Assert.IsType<ValidationException>(rejected.Error);
                    var expectedMessage = rejected.Target.Operation switch
                    {
                        Operation.Deactivate =>
                            "La empresa debe conservar al menos un Administrator activo.",
                        Operation.ChangeRole =>
                            "No se puede quitar el rol Administrator al último Administrator activo de la empresa.",
                        Operation.Delete =>
                            "No se puede eliminar al último Administrator activo de la empresa.",
                        _ => throw new InvalidOperationException("Operación de prueba desconocida.")
                    };
                    Assert.Equal(expectedMessage, error.Message);
                }

                await AssertPersistedResultsAsync(factory, results, callerId, callerCompanyId);

                // Una nueva transacción por Company debe adquirir el bloqueo
                // después de todos los commits y del rollback esperado.
                foreach (var companyId in targets.Select(x => x.CompanyId).Distinct())
                {
                    using var probeScope = factory.Services.CreateScope();
                    var protection = probeScope.ServiceProvider
                        .GetRequiredService<AdministratorProtectionService>();
                    using var probeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await protection.ExecuteAsync(companyId, _ => Task.CompletedTask,
                        probeTimeout.Token);
                }
            }
            finally
            {
                // Task.WhenAll se observa antes de limpiar: no quedan operaciones
                // concurrentes usando los registros exclusivos de esta ronda.
                await CleanupAsync(factory, companyIds);
            }
        }
    }

    private static async Task<Result> ExecuteAsync(
        SqlServerWebApplicationFactory factory, Target target,
        Guid callerId, Guid callerCompanyId, Task start, CancellationToken cancellationToken)
    {
        using var scope = factory.Services.CreateScope();
        await start.WaitAsync(cancellationToken);
        var elapsed = Stopwatch.StartNew();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.IsAuthenticated).Returns(true);
        currentUser.Setup(x => x.IsSuperAdministrator).Returns(true);
        currentUser.Setup(x => x.UserId).Returns(callerId);
        currentUser.Setup(x => x.CompanyId).Returns(callerCompanyId);
        currentUser.Setup(x => x.Roles).Returns([ApplicationRoles.SuperAdministrator]);
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var protection = scope.ServiceProvider.GetRequiredService<AdministratorProtectionService>();

        try
        {
            switch (target.Operation)
            {
                case Operation.Deactivate:
                    await new ChangeUserStatusHandler(currentUser.Object, identity, protection)
                        .HandleAsync(new ChangeUserStatusCommand(target.UserId, false), cancellationToken);
                    break;
                case Operation.ChangeRole:
                    await new ChangeUserRoleHandler(currentUser.Object, identity, protection)
                        .HandleAsync(new ChangeUserRoleCommand(target.UserId, ApplicationRoles.Operations),
                            cancellationToken);
                    break;
                case Operation.Delete:
                    await new DeleteUserHandler(currentUser.Object, identity, protection)
                        .HandleAsync(new DeleteUserCommand(target.UserId), cancellationToken);
                    break;
            }

            return new Result(target, null, elapsed.Elapsed);
        }
        catch (Exception error)
        {
            // SQL errors, deadlocks y cancelaciones también se capturan, pero
            // las aserciones solo admiten la validación del último administrador.
            return new Result(target, error, elapsed.Elapsed);
        }
    }

    private static async Task MonitorSqlAsync(string connectionString, string applicationName,
        ConcurrentQueue<string> snapshots, CancellationToken cancellationToken)
    {
        var monitorConnection = new SqlConnectionStringBuilder(connectionString)
        {
            ApplicationName = "Sicotyc.Stress.Monitor",
            Pooling = false,
            ConnectTimeout = 5
        };
        try
        {
            await using var connection = new SqlConnection(monitorConnection.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandTimeout = 3;
                command.CommandText = """
                    SELECT s.session_id, s.status, s.open_transaction_count,
                           COALESCE(r.blocking_session_id, 0), COALESCE(r.wait_type, ''),
                           COALESCE(r.wait_time, 0), COALESCE(r.command, ''),
                           COALESCE(r.wait_resource, ''),
                           COALESCE(SUBSTRING(t.text,
                               COALESCE(r.statement_start_offset, 0) / 2 + 1, 400), '')
                    FROM sys.dm_exec_sessions s
                    LEFT JOIN sys.dm_exec_requests r ON r.session_id = s.session_id
                    OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) t
                    WHERE s.program_name = @ApplicationName
                      AND (r.session_id IS NOT NULL OR s.open_transaction_count > 0)
                    ORDER BY s.session_id;

                    SELECT l.request_session_id, l.resource_type, l.request_mode,
                           l.request_status, COALESCE(l.resource_description, '')
                    FROM sys.dm_tran_locks l
                    JOIN sys.dm_exec_sessions s ON s.session_id = l.request_session_id
                    WHERE s.program_name = @ApplicationName
                      AND l.resource_database_id = DB_ID()
                      AND l.resource_type = 'APPLICATION'
                    ORDER BY l.request_session_id;
                    """;
                command.Parameters.AddWithValue("@ApplicationName", applicationName);
                var snapshot = new StringBuilder($"SQL SNAPSHOT {DateTimeOffset.UtcNow:O}, " +
                    $"threads={ThreadPool.ThreadCount}, pendientes={ThreadPool.PendingWorkItemCount}\n");
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var sessionCount = 0;
                while (await reader.ReadAsync(cancellationToken))
                {
                    sessionCount++;
                    snapshot.AppendLine($"SPID={reader.GetInt16(0)}, estado={reader.GetString(1)}, " +
                        $"transacciones={reader.GetInt32(2)}, bloqueador={Convert.ToInt32(reader.GetValue(3))}, " +
                        $"espera={reader.GetString(4)}, esperaMs={reader.GetInt32(5)}, " +
                        $"comando={reader.GetString(6)}, recurso={reader.GetString(7)}, " +
                        $"SQL={reader.GetString(8).Replace('\r', ' ').Replace('\n', ' ')}");
                }
                await reader.NextResultAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    snapshot.AppendLine($"APPLOCK: SPID={reader.GetInt32(0)}, " +
                        $"tipo={reader.GetString(1)}, modo={reader.GetString(2)}, " +
                        $"estado={reader.GetString(3)}, recurso={reader.GetString(4)}");
                }
                snapshot.AppendLine($"Sesiones activas o con transacción: {sessionCount}");
                if (sessionCount > 0 || snapshots.IsEmpty)
                {
                    snapshots.Enqueue(snapshot.ToString());
                }
                // Limitar la salida a las tres observaciones más recientes.
                while (snapshots.Count > 3)
                {
                    snapshots.TryDequeue(out _);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Detención normal después de observar todas las operaciones.
        }
        catch (SqlException) when (cancellationToken.IsCancellationRequested)
        {
            // La cancelación de un comando diagnóstico también puede llegar
            // como SqlException. No sustituir el resultado de la prueba.
        }
        catch (Exception error)
        {
            // Las DMV pueden requerir permisos que la cuenta de integración
            // no tenga. Registrar la limitación sin alterar los resultados.
            var code = error is SqlException sql ? $", SQL code={sql.Number}" : "";
            snapshots.Enqueue($"DIAGNOSTICO SQL NO DISPONIBLE: {error.GetType().Name}{code}");
        }
    }

    private static async Task AssertPersistedResultsAsync(
        SqlServerWebApplicationFactory factory, Result[] results,
        Guid callerId, Guid callerCompanyId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var companyIds = results.Select(x => x.Target.CompanyId).Distinct().ToArray();
        var users = await context.Users.AsNoTracking()
            .Where(x => companyIds.Contains(x.CompanyId)).ToDictionaryAsync(x => x.Id);
        var administratorRoleId = await context.Roles
            .Where(x => x.Name == ApplicationRoles.Administrator).Select(x => x.Id).SingleAsync();
        var operationsRoleId = await context.Roles
            .Where(x => x.Name == ApplicationRoles.Operations).Select(x => x.Id).SingleAsync();
        var userIds = users.Keys.ToArray();
        var roles = await context.UserRoles.AsNoTracking()
            .Where(x => userIds.Contains(x.UserId)).ToListAsync();

        foreach (var companyId in companyIds)
        {
            Assert.Equal(1, await identity.CountActiveUsersInRoleAsync(
                companyId, ApplicationRoles.Administrator));
        }

        foreach (var result in results)
        {
            var target = result.Target;
            if (result.Error is null && target.Operation == Operation.Delete)
            {
                Assert.False(users.ContainsKey(target.UserId));
                continue;
            }

            Assert.True(users.TryGetValue(target.UserId, out var user));
            Assert.Equal(target.CompanyId, user.CompanyId);
            var remainsAdministrator = result.Error is not null
                || target.Operation == Operation.Deactivate;
            Assert.Equal(remainsAdministrator,
                roles.Any(x => x.UserId == target.UserId && x.RoleId == administratorRoleId));
            Assert.Equal(result.Error is not null || target.Operation == Operation.ChangeRole,
                user.IsActive);
            if (result.Error is null && target.Operation == Operation.ChangeRole)
            {
                Assert.Contains(roles, x => x.UserId == target.UserId && x.RoleId == operationsRoleId);
            }
        }

        var caller = await identity.GetUserByIdAsync(callerId);
        Assert.NotNull(caller);
        Assert.Equal(callerCompanyId, caller.CompanyId);
        Assert.True(caller.IsActive);
        Assert.Contains(ApplicationRoles.SuperAdministrator, caller.Roles);
        Assert.Equal(1, await context.Users.CountAsync(x => x.CompanyId == callerCompanyId));
    }

    private static async Task<Guid> CreateCompanyAsync(
        SicotycDbContext context, string idDist, List<Guid> companyIds)
    {
        var company = new Company($"20{Random.Shared.Next(100_000_000, 1_000_000_000)}",
            $"Stress {Guid.NewGuid():N}", "Dirección Test", idDist);
        companyIds.Add(company.Id);
        context.Companies.Add(company);
        await context.SaveChangesAsync();
        return company.Id;
    }

    private static async Task<Guid> CreateUserAsync(
        UserManager<ApplicationUser> manager, Guid companyId, string role)
    {
        var email = $"stress.{Guid.NewGuid():N}@test.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), CompanyId = companyId,
            FirstName = "Stress", LastName = "Test",
            UserName = email, Email = email, EmailConfirmed = true, IsActive = true
        };
        var created = await manager.CreateAsync(user, "Test123!");
        Assert.True(created.Succeeded, string.Join("; ", created.Errors.Select(x => x.Description)));
        var assigned = await manager.AddToRoleAsync(user, role);
        Assert.True(assigned.Succeeded, string.Join("; ", assigned.Errors.Select(x => x.Description)));
        return user.Id;
    }

    private static async Task CleanupAsync(SqlServerWebApplicationFactory factory, List<Guid> companyIds)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var users = await context.Users.Where(x => companyIds.Contains(x.CompanyId)).ToListAsync();
        foreach (var user in users)
        {
            var deleted = await manager.DeleteAsync(user);
            Assert.True(deleted.Succeeded, string.Join("; ", deleted.Errors.Select(x => x.Description)));
        }

        await context.Companies.Where(x => companyIds.Contains(x.Id)).ExecuteDeleteAsync();
    }
}
