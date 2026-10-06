using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jarasoft.Sicotyc.Test.Common;

public sealed class SqlServerWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public SqlServerWebApplicationFactory(
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "Jwt:Issuer",
            "Jarasoft.Sicotyc.Test");

        builder.UseSetting(
            "Jwt:Audience",
            "Jarasoft.Sicotyc.Test");

        builder.UseSetting(
            "Jwt:SecretKey",
            "ThisIsOnlyATestJwtSecretKeyForIntegrationTests123456789");

        builder.UseSetting(
            "Jwt:ExpirationMinutes",
            "60");

        builder.ConfigureServices(services =>
        {
            // Eliminar la configuración original
            // de SicotycDbContext.
            services.RemoveAll<SicotycDbContext>();

            services.RemoveAll<
                DbContextOptions<SicotycDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<SicotycDbContext>>();

            // Configurar SQL Server para las
            // pruebas de integración.
            services.AddDbContext<SicotycDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        _connectionString);
                });

            // Utilizar el bloqueo real de SQL Server.
            services.RemoveAll<ICompanyAdministrationLock>();

            services.AddScoped<
                ICompanyAdministrationLock,
                CompanyAdministrationLock>();
        });
    }
}