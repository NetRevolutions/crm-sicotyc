using FluentAssertions;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Jarasoft.Sicotyc.Test.API.Ubigeos
{
    public sealed class UbigeosControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UbigeosControllerTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetDepartamentos_ShouldReturnOkWithDepartamentos()
        {
            // Arrange
            await SeedDatabaseAsync();

            // Act
            var response =
                await _client.GetAsync(
                    "/api/ubigeos/departamentos");

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);

            var result =
                await response.Content
                    .ReadFromJsonAsync<List<DepartamentoDto>>();

            result.Should().NotBeNull();

            result.Should().HaveCount(2);

            result!
                .Select(x => x.Nombre)
                .Should()
                .Equal(
                    "AREQUIPA",
                    "LIMA");
        }

        [Fact]
        public async Task GetProvincias_ShouldReturnOkWithProvincias()
        {
            // Arrange
            await SeedDatabaseAsync();

            // Act
            var response =
                await _client.GetAsync(
                    "/api/ubigeos/departamentos/LIMA/provincias");

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);

            var result =
                await response.Content
                    .ReadFromJsonAsync<List<ProvinciaDto>>();

            result.Should().NotBeNull();

            result!
                .Select(x => x.Nombre)
                .Should()
                .Equal(
                    "BARRANCA",
                    "LIMA");
        }

        [Fact]
        public async Task GetDistritos_ShouldReturnOkWithDistritos()
        {
            // Arrange
            await SeedDatabaseAsync();

            // Act
            var response =
                await _client.GetAsync(
                    "/api/ubigeos/departamentos/LIMA/provincias/LIMA/distritos");

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);

            var result =
                await response.Content
                    .ReadFromJsonAsync<List<DistritoDto>>();

            result.Should().NotBeNull();

            result.Should().HaveCount(3);

            result!
                .Select(x => x.Nombre)
                .Should()
                .Equal(
                    "LIMA",
                    "MIRAFLORES",
                    "SAN ISIDRO");

            result.Should().OnlyContain(x =>
                x.RegionNatural == "COSTA");
        }

        private async Task SeedDatabaseAsync()
        {
            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            await context.Database.EnsureCreatedAsync();

            context.Ubigeos.RemoveRange(context.Ubigeos);

            await context.SaveChangesAsync();

            context.Ubigeos.AddRange(
                new Ubigeo(
                    "150101",
                    "LIMA",
                    "LIMA",
                    "LIMA",
                    "LIMA",
                    1,
                    "COSTA"),

                new Ubigeo(
                    "150122",
                    "LIMA",
                    "LIMA",
                    "MIRAFLORES",
                    "MIRAFLORES",
                    1,
                    "COSTA"),

                new Ubigeo(
                    "150131",
                    "LIMA",
                    "LIMA",
                    "SAN ISIDRO",
                    "SAN ISIDRO",
                    1,
                    "COSTA"),

                new Ubigeo(
                    "150201",
                    "LIMA",
                    "BARRANCA",
                    "BARRANCA",
                    "BARRANCA",
                    1,
                    "COSTA"),

                new Ubigeo(
                    "150202",
                    "LIMA",
                    "BARRANCA",
                    "PARAMONGA",
                    "PARAMONGA",
                    1,
                    "COSTA"),

                new Ubigeo(
                    "040101",
                    "AREQUIPA",
                    "AREQUIPA",
                    "AREQUIPA",
                    "AREQUIPA",
                    2,
                    "SIERRA")
            );

            await context.SaveChangesAsync();
        }
    }
}
