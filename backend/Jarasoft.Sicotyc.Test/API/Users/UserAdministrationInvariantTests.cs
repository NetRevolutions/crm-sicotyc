using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

public sealed class UserAdministrationInvariantTests
    : AuthenticatedIntegrationTestBase
{
    public UserAdministrationInvariantTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Administrator_ShouldNotModifySuperAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var superAdminId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "superadmin@test.com",
                "Super",
                "Administrator",
                ApplicationRoles.SuperAdministrator);

        var login = await LoginAsync(environment);

        using var statusRequest =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{superAdminId}/status");

        SetBearerToken(
            statusRequest,
            login.AccessToken);

        statusRequest.Content =
            JsonContent.Create(new
            {
                IsActive = false
            });

        var statusResponse =
            await Client.SendAsync(statusRequest);

        statusResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);

        using var roleRequest =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{superAdminId}/role");

        SetBearerToken(
            roleRequest,
            login.AccessToken);

        roleRequest.Content =
            JsonContent.Create(new
            {
                Role = ApplicationRoles.Operations
            });

        var roleResponse =
            await Client.SendAsync(roleRequest);

        roleResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);

        using var deleteRequest =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{superAdminId}");

        SetBearerToken(
            deleteRequest,
            login.AccessToken);

        var deleteResponse =
            await Client.SendAsync(deleteRequest);

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdministrator_ShouldNotDeactivateOrDeleteItself()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login = await LoginAsync(environment);

        using var statusRequest =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{environment.UserId}/status");

        SetBearerToken(
            statusRequest,
            login.AccessToken);

        statusRequest.Content =
            JsonContent.Create(new
            {
                IsActive = false
            });

        var statusResponse =
            await Client.SendAsync(statusRequest);

        statusResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        using var deleteRequest =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{environment.UserId}");

        SetBearerToken(
            deleteRequest,
            login.AccessToken);

        var deleteResponse =
            await Client.SendAsync(deleteRequest);

        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivatedUser_ShouldNotAccessProtectedEndpoints_WithExistingToken()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using (var scope =
            Factory.Services.CreateScope())
        {
            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

            var user =
                await userManager.FindByIdAsync(
                    environment.UserId.ToString());

            user.Should().NotBeNull();

            user!.IsActive = false;

            var updateResult =
                await userManager.UpdateAsync(user);

            updateResult.Succeeded.Should().BeTrue();
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me");

        SetBearerToken(
            request,
            login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}
