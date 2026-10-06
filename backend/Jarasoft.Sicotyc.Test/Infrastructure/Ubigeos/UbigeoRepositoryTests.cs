using FluentAssertions;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Test.Infrastructure.Ubigeos
{
    public sealed class UbigeoRepositoryTests
    {
        private static async Task<(SqliteConnection Connection, SicotycDbContext Context)>
            CreateDbContextAsync()
        { 
            var connection = 
                new SqliteConnection("DataSource=:memory:");
            
            await connection.OpenAsync();

            var options = 
                new DbContextOptionsBuilder<SicotycDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = 
                new SicotycDbContext(options);

            await context.Database.EnsureCreatedAsync();

            return (connection, context);
        }

        [Fact]
        public async Task GetDepartamentosAsync_ShouldReturnDistinctOrderedDepartamentos()
        {
            // Arrange
            var (connection, context) =
                await CreateDbContextAsync();

            await using (connection)
            await using (context)
            {
                var ubigeos = new List<Ubigeo>
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
                        "COSTA"),

                    new(
                        "040101",
                        "AREQUIPA",
                        "AREQUIPA",
                        "AREQUIPA",
                        "AREQUIPA",
                        2,
                        "SIERRA"),

                    new(
                        "080101",
                        "CUSCO",
                        "CUSCO",
                        "CUSCO",
                        "CUSCO",
                        2,
                        "SIERRA")
                };

                await context.Ubigeos.AddRangeAsync(ubigeos);
                await context.SaveChangesAsync();

                var repository =
                    new UbigeoRepository(context);

                // Act
                var result =
                    await repository.GetDepartamentosAsync(
                        CancellationToken.None);

                // Assert
                result.Should().HaveCount(3);

                result.Should().Equal(
                    "AREQUIPA",
                    "CUSCO",
                    "LIMA");
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUbigeos()
        {
            // Arrange
            var (connection, context) =
                await CreateDbContextAsync();

            await using (connection)
            await using (context)
            {
                var ubigeos = new List<Ubigeo>
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
                "COSTA"),

            new(
                "040101",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                2,
                "SIERRA")
        };

                await context.Ubigeos.AddRangeAsync(ubigeos);
                await context.SaveChangesAsync();

                var repository =
                    new UbigeoRepository(context);

                // Act
                var result =
                    await repository.GetAllAsync(
                        CancellationToken.None);

                // Assert
                result.Should().HaveCount(3);

                result.Should().Contain(x =>
                    x.IdDist == "150101" &&
                    x.NombreDistrito == "LIMA");

                result.Should().Contain(x =>
                    x.IdDist == "150122" &&
                    x.NombreDistrito == "MIRAFLORES");

                result.Should().Contain(x =>
                    x.IdDist == "040101" &&
                    x.NombreDistrito == "AREQUIPA");
            }
        }

        [Fact]
        public async Task GetProvinciasByDepartamentoAsync_ShouldReturnOnlyDistinctOrderedProvinciasForDepartamento()
        {
            // Arrange
            var (connection, context) =
                await CreateDbContextAsync();

            await using (connection)
            await using (context)
            {
                var ubigeos = new List<Ubigeo>
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
                "COSTA"),

            new(
                "150201",
                "LIMA",
                "BARRANCA",
                "BARRANCA",
                "BARRANCA",
                1,
                "COSTA"),

            new(
                "150801",
                "LIMA",
                "HUAURA",
                "HUACHO",
                "HUACHO",
                1,
                "COSTA"),

            new(
                "040101",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                2,
                "SIERRA")
        };

                await context.Ubigeos.AddRangeAsync(ubigeos);
                await context.SaveChangesAsync();

                var repository =
                    new UbigeoRepository(context);

                // Act
                var result =
                    await repository.GetProvinciasByDepartamentoAsync(
                        "LIMA",
                        CancellationToken.None);

                // Assert
                result.Should().HaveCount(3);

                result.Should().Equal(
                    "BARRANCA",
                    "HUAURA",
                    "LIMA");

                result.Should().NotContain("AREQUIPA");
            }
        }

        [Fact]
        public async Task GetDistritosByDepartamentoProvinciaAsync_ShouldReturnOnlyDistrictsForDepartamentoAndProvincia()
        {
            // Arrange
            var (connection, context) =
                await CreateDbContextAsync();

            await using (connection)
            await using (context)
            {
                var ubigeos = new List<Ubigeo>
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
                "COSTA"),

            new(
                "150131",
                "LIMA",
                "LIMA",
                "SAN ISIDRO",
                "SAN ISIDRO",
                1,
                "COSTA"),

            // Mismo departamento, otra provincia
            new(
                "150201",
                "LIMA",
                "BARRANCA",
                "BARRANCA",
                "BARRANCA",
                1,
                "COSTA"),

            // Otro departamento
            new(
                "040101",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                "AREQUIPA",
                2,
                "SIERRA")
        };

                await context.Ubigeos.AddRangeAsync(ubigeos);
                await context.SaveChangesAsync();

                var repository =
                    new UbigeoRepository(context);

                // Act
                var result =
                    await repository
                        .GetDistritosByDepartamentoProvinciaAsync(
                            "LIMA",
                            "LIMA",
                            CancellationToken.None);

                // Assert
                result.Should().HaveCount(3);

                result.Should().OnlyContain(x =>
                    x.NombreDepartamento == "LIMA" &&
                    x.NombreProvincia == "LIMA");

                result
                    .Select(x => x.NombreDistrito)
                    .Should()
                    .Equal(
                        "LIMA",
                        "MIRAFLORES",
                        "SAN ISIDRO");

                result.Should().NotContain(x =>
                    x.NombreDistrito == "BARRANCA");

                result.Should().NotContain(x =>
                    x.NombreDistrito == "AREQUIPA");
            }
        }
    }
}
