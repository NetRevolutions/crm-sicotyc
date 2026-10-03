using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Jarasoft.Sicotyc.Test.Common;

public abstract class AuthenticatedIntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected AuthenticatedIntegrationTestBase(
        CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<TestEnvironment> SeedEnvironmentAsync(
        string role)
    {
        using var scope =
            Factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        // Limpiar relaciones Identity primero.
        context.UserRoles.RemoveRange(
            context.UserRoles);

        context.Users.RemoveRange(
            context.Users);

        context.Companies.RemoveRange(
            context.Companies);

        context.Ubigeos.RemoveRange(
            context.Ubigeos);

        await context.SaveChangesAsync();

        // Ubigeo requerido por Company.
        var ubigeo =
            CreateTestUbigeo();

        context.Ubigeos.Add(ubigeo);

        var companyA =
            CreateTestCompany(
                "20111111111",
                "Company A",
                ubigeo.IdDist);

        var companyB =
            CreateTestCompany(
                "20222222222",
                "Company B",
                ubigeo.IdDist);

        context.Companies.AddRange(
            companyA,
            companyB);

        await context.SaveChangesAsync();

        const string email =
            "caller@test.com";

        const string password =
            "Test123!";

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyA.Id,
                FirstName = "Test",
                LastName = "Caller",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        var createResult =
            await userManager.CreateAsync(
                user,
                password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    createResult.Errors.Select(
                        x => x.Description)));
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    roleResult.Errors.Select(
                        x => x.Description)));
        }

        return new TestEnvironment(
            user.Id,
            companyA.Id,
            companyB.Id,
            email,
            password);
    }

    protected async Task<LoginResult> LoginAsync(
        TestEnvironment environment)
    {
        var response =
            await Client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    environment.Email,
                    environment.Password
                });

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResult>();

        return result
            ?? throw new InvalidOperationException(
                "No se pudo obtener la respuesta del login.");
    }

    protected async Task<Guid> CreateTestUserAsync(
        Guid companyId,
        string email,
        string firstName,
        string lastName,
        string role,
        bool isActive = true)
    {
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Test123!");

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    createResult.Errors.Select(
                        x => x.Description)));
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    ", ",
                    roleResult.Errors.Select(
                        x => x.Description)));
        }

        return user.Id;
    }

    protected static void SetBearerToken(
        HttpRequestMessage request,
        string accessToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }

    #region Private Methods

    private static Ubigeo CreateTestUbigeo()
    {
        // Adapta el constructor/propiedades a tu
        // entidad Ubigeo actual.
        return new Ubigeo(
            "150101",
            "Lima",
            "Lima",
            "Lima",
            "Lima",
            1,
            "Costa"
            );

    }

    private static Company CreateTestCompany(
        string ruc,
        string nombreEmpresa,
        string idDist)
    {
        // Adapta este constructor a la implementación
        // exacta actual de Company.
        return new Company(
            ruc,
            nombreEmpresa,
            "Dirección Test",
            idDist
            );
    }
    private async Task<Guid> CreateTestUserAsync(
    Guid companyId,
    string email,
    string firstName,
    string lastName,
    string role)
    {
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Test123!");

        createResult.Succeeded
            .Should()
            .BeTrue();

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                role);

        roleResult.Succeeded
            .Should()
            .BeTrue();

        return user.Id;
    }
    #endregion

    #region Private Records (DTOs)
    protected sealed record TestEnvironment(
        Guid UserId,
        Guid CompanyAId,
        Guid CompanyBId,
        string Email,
        string Password);
    #endregion
}