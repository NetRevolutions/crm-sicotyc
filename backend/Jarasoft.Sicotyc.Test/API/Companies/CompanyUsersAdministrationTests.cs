using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Jarasoft.Sicotyc.Test.API.Companies;

public sealed class CompanyUsersAdministrationTests
    : AuthenticatedIntegrationTestBase
{
    public CompanyUsersAdministrationTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeactivateUsers_ShouldNotAffectUsersFromOtherCompanies()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var companyBUserId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "User",
                "B",
                ApplicationRoles.User);

        var companyAUserId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "User",
                "A",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var companyBUser =
            await userManager.FindByIdAsync(
                companyBUserId.ToString());

        var companyAUser =
            await userManager.FindByIdAsync(
                companyAUserId.ToString());

        companyBUser.Should().NotBeNull();
        companyBUser!.IsActive.Should().BeFalse();

        companyAUser.Should().NotBeNull();
        companyAUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUsers_ShouldAllowDeactivatingLastAdministrator_AsPartOfGlobalOperation()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administratorId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "only.admin.b@test.com",
                "Only",
                "Administrator",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var administrator =
            await userManager.FindByIdAsync(
                administratorId.ToString());

        administrator.Should().NotBeNull();

        administrator!.IsActive
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnForbidden_WhenCallerIsOperations()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnForbidden_WhenCallerIsAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "admin.b@test.com",
            "Admin",
            "B",
            ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnBadRequest_WhenSuperAdministratorTargetsOwnCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        await CreateTestUserAsync(
            environment.CompanyAId,
            "operations.a@test.com",
            "Operations",
            "A",
            ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyAId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var caller =
            await userManager.FindByIdAsync(
                environment.UserId.ToString());

        caller.Should().NotBeNull();
        caller!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        var nonexistentCompanyId =
            Guid.NewGuid();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{nonexistentCompanyId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateUsers_ShouldReturnZero_WhenCompanyHasNoActiveUsers()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "admin.inactive@test.com",
            "Admin",
            "Inactive",
            ApplicationRoles.Administrator,
            isActive: false);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "operations.inactive@test.com",
            "Operations",
            "Inactive",
            ApplicationRoles.Operations,
            isActive: false);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<DeactivateCompanyUsersResult>();

        result.Should().NotBeNull();

        result!.CompanyId
            .Should()
            .Be(environment.CompanyBId);

        result.DeactivatedUsers
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task DeactivateUsers_ShouldDeactivateAllUsers_WhenCallerIsSuperAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "admin.b@test.com",
            "Administrator",
            "B",
            ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "operations.b@test.com",
            "Operations",
            "B",
            ApplicationRoles.Operations);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "billing.b@test.com",
            "Billing",
            "B",
            ApplicationRoles.Billing);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "user.b@test.com",
            "User",
            "B",
            ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/companies/{environment.CompanyBId}/users/deactivate");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var users =
            await userManager.Users
                .Where(x =>
                    x.CompanyId ==
                    environment.CompanyBId)
                .ToListAsync();

        users.Should().HaveCount(4);

        users.Should()
            .OnlyContain(x => !x.IsActive);

        var result =
            await response.Content
                .ReadFromJsonAsync<DeactivateCompanyUsersResult>();

                result.Should().NotBeNull();

                result!.CompanyId
                    .Should()
                    .Be(environment.CompanyBId);

                result.DeactivatedUsers
                    .Should()
                    .Be(4);
    }
}