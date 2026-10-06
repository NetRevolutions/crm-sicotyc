using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia;
using Jarasoft.Sicotyc.Domain.Entities;
using Moq;

namespace Jarasoft.Sicotyc.Test.Application.Ubigeos.Queries
{
    public sealed class GetDistritosByDepartamentoProvinciaHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldReturnDistritos_WhenDepartamentoProvinciaExits()
        {
            // Arrange
            var repositoryMock = new Mock<IUbigeoRepository>();

            var distritos = new List<Ubigeo>
            {
                new(
                "150101",
                "LIMA",
                "LIMA",
                "LIMA",
                "LIMA",
                1,
                "COSTA"),

            new(
                "150122",
                "LIMA",
                "LIMA",
                "MIRAFLORES",
                "MIRAFLORES",
                1,
                "COSTA")

            };

            repositoryMock
                .Setup(repository => 
                    repository.GetDistritosByDepartamentoProvinciaAsync(
                    "LIMA",
                    "LIMA",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(distritos);

            var handler = 
                new GetDistritosByDepartamentoProvinciaHandler(
                    repositoryMock.Object);
            
            var query = 
                new GetDistritosByDepartamentoProvinciaQuery(
                    "LIMA", 
                    "LIMA");
            
            // Act
            var result = await handler.HandleAsync(
                query,
                CancellationToken.None);

            // Assert
            var resultList = result.ToList();

            result.Should().HaveCount(2);

            resultList[0].Id.Should().Be("150101");
            resultList[0].Nombre.Should().Be("LIMA");
            resultList[0].Capital.Should().Be("LIMA");
            resultList[0].CodigoRegionNatural.Should().Be(1);
            resultList[0].RegionNatural.Should().Be("COSTA");

            resultList[1].Id.Should().Be("150122");
            resultList[1].Nombre.Should().Be("MIRAFLORES");
            resultList[1].Capital.Should().Be("MIRAFLORES");
            resultList[1].CodigoRegionNatural.Should().Be(1);
            resultList[1].RegionNatural.Should().Be("COSTA");

            repositoryMock.Verify(
                repository => repository.GetDistritosByDepartamentoProvinciaAsync(
                    "LIMA",
                    "LIMA",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnEmptyList_WhenProvinciaHasNoDistritos()
        {
            // Arrange
            var repositoryMock = new Mock<IUbigeoRepository>();

            using var cancellationTokenSource =
                new CancellationTokenSource();

            var cancellationToken =
                cancellationTokenSource.Token;

            repositoryMock
                .Setup(repository =>
                    repository.GetDistritosByDepartamentoProvinciaAsync(
                        "LIMA",
                        "LIMA",
                        cancellationToken))
                .ReturnsAsync(Array.Empty<Ubigeo>());

            var handler =
                new GetDistritosByDepartamentoProvinciaHandler(
                    repositoryMock.Object);

            var query =
                new GetDistritosByDepartamentoProvinciaQuery(
                    "LIMA",
                    "LIMA");

            // Act
            var result = await handler.HandleAsync(
                query,
                cancellationToken);

            // Assert
            result.Should().BeEmpty();

            repositoryMock.Verify(
                repository =>
                    repository.GetDistritosByDepartamentoProvinciaAsync(
                        "LIMA",
                        "LIMA",
                        cancellationToken),
                Times.Once);
        }
    }
}
