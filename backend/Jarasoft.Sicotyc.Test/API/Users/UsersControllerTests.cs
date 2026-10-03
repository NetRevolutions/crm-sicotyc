using FluentAssertions;
using Jarasoft.Sicotyc.API.Contracts.Users;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace Jarasoft.Sicotyc.Test.API.Users;

public sealed class UsersControllerTests
    : AuthenticatedIntegrationTestBase
{
    public UsersControllerTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {        
    }

    #region Test Methods

    [Fact]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "delete.target@test.com",
                "Target",
                "User",
                ApplicationRoles.User);

        var response =
            await Client.DeleteAsync(
                $"/api/users/{userId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnForbidden_WhenCallerIsOperations()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Operations);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "delete.target@test.com",
                "Target",
                "User",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{Guid.NewGuid()}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ShouldAllowSuperAdministratorToDeleteUserFromAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "operations.b@test.com",
                "Operations",
                "B",
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUser_ShouldAllowDeletingAdministrator_WhenAnotherActiveAdministratorRemains()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var secondAdministratorId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "admin2.a@test.com",
                "Administrador",
                "Dos",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{secondAdministratorId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var deletedUser =
            await userManager.FindByIdAsync(
                secondAdministratorId.ToString());

        deletedUser.Should().BeNull();

        var administrators =
            await userManager.GetUsersInRoleAsync(
                ApplicationRoles.Administrator);

        administrators
            .Count(x =>
                x.CompanyId == environment.CompanyAId &&
                x.IsActive)
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenDeletingLastActiveAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administratorId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "admin.b@test.com",
                "Administrador",
                "B",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{administratorId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
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

        var user =
            await userManager.FindByIdAsync(
                administratorId.ToString());

        user.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenAdministratorDeletesItself()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{environment.UserId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserBelongsToAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);

        // Importante: además comprobamos que
        // el usuario NO fue eliminado.
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUser_WhenUserBelongsToAdministratorsCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.delete@test.com",
                "Usuario",
                "Eliminar",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user.Should().BeNull();
    }

    [Fact]
    public async Task ChangeUserRole_ShouldAllowSuperAdministratorToAssignAdministratorInAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "operations.b@test.com",
                "Operations",
                "B",
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.Administrator));

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

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        var roles =
            await userManager.GetRolesAsync(user!);

        roles.Should()
            .ContainSingle()
            .Which.Should()
            .Be(ApplicationRoles.Administrator);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldAllowChangingAdministratorRole_WhenAnotherActiveAdministratorExists()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administrator1Id =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "admin1.b@test.com",
                "Administrador",
                "Uno",
                ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "admin2.b@test.com",
            "Administrador",
            "Dos",
            ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{administrator1Id}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.Operations));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldReturnBadRequest_WhenRemovingRoleFromLastActiveAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administratorId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "admin.b@test.com",
                "Administrador",
                "B",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{administratorId}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.Operations));

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

        var user =
            await userManager.FindByIdAsync(
                administratorId.ToString());

        var roles =
            await userManager.GetRolesAsync(user!);

        roles.Should()
            .Contain(ApplicationRoles.Administrator);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldReturnNotFound_WhenUserBelongsToAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.Administrator));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldReturnBadRequest_WhenAdministratorAssignsSuperAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "Usuario",
                "A",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.SuperAdministrator));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldAllowAdministratorToPromoteUserToAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "operations.a@test.com",
                "Operations",
                "User",
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/role");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserRoleRequest(
                    ApplicationRoles.Administrator));

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

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        var roles =
            await userManager.GetRolesAsync(user!);

        roles.Should()
            .ContainSingle()
            .Which.Should()
            .Be(ApplicationRoles.Administrator);
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldAllowAdministratorToDeactivateAnotherAdministrator_WhenAnotherActiveAdministratorRemains()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var secondAdministratorId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "admin2.a@test.com",
                "Administrador",
                "Dos",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{secondAdministratorId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldAllowDeactivatingAdministrator_WhenAnotherActiveAdministratorExists()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administrator1Id =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "admin1.b@test.com",
                "Administrador",
                "Uno",
                ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "admin2.b@test.com",
            "Administrador",
            "Dos",
            ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{administrator1Id}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

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

        var administrator1 =
            await userManager.FindByIdAsync(
                administrator1Id.ToString());

        administrator1.Should().NotBeNull();
        administrator1!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldReturnBadRequest_WhenDeactivatingLastActiveAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var administratorId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "admin.b@test.com",
                "Administrador",
                "Empresa B",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{administratorId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        // Además comprobamos que realmente
        // haya permanecido activo.
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(
                administratorId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{Guid.NewGuid()}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldAllowSuperAdministratorToDeactivateUserFromAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

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

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldReturnBadRequest_WhenAdministratorDeactivatesItself()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{environment.UserId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldReturnNotFound_WhenUserBelongsToAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldReactivateUser_WhenUserIsInactive()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "inactive@test.com",
                "Usuario",
                "Inactivo",
                ApplicationRoles.User);

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

            var user =
                await userManager.FindByIdAsync(
                    userId.ToString());

            user!.IsActive = false;

            var result =
                await userManager.UpdateAsync(user);

            result.Succeeded.Should().BeTrue();
        }

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(true));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        using var verificationScope =
            Factory.Services.CreateScope();

        var verificationUserManager =
            verificationScope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var updatedUser =
            await verificationUserManager.FindByIdAsync(
                userId.ToString());

        updatedUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeUserStatus_ShouldDeactivateUser_WhenUserBelongsToAdministratorsCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "Usuario",
                "A",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/users/{userId}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new ChangeUserStatusRequest(false));

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

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "Usuario",
                "A",
                ApplicationRoles.User);

        var response =
            await Client.GetAsync(
                $"/api/users/{userId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnForbidden_WhenCallerIsOperations()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Operations);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "Usuario",
                "A",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users/{Guid.NewGuid()}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task GetUserById_ShouldAllowSuperAdministratorToQueryUserFromAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var user =
            await response.Content
                .ReadFromJsonAsync<UserListResponse>();

        user.Should().NotBeNull();

        user!.Id
            .Should()
            .Be(userId);

        user.CompanyId
            .Should()
            .Be(environment.CompanyBId);

        user.Email
            .Should()
            .Be("user.b@test.com");
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserBelongsToAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyBId,
                "user.b@test.com",
                "Usuario",
                "B",
                ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserBelongsToAdministratorsCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var userId =
            await CreateTestUserAsync(
                environment.CompanyAId,
                "user.a@test.com",
                "Usuario",
                "A",
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users/{userId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var user =
            await response.Content
                .ReadFromJsonAsync<UserListResponse>();

        user.Should().NotBeNull();

        user!.Id
            .Should()
            .Be(userId);

        user.CompanyId
            .Should()
            .Be(environment.CompanyAId);

        user.Email
            .Should()
            .Be("user.a@test.com");

        user.Roles
            .Should()
            .Contain(ApplicationRoles.Operations);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        await SeedEnvironmentAsync(
            ApplicationRoles.Administrator);

        // Act
        var response =
            await Client.GetAsync(
                "/api/users");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnForbidden_WhenCallerIsOperations()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnBadRequest_WhenCompanyDoesNotExist()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        var invalidCompanyId =
            Guid.NewGuid();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users?companyId={invalidCompanyId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnBadRequest_WhenSuperAdministratorDoesNotProvideCompanyId()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task GetUsers_ShouldAllowSuperAdministratorToQueryAnotherCompany()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        await CreateTestUserAsync(
            environment.CompanyAId,
            "user.a@test.com",
            "Usuario",
            "A",
            ApplicationRoles.User);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "user.b@test.com",
            "Usuario",
            "B",
            ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users?companyId={environment.CompanyBId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var users =
            await response.Content
                .ReadFromJsonAsync<UserListResponse[]>();

        users.Should().NotBeNull();

        users!
            .Should()
            .OnlyContain(
                x => x.CompanyId ==
                     environment.CompanyBId);

        users
            .Should()
            .ContainSingle(
                x => x.Email == "user.b@test.com");

        users
            .Should()
            .NotContain(
                x => x.Email == "user.a@test.com");
    }

    [Fact]
    public async Task GetUsers_ShouldIgnoreCompanyId_WhenCallerIsAdministrator()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyAId,
            "user.a@test.com",
            "Usuario",
            "A",
            ApplicationRoles.User);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "user.b@test.com",
            "Usuario",
            "B",
            ApplicationRoles.User);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/users?companyId={environment.CompanyBId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var users =
            await response.Content
                .ReadFromJsonAsync<UserListResponse[]>();

        users.Should().NotBeNull();

        users!
            .Should()
            .OnlyContain(
                x => x.CompanyId ==
                     environment.CompanyAId);

        users
            .Should()
            .Contain(
                x => x.Email == "user.a@test.com");

        users
            .Should()
            .NotContain(
                x => x.Email == "user.b@test.com");
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOnlyUsersFromAdministratorsCompany()
    {
        // Arrange
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        await CreateTestUserAsync(
            environment.CompanyAId,
            "operations.a@test.com",
            "Carlos",
            "Perez",
            ApplicationRoles.Operations);

        await CreateTestUserAsync(
            environment.CompanyBId,
            "operations.b@test.com",
            "Pedro",
            "Gomez",
            ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await Client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var users =
            await response.Content
                .ReadFromJsonAsync<UserListResponse[]>();

        users.Should().NotBeNull();

        users!
            .Should()
            .NotBeEmpty();

        users
            .Should()
            .OnlyContain(
                x => x.CompanyId ==
                     environment.CompanyAId);

        users
            .Should()
            .Contain(
                x => x.Email ==
                     "operations.a@test.com");

        users
            .Should()
            .NotContain(
                x => x.Email ==
                     "operations.b@test.com");
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUserInAdministratorsCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Carlos",
                    "Perez",
                    "carlos@test.com",
                    "Test123!",
                    ApplicationRoles.Operations,
                    null));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByEmailAsync(
                "carlos@test.com");

        user.Should().NotBeNull();

        user!.CompanyId
            .Should()
            .Be(environment.CompanyAId);

        var roles =
            await userManager.GetRolesAsync(user);

        roles.Should()
            .Contain(ApplicationRoles.Operations);
    }

    [Fact]
    public async Task CreateUser_ShouldIgnoreCompanyId_WhenCallerIsAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Pedro",
                    "Gomez",
                    "pedro@test.com",
                    "Test123!",
                    ApplicationRoles.User,
                    environment.CompanyBId));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByEmailAsync(
                "pedro@test.com");

        user.Should().NotBeNull();

        user!.CompanyId
            .Should()
            .Be(environment.CompanyAId);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenAdministratorAssignsSuperAdministrator()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Super",
                    "Admin",
                    "super@test.com",
                    "Test123!",
                    ApplicationRoles.SuperAdministrator,
                    null));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnForbidden_WhenCallerIsOperations()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Operations);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Ana",
                    "Torres",
                    "ana@test.com",
                    "Test123!",
                    ApplicationRoles.User,
                    null));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        await SeedEnvironmentAsync(
            ApplicationRoles.Administrator);

        var request =
            new CreateUserRequest(
                "Ana",
                "Torres",
                "ana@test.com",
                "Test123!",
                ApplicationRoles.User,
                null);

        var response =
            await Client.PostAsJsonAsync(
                "/api/users",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_ShouldAllowSuperAdministratorToCreateUserInAnotherCompany()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Maria",
                    "Lopez",
                    "maria@test.com",
                    "Test123!",
                    ApplicationRoles.Administrator,
                    environment.CompanyBId));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByEmailAsync(
                "maria@test.com");

        user.Should().NotBeNull();

        user!.CompanyId
            .Should()
            .Be(environment.CompanyBId);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenSuperAdministratorDoesNotProvideCompanyId()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Maria",
                    "Lopez",
                    "maria@test.com",
                    "Test123!",
                    ApplicationRoles.User,
                    null));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenCompanyDoesNotExist()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.SuperAdministrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Maria",
                    "Lopez",
                    "maria@test.com",
                    "Test123!",
                    ApplicationRoles.User,
                    Guid.NewGuid()));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var environment =
            await SeedEnvironmentAsync(
                ApplicationRoles.Administrator);

        var login =
            await LoginAsync(environment);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        request.Content =
            JsonContent.Create(
                new CreateUserRequest(
                    "Duplicado",
                    "Test",
                    environment.Email,
                    "Test123!",
                    ApplicationRoles.User,
                    null));

        var response =
            await Client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    #endregion    

    #region Private Records
    private sealed record UserListResponse(
        Guid Id,
        Guid CompanyId,
        string FirstName,
        string LastName,
        string Email,
        bool IsActive,
        string[] Roles);

    #endregion
}