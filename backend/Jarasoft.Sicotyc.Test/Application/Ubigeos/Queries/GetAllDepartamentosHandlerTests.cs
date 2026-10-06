using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos;
using Moq;

namespace Jarasoft.Sicotyc.Test.Application.Ubigeos.Queries
{
    public sealed class GetAllDepartamentosHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldReturnDepartamentos_WhenDepartamentosExist()
        {
            // Arrange (Organizar)
            var repositoryMock = new Mock<IUbigeoRepository>();

            var departamentos = new List<string> 
            { 
                "LIMA", 
                "AREQUIPA",
                "CUSCO"
            };

            repositoryMock
                .Setup(repository => repository.GetDepartamentosAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(departamentos);

            var handler = new GetAllDepartamentosHandler(
                repositoryMock.Object);

            var query = new GetAllDepartamentosQuery();

            // Act (Actuar)
            var result = await handler.HandleAsync(
                query, 
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);

            result.Should().ContainSingle(d => d.Nombre == "LIMA");
            result.Should().ContainSingle(d => d.Nombre == "AREQUIPA");
            result.Should().ContainSingle(d => d.Nombre == "CUSCO");

            repositoryMock.Verify(
                repository => repository.GetDepartamentosAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnEmptyList_WhenNoDepartamentosExist()
        {
            // Arrange (Afirmar)
            var repositoryMock = new Mock<IUbigeoRepository>();

            using var cancellationTokenSource =
                    new CancellationTokenSource();

            var cancellationToken =
                cancellationTokenSource.Token;

            repositoryMock
                .Setup(repository => repository.GetDepartamentosAsync(
                    cancellationToken))
                .ReturnsAsync(new List<string>());

            var handler = new GetAllDepartamentosHandler(
                repositoryMock.Object);

            var query = new GetAllDepartamentosQuery();

            // Act
            var result = await handler.HandleAsync(
                query, 
                cancellationToken);

            // Assert
            result.Should().BeEmpty();

            repositoryMock.Verify(
                repository => 
                    repository.GetDepartamentosAsync(cancellationToken),
                Times.Once);
        } 
    }
}
