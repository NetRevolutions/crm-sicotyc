using FluentAssertions;
using Jarasoft.Sicotyc.API.Contracts.Authentication;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Jarasoft.Sicotyc.Test.API.Authentication
{
    public sealed class AuthControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        public AuthControllerTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task SeedDatabaseAsync()
        {
            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            await context.Database.EnsureCreatedAsync();

            // Limpiar primero las tablas dependientes.
            context.UserRoles.RemoveRange(
                context.UserRoles);

            context.Users.RemoveRange(
                context.Users);

            context.Companies.RemoveRange(
                context.Companies);

            context.Ubigeos.RemoveRange(
                context.Ubigeos);

            await context.SaveChangesAsync();

            // Ubigeo necesario para crear Companies.
            context.Ubigeos.Add(
                new Ubigeo(
                    "150101",
                    "LIMA",
                    "LIMA",
                    "LIMA",
                    "LIMA",
                    1,
                    "COSTA"));

            await context.SaveChangesAsync();

            // Garantizar rol User.
            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<RoleManager<ApplicationRole>>();

            if (!await roleManager.RoleExistsAsync(
                    ApplicationRoles.User))
            {
                var result =
                    await roleManager.CreateAsync(
                        new ApplicationRole
                        {
                            Id = Guid.NewGuid(),
                            Name = ApplicationRoles.User
                        });

                result.Succeeded.Should().BeTrue();
            }
        }

        private static RegisterRequest CreateValidRequest(
            string email = "usuario.test@test.com",
            string ruc = "20123456789")
        {
            return new RegisterRequest(
                FirstName: "Usuario",
                LastName: "Prueba",
                Email: email,
                Password: "Test123!",
                Ruc: ruc,
                NombreEmpresa: "Empresa de Prueba SAC",
                Direccion: "Av. Prueba 123",
                IdDist: "150101",
                CompanyEmail: "contacto@empresa-prueba.com");
        }

        // Test...

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenRequestIsValid()
        {
            // Arrange
            await SeedDatabaseAsync();

            var request = CreateValidRequest();

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.Created);

            var result =
                await response.Content
                    .ReadFromJsonAsync<RegisterUserResult>();

            result.Should().NotBeNull();

            result!.UserId
                .Should()
                .NotBe(Guid.Empty);

            result.CompanyId
                .Should()
                .NotBe(Guid.Empty);

            result.Email
                .Should()
                .Be(request.Email);
        }

        [Fact]
        public async Task Register_ShouldCreateCompanyUserAndUserRole()
        {
            // Arrange
            await SeedDatabaseAsync();

            var request = CreateValidRequest();

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.Created);

            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var company =
                await context.Companies
                    .SingleOrDefaultAsync(
                        x => x.Ruc == request.Ruc);            

            company.Should().NotBeNull();

            company!.ActualizadoDeSunat
                .Should()
                .BeFalse();

            company.FechaActualizacionSunat
                .Should()
                .BeNull();

            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

            var user =
                await userManager.FindByEmailAsync(
                    request.Email);

            user.Should().NotBeNull();

            user!.CompanyId
                .Should()
                .Be(company.Id);

            var roles =
                await userManager.GetRolesAsync(user);

            roles.Should().Contain(
                ApplicationRoles.User);
        }

        [Fact]
        public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            await SeedDatabaseAsync();

            var request = CreateValidRequest();

            var firstResponse =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            firstResponse.StatusCode
                .Should()
                .Be(HttpStatusCode.Created);

            // Act
            var secondResponse =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            // Assert
            secondResponse.StatusCode
                .Should()
                .Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUbigeoDoesNotExist()
        {
            // Arrange
            await SeedDatabaseAsync();

            var request =
                new RegisterRequest(
                    FirstName: "Usuario",
                    LastName: "Prueba",
                    Email: "ubigeo.invalido@test.com",
                    Password: "Test123!",
                    Ruc: "20999999999",
                    NombreEmpresa: "Empresa Ubigeo Test",
                    Direccion: "Av. Prueba 123",
                    IdDist: "999999",
                    CompanyEmail: null);

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            // Assert
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.BadRequest);

            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var companyExists =
                await context.Companies
                    .AnyAsync(
                        x => x.Ruc == request.Ruc);

            companyExists.Should().BeFalse();

            var userExists =
                await context.Users
                    .AnyAsync(
                        x => x.Email == request.Email);

            userExists.Should().BeFalse();
        }

        [Fact]
        public async Task Register_ShouldReuseCompany_WhenRucAlreadyExists()
        {
            // Arrange
            await SeedDatabaseAsync();

            var firstRequest =
                CreateValidRequest(
                    email: "usuario1@test.com");

            var secondRequest =
                CreateValidRequest(
                    email: "usuario2@test.com");

            var firstResponse =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    firstRequest);

            firstResponse.StatusCode
                .Should()
                .Be(HttpStatusCode.Created);

            // Act
            var secondResponse =
                await _client.PostAsJsonAsync(
                    "/api/auth/register",
                    secondRequest);

            // Assert
            secondResponse.StatusCode
                .Should()
                .Be(HttpStatusCode.Created);

            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<SicotycDbContext>();

            var companies =
                await context.Companies
                    .Where(x =>
                        x.Ruc == firstRequest.Ruc)
                    .ToListAsync();

            companies.Should().HaveCount(1);

            var companyId =
                companies.Single().Id;

            var users =
                await context.Users
                    .Where(x =>
                        x.CompanyId == companyId)
                    .ToListAsync();

            users.Should().HaveCount(2);

            users
                .Select(x => x.Email)
                .Should()
                .Contain(
                    firstRequest.Email,
                    secondRequest.Email);
        }


    }
}
