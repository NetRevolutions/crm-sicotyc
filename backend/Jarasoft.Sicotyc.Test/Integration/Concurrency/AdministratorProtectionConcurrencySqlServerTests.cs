
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jarasoft.Sicotyc.Test.Integration.Concurrency;

public sealed class AdministratorProtectionConcurrencySqlServerTests
{
    private static SqlServerWebApplicationFactory CreateFactory()
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER " +
                "con la conexión a SQL Server de pruebas.");
        }

        return new SqlServerWebApplicationFactory(
            connectionString);
    }

    private static AdministratorProtectionService CreateService(
        IServiceScope scope)
    {
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var companyLock = scope.ServiceProvider
            .GetRequiredService<ICompanyAdministrationLock>();

        return new AdministratorProtectionService(
            unitOfWork,
            companyLock);
    }

    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ExecuteAsync_ShouldSerializeOperations_ForSameCompany()
    {
        // Arrange
        using var factory = CreateFactory();

        using var firstScope =
            factory.Services.CreateScope();

        using var secondScope =
            factory.Services.CreateScope();

        var firstService = CreateService(firstScope);
        var secondService = CreateService(secondScope);

        var companyId = Guid.NewGuid();

        var firstOperationStarted =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var releaseFirstOperation =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var secondOperationStarted =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        // Act: la primera operación adquiere el bloqueo.
        var firstTask = firstService.ExecuteAsync(
            companyId,
            async _ =>
            {
                firstOperationStarted.SetResult();

                await releaseFirstOperation.Task;
            });

        await firstOperationStarted.Task;

        // La segunda operación intenta adquirir
        // el mismo bloqueo.
        var secondTask = secondService.ExecuteAsync(
            companyId,
            _ =>
            {
                secondOperationStarted.SetResult();

                return Task.CompletedTask;
            });

        try
        {
            // Comprobar que la segunda operación
            // todavía no ha ingresado.
            Assert.False(
                secondOperationStarted.Task.IsCompleted);
        }
        finally
        {
            // Liberar siempre la primera operación,
            // incluso si falla la aserción.
            releaseFirstOperation.TrySetResult();
        }

        await Task.WhenAll(
            firstTask,
            secondTask);

        // Assert
        Assert.True(
            secondOperationStarted.Task
                .IsCompletedSuccessfully);
    }

    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ExecuteAsync_ShouldAllowOperations_ForDifferentCompanies()
    {
        // Arrange
        using var factory = CreateFactory();

        using var firstScope =
            factory.Services.CreateScope();

        using var secondScope =
            factory.Services.CreateScope();

        var firstService = CreateService(firstScope);
        var secondService = CreateService(secondScope);

        var firstCompanyId = Guid.NewGuid();
        var secondCompanyId = Guid.NewGuid();

        var firstOperationStarted =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var releaseFirstOperation =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var secondOperationStarted =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

        // Act
        var firstTask = firstService.ExecuteAsync(
            firstCompanyId,
            async _ =>
            {
                firstOperationStarted.SetResult();

                await releaseFirstOperation.Task;
            });

        await firstOperationStarted.Task;

        try
        {
            // La segunda operación utiliza
            // una Company diferente.
            var secondTask = secondService.ExecuteAsync(
                secondCompanyId,
                _ =>
                {
                    secondOperationStarted.SetResult();

                    return Task.CompletedTask;
                });

            // Debe poder completar su operación
            // sin esperar a que termine la primera.
            await secondOperationStarted.Task
                .WaitAsync(TimeSpan.FromSeconds(5));

            await secondTask;
        }
        finally
        {
            releaseFirstOperation.TrySetResult();
        }

        await firstTask;

        // Assert
        Assert.True(
            secondOperationStarted.Task
                .IsCompletedSuccessfully);
    }

    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ExecuteAsync_ShouldReleaseLock_AfterRollback()
    {
        // Arrange
        using var factory = CreateFactory();

        using var firstScope =
            factory.Services.CreateScope();

        using var secondScope =
            factory.Services.CreateScope();

        var firstService = CreateService(firstScope);
        var secondService = CreateService(secondScope);

        var companyId = Guid.NewGuid();

        // Act: provocar un error dentro
        // de la primera transacción.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => firstService.ExecuteAsync(
                companyId,
                _ => throw new InvalidOperationException(
                    "Error de prueba.")));

        var secondOperationExecuted = false;

        // Una nueva transacción debe poder
        // adquirir el mismo bloqueo.
        await secondService.ExecuteAsync(
            companyId,
            _ =>
            {
                secondOperationExecuted = true;

                return Task.CompletedTask;
            });

        // Assert
        Assert.True(secondOperationExecuted);
    }
}
