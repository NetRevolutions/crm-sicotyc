using System.Data;
using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Moq;

namespace Jarasoft.Sicotyc.Test.Application.Companies
{
    public sealed class DeactivateCompanyUsersHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldRollbackTransaction_WhenBulkDeactivationFails()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            var currentUser = new Mock<ICurrentUser>();
            var identityService = new Mock<IIdentityService>();
            var companyRepository = new Mock<ICompanyRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();

            currentUser
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            currentUser
                .Setup(x => x.IsSuperAdministrator)
                .Returns(true);

            currentUser
                .Setup(x => x.CompanyId)
                .Returns(Guid.NewGuid());

            companyRepository
                .Setup(x => x.ExistsByIdAsync(
                    companyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            identityService
                .Setup(x => x.DeactivateUsersByCompanyAsync(
                    companyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new IdentityBulkOperationResult(
                        false,
                        1,
                        ["Error de prueba."]));

            var companyLock =
                new Mock<ICompanyAdministrationLock>();

            var administratorProtection =
                new AdministratorProtectionService(
                    unitOfWork.Object,
                    companyLock.Object);

            var handler =
                new DeactivateCompanyUsersHandler(
                    currentUser.Object,
                    identityService.Object,
                    companyRepository.Object,
                    administratorProtection);

            // Act
            var action = async () =>
                await handler.HandleAsync(
                    new DeactivateCompanyUsersCommand(companyId));

            // Assert
            await action.Should()
                .ThrowAsync<ValidationException>();

            unitOfWork.Verify(
                x => x.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                x => x.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                x => x.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_ShouldCommitTransaction_WhenBulkDeactivationSucceeds()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            var currentUser = new Mock<ICurrentUser>();
            var identityService = new Mock<IIdentityService>();
            var companyRepository = new Mock<ICompanyRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();

            currentUser
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            currentUser
                .Setup(x => x.IsSuperAdministrator)
                .Returns(true);

            currentUser
                .Setup(x => x.CompanyId)
                .Returns(Guid.NewGuid());

            companyRepository
                .Setup(x => x.ExistsByIdAsync(
                    companyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            identityService
                .Setup(x => x.DeactivateUsersByCompanyAsync(
                    companyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new IdentityBulkOperationResult(
                        true,
                        3,
                        Array.Empty<string>()));

            var companyLock = new Mock<ICompanyAdministrationLock>();

            var administratorProtection =
                new AdministratorProtectionService(
                    unitOfWork.Object,
                    companyLock.Object);

            var handler = new DeactivateCompanyUsersHandler(
                currentUser.Object,
                identityService.Object,
                companyRepository.Object,
                administratorProtection);

            // Act
            var result =
                await handler.HandleAsync(
                    new DeactivateCompanyUsersCommand(companyId));

            // Assert
            result.CompanyId.Should().Be(companyId);
            result.DeactivatedUsers.Should().Be(3);

            unitOfWork.Verify(
                x => x.BeginTransactionAsync(
                    IsolationLevel.Serializable,
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
        }
    }
}
