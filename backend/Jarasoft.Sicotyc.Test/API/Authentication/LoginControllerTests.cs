using FluentAssertions;
using Jarasoft.Sicotyc.API.Contracts.Authentication;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Jarasoft.Sicotyc.Test.API.Authentication;

public sealed class LoginControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LoginControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }       

    // Tests...
    [Fact]
    public async Task Login_ShouldReturnOkWithAccessToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user =
            await SeedUserAsync();

        var request =
            new LoginRequest(
                user.Email,
                user.Password);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResult>();

        result.Should().NotBeNull();

        result!.UserId
            .Should()
            .Be(user.UserId);

        result.CompanyId
            .Should()
            .Be(user.CompanyId);

        result.Email
            .Should()
            .Be(user.Email);

        result.Roles
            .Should()
            .Contain(ApplicationRoles.User);

        result.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        result.ExpiresAt
            .Should()
            .BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldGenerateTokenWithExpectedClaims()
    {
        // Arrange
        var user =
            await SeedUserAsync();

        var request =
            new LoginRequest(
                user.Email,
                user.Password);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResult>();

        // Assert
        result.Should().NotBeNull();

        var handler =
            new JwtSecurityTokenHandler();

        var token =
            handler.ReadJwtToken(
                result!.AccessToken);

        token.Claims
            .Single(x =>
                x.Type ==
                JwtRegisteredClaimNames.Sub)
            .Value
            .Should()
            .Be(user.UserId.ToString());

        token.Claims
            .Single(x =>
                x.Type == "company_id")
            .Value
            .Should()
            .Be(user.CompanyId.ToString());

        token.Claims
            .Single(x =>
                x.Type ==
                JwtRegisteredClaimNames.Email)
            .Value
            .Should()
            .Be(user.Email);

        token.Claims
            .Where(x =>
                x.Type == ClaimTypes.Role)
            .Select(x => x.Value)
            .Should()
            .Contain(ApplicationRoles.User);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var user =
            await SeedUserAsync();

        var request =
            new LoginRequest(
                user.Email,
                "PasswordIncorrecto123!");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        await SeedUserAsync();

        var request =
            new LoginRequest(
                "no.existe@test.com",
                "Test123!");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        await SeedUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/auth/me");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ShouldReturnOk_WhenTokenIsValid()
    {
        // Arrange
        var user =
            await SeedUserAsync();

        var login =
            await LoginAsync(user);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // Act
        var response =
            await _client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var me =
            await response.Content
                .ReadFromJsonAsync<MeResponse>();

        me.Should().NotBeNull();

        me!.UserId
            .Should()
            .Be(user.UserId.ToString());

        me.CompanyId
            .Should()
            .Be(user.CompanyId.ToString());

        me.Email
            .Should()
            .Be(user.Email);

        me.FirstName
            .Should()
            .Be("Jose");

        me.LastName
            .Should()
            .Be("Rodriguez");

        me.Roles
            .Should()
            .Contain(ApplicationRoles.User);
    }


    // Private methods

    private async Task<TestUserData> SeedUserAsync()
    {
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<SicotycDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Limpiar dependencias.
        context.UserRoles.RemoveRange(
            context.UserRoles);

        context.Users.RemoveRange(
            context.Users);

        context.Companies.RemoveRange(
            context.Companies);

        context.Ubigeos.RemoveRange(
            context.Ubigeos);

        await context.SaveChangesAsync();

        var ubigeo =
            new Ubigeo(
                "150101",
                "LIMA",
                "LIMA",
                "LIMA",
                "LIMA",
                1,
                "COSTA");

        context.Ubigeos.Add(ubigeo);

        var company =
            new Company(
                "20123456789",
                "Empresa Login Test SAC",
                "Av. Test 123",
                "150101",
                "empresa@test.com");

        context.Companies.Add(company);

        await context.SaveChangesAsync();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                FirstName = "Jose",
                LastName = "Rodriguez",
                UserName = "login@test.com",
                Email = "login@test.com",
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
                ApplicationRoles.User);

        roleResult.Succeeded
            .Should()
            .BeTrue();

        return new TestUserData(
            user.Id,
            company.Id,
            user.Email!,
            "Test123!");
    }
    private async Task<LoginResult> LoginAsync(
    TestUserData user)
    {
        var request =
            new LoginRequest(
                user.Email,
                user.Password);

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResult>();

        result.Should().NotBeNull();

        return result!;
    }


    // Private Records
    private sealed record MeResponse(
    string UserId,
    string CompanyId,
    string Email,
    string FirstName,
    string LastName,
    string[] Roles);

    private sealed record TestUserData(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string Password);
}
