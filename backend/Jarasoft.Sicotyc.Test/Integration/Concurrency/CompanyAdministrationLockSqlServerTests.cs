using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jarasoft.Sicotyc.Test.Integration.Concurrency;

public sealed class CompanyAdministrationLockSqlServerTests
{
    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ExecuteAsync_ShouldCommit_WhenLockIsAcquired()
    {
        // Arrange
        var connectionString =
            Environment.GetEnvironmentVariable(
                "SICOTYC_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(
            connectionString))
        {
            throw new InvalidOperationException(
                "Configure SICOTYC_TEST_SQLSERVER " +
                "con la conexión a la base de datos de pruebas.");
        }

        using var factory =
            new SqlServerWebApplicationFactory(
                connectionString);

        using var scope =
            factory.Services.CreateScope();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        var companyLock =
            scope.ServiceProvider
                .GetRequiredService<ICompanyAdministrationLock>();

        var service =
            new AdministratorProtectionService(
                unitOfWork,
                companyLock);

        var companyId = Guid.NewGuid();

        var operationExecuted = false;

        // Act
        await service.ExecuteAsync(
            companyId,
            _ =>
            {
                operationExecuted = true;

                return Task.CompletedTask;
            });

        // Assert
        Assert.True(operationExecuted);
    }
}