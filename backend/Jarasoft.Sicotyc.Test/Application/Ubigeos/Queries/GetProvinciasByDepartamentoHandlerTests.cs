using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento;
using Moq;

namespace Jarasoft.Sicotyc.Test.Application.Ubigeos.Queries
{
    public sealed class GetProvinciasByDepartamentoHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldReturnProvincias_WhenDepartamentoExits()
        {
            // Arrange
            var repositoryMock = new Mock<IUbigeoRepository>();
            var provincias = new List<string>
            {
                "LIMA",
                "BARRANCA",
                "CAÑETE",
                "HUARAL"
            };
            repositoryMock
                .Setup(repository => 
                    repository.GetProvinciasByDepartamentoAsync(
                    "LIMA",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(provincias);

            var handler = new GetProvinciasByDepartamentoHandler(
                repositoryMock.Object);
            
            var query = new GetProvinciasByDepartamentoQuery("LIMA");
            
            // Act
            var result = await handler.HandleAsync(
                query,
                CancellationToken.None);
            
            // Assert
            result.Should().HaveCount(4);

            result.Should().ContainSingle(p => p.Nombre == "LIMA");
            result.Should().ContainSingle(p => p.Nombre == "BARRANCA");
            result.Should().ContainSingle(p => p.Nombre == "CAÑETE");
            result.Should().ContainSingle(p => p.Nombre == "HUARAL");
            
            repositoryMock.Verify(
                repository => repository.GetProvinciasByDepartamentoAsync(
                    "LIMA",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnEmptyList_WhenDepartamentoHasNoProvincias()
        {
            // Arrange
            var repositoryMock = new Mock<IUbigeoRepository>();

            using var cancellationTokenSource =
                new CancellationTokenSource();

            var cancellationToken =
                cancellationTokenSource.Token;

            repositoryMock
                .Setup(repository =>
                    repository.GetProvinciasByDepartamentoAsync(
                        "LIMA",
                        cancellationToken))
                .ReturnsAsync(Array.Empty<string>());

            var handler =
                new GetProvinciasByDepartamentoHandler(
                    repositoryMock.Object);

            var query =
                new GetProvinciasByDepartamentoQuery("LIMA");

            // Act
            var result = await handler.HandleAsync(
                query,
                cancellationToken);

            // Assert
            result.Should().BeEmpty();

            repositoryMock.Verify(
                repository =>
                    repository.GetProvinciasByDepartamentoAsync(
                        "LIMA",
                        cancellationToken),
                Times.Once);
        }
    }
}
