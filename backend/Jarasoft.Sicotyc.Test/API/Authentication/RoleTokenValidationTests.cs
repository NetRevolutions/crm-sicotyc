using FluentAssertions;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Test.Common;
using System.Net;
using System.Net.Http.Json;

namespace Jarasoft.Sicotyc.Test.API.Authentication;

public sealed class RoleTokenValidationTests(CustomWebApplicationFactory factory)
    : AuthenticatedIntegrationTestBase(factory)
{
    [Theory]
    [InlineData(ApplicationRoles.Administrator, ApplicationRoles.Operations, HttpStatusCode.Forbidden)]
    [InlineData(ApplicationRoles.Operations, ApplicationRoles.Administrator, HttpStatusCode.OK)]
    public async Task ChangedRole_ShouldRejectOldToken_AndUseNewRoleAfterLogin(
        string initialRole, string newRole, HttpStatusCode expectedAccess)
    {
        var environment = await SeedEnvironmentAsync(ApplicationRoles.SuperAdministrator);
        // Otro administrador permite la degradación sin violar la invariante.
        await CreateTestUserAsync(environment.CompanyBId, "keeper@test.com",
            "Keeper", "Admin", ApplicationRoles.Administrator);
        var targetId = await CreateTestUserAsync(environment.CompanyBId,
            "target@test.com", "Target", "User", initialRole);
        var target = environment with { UserId = targetId, Email = "target@test.com" };
        var oldLogin = await LoginAsync(target);
        var actorLogin = await LoginAsync(environment);

        using var change = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{targetId}/role");
        SetBearerToken(change, actorLogin.AccessToken);
        change.Content = JsonContent.Create(new { Role = newRole });
        using var changed = await Client.SendAsync(change);
        changed.StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var path in new[] { "/api/auth/me", "/api/users" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            SetBearerToken(request, oldLogin.AccessToken);
            using var response = await Client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var newLogin = await LoginAsync(target);
        newLogin.Roles.Should().ContainSingle().Which.Should().Be(newRole);
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        SetBearerToken(meRequest, newLogin.AccessToken);
        using var me = await Client.SendAsync(meRequest);
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await me.Content.ReadFromJsonAsync<MeRoles>();
        body!.Roles.Should().ContainSingle().Which.Should().Be(newRole);
        using var accessRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        SetBearerToken(accessRequest, newLogin.AccessToken);
        using var access = await Client.SendAsync(accessRequest);
        access.StatusCode.Should().Be(expectedAccess);
    }

    [Fact]
    public async Task AssigningSameRole_ShouldKeepTokenValid()
    {
        var environment = await SeedEnvironmentAsync(ApplicationRoles.SuperAdministrator);
        var targetId = await CreateTestUserAsync(environment.CompanyBId,
            "target@test.com", "Target", "User", ApplicationRoles.Administrator);
        var login = await LoginAsync(environment with { UserId = targetId, Email = "target@test.com" });
        var actor = await LoginAsync(environment);
        using var change = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{targetId}/role");
        SetBearerToken(change, actor.AccessToken);
        change.Content = JsonContent.Create(new { Role = ApplicationRoles.Administrator });
        using var changed = await Client.SendAsync(change);
        changed.StatusCode.Should().Be(HttpStatusCode.OK);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        SetBearerToken(request, login.AccessToken);
        using var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record MeRoles(string[] Roles);
}
