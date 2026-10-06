using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Jarasoft.Sicotyc.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Test.Infrastructure.Identity;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task CountActiveUsersInRoleAsync_ShouldFilterCompanyStatusAndNormalizedRole()
    {
        using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SicotycDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var ubigeo = new Ubigeo("999999", "Test", "Test", "Test", "Test", 1, "Test");
        context.Ubigeos.Add(ubigeo);
        var firstCompany = new Company("20999999991", "Count A", "Test", ubigeo.IdDist);
        var secondCompany = new Company("20999999992", "Count B", "Test", ubigeo.IdDist);
        context.Companies.AddRange(firstCompany, secondCompany);
        await context.SaveChangesAsync();

        await CreateUserAsync(manager, firstCompany.Id, ApplicationRoles.Administrator, true);
        await CreateUserAsync(manager, firstCompany.Id, ApplicationRoles.Administrator, true);
        await CreateUserAsync(manager, firstCompany.Id, ApplicationRoles.Administrator, false);
        await CreateUserAsync(manager, firstCompany.Id, ApplicationRoles.Operations, true);
        await CreateUserAsync(manager, secondCompany.Id, ApplicationRoles.Administrator, true);
        await CreateUserAsync(manager, secondCompany.Id, ApplicationRoles.Administrator, false);

        Assert.Equal(2, await identity.CountActiveUsersInRoleAsync(
            firstCompany.Id, ApplicationRoles.Administrator.ToLowerInvariant()));
        Assert.Equal(1, await identity.CountActiveUsersInRoleAsync(
            secondCompany.Id, ApplicationRoles.Administrator));
        Assert.Equal(1, await identity.CountActiveUsersInRoleAsync(
            firstCompany.Id, ApplicationRoles.Operations));
        Assert.Equal(0, await identity.CountActiveUsersInRoleAsync(Guid.NewGuid(),
            ApplicationRoles.Administrator));
        Assert.Equal(0, await identity.CountActiveUsersInRoleAsync(firstCompany.Id, "UnknownRole"));

        // El recuento debe leer las escrituras de la transacción del mismo scope
        // y volver a contar el estado original después de revertirla.
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<Jarasoft.Sicotyc.Application.Abstractions.Persistence.IUnitOfWork>();
        await unitOfWork.BeginTransactionAsync();
        try
        {
            await context.Users.Where(x => x.CompanyId == firstCompany.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsActive, false));
            Assert.Equal(0, await identity.CountActiveUsersInRoleAsync(
                firstCompany.Id, ApplicationRoles.Administrator));
            Assert.Equal(1, await identity.CountActiveUsersInRoleAsync(
                secondCompany.Id, ApplicationRoles.Administrator));
        }
        finally
        {
            await unitOfWork.RollbackTransactionAsync();
        }

        Assert.Equal(2, await identity.CountActiveUsersInRoleAsync(
            firstCompany.Id, ApplicationRoles.Administrator));
    }

    [Fact]
    public async Task CountActiveUsersInRoleAsync_ShouldPropagateCancellation()
    {
        using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            identity.CountActiveUsersInRoleAsync(Guid.NewGuid(),
                ApplicationRoles.Administrator, cancellation.Token));
    }

    private static async Task CreateUserAsync(UserManager<ApplicationUser> manager,
        Guid companyId, string role, bool isActive)
    {
        var email = $"count.{Guid.NewGuid():N}@test.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), CompanyId = companyId, IsActive = isActive,
            FirstName = "Count", LastName = "Test", UserName = email, Email = email
        };
        var created = await manager.CreateAsync(user, "Test123!");
        Assert.True(created.Succeeded, string.Join("; ", created.Errors.Select(x => x.Description)));
        var assigned = await manager.AddToRoleAsync(user, role);
        Assert.True(assigned.Succeeded, string.Join("; ", assigned.Errors.Select(x => x.Description)));
    }
}
