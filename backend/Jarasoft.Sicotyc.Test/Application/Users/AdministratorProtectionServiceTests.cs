using System.Data;
using FluentAssertions;
using Moq;
using Xunit;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Jarasoft.Sicotyc.Test.Common;

namespace Jarasoft.Sicotyc.Test.Application.Users;

public sealed class AdministratorProtectionServiceTests
{

    [Fact]
    public async Task ExecuteAsync_ShouldSerializeOperations_ForSameCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var firstUnitOfWork = new Mock<IUnitOfWork>();
        var secondUnitOfWork = new Mock<IUnitOfWork>();

        var firstService =
            new AdministratorProtectionService(
                firstUnitOfWork.Object,
                new TestCompanyAdministrationLock());

        var secondService =
            new AdministratorProtectionService(
                secondUnitOfWork.Object,
                new TestCompanyAdministrationLock());

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
            companyId,
            async _ =>
            {
                firstOperationStarted.SetResult();

                await releaseFirstOperation.Task;
            });

        await firstOperationStarted.Task;

        var secondTask = secondService.ExecuteAsync(
            companyId,
            _ =>
            {
                secondOperationStarted.SetResult();

                return Task.CompletedTask;
            });

        // La segunda operación no debe haber
        // ingresado mientras la primera mantiene
        // el bloqueo.
        secondOperationStarted.Task.IsCompleted
            .Should().BeFalse();

        // Liberar la primera operación.
        releaseFirstOperation.SetResult();

        await firstTask;
        await secondTask;

        // Assert
        secondOperationStarted.Task.IsCompletedSuccessfully
            .Should().BeTrue();

        firstUnitOfWork.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        secondUnitOfWork.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldCommitAndReleaseLock_WhenOperationSucceeds()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var unitOfWork = new Mock<IUnitOfWork>();

        var companyLock =
            new TestCompanyAdministrationLock();

        var service =
            new AdministratorProtectionService(
                unitOfWork.Object,
                companyLock);

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
        operationExecuted.Should().BeTrue();

        unitOfWork.Verify(
            x => x.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        // Si el bloqueo fue liberado,
        // podemos adquirirlo nuevamente.
        await companyLock.AcquireAsync(companyId);

        await companyLock.ReleaseAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRollbackAndReleaseLock_WhenOperationFails()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var unitOfWork = new Mock<IUnitOfWork>();

        var companyLock =
            new TestCompanyAdministrationLock();

        var service =
            new AdministratorProtectionService(
                unitOfWork.Object,
                companyLock);

        // Act
        var action = async () =>
            await service.ExecuteAsync(
                companyId,
                _ => throw new InvalidOperationException(
                    "Error de prueba."));

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Error de prueba.");

        unitOfWork.Verify(
            x => x.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        unitOfWork.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        // Verificar que el bloqueo se liberó
        // después del rollback.
        await companyLock.AcquireAsync(companyId);

        await companyLock.ReleaseAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReleaseLock_WhenCommitFails()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Error durante el commit."));

        var companyLock =
            new TestCompanyAdministrationLock();

        var service =
            new AdministratorProtectionService(
                unitOfWork.Object,
                companyLock);

        // Act
        var action = async () =>
            await service.ExecuteAsync(
                companyId,
                _ => Task.CompletedTask);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Error durante el commit.");

        unitOfWork.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        // El bloqueo debe quedar disponible.
        await companyLock.AcquireAsync(companyId);

        await companyLock.ReleaseAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectNullOperation()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>();

        var companyLock =
            new Mock<ICompanyAdministrationLock>();

        var service =
            new AdministratorProtectionService(
                unitOfWork.Object,
                companyLock.Object);

        // Act
        var action = async () =>
            await service.ExecuteAsync(
                Guid.NewGuid(),
                null!);

        // Assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>();

        unitOfWork.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<IsolationLevel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
