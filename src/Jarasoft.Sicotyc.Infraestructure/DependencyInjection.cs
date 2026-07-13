using Jarasoft.Sicotyc.Application.Roles;
using Jarasoft.Sicotyc.Application.Users;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infraestructure.Persistence;
using Jarasoft.Sicotyc.Infraestructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Infraestructure;

public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "The SQL Server connection string 'ConnectionStrings:DefaultConnection' was not found. Store it in User Secrets or environment variables.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
                sqlServerOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IApplicationRoleQueryService, ApplicationRoleQueryService>();
        services.AddScoped<IApplicationUserQueryService, ApplicationUserQueryService>();
        services.AddScoped<ISystemUserRegistrationService, SystemUserRegistrationService>();

        return services;
    }
}
