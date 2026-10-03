using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Test.Common;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

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
            // Remover completamente la configuración
            // original de SicotycDbContext (SQL Server).
            services.RemoveAll<SicotycDbContext>();

            services.RemoveAll<
                DbContextOptions<SicotycDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<SicotycDbContext>>();

            // SQLite In-Memory
            _connection =
                new SqliteConnection("DataSource=:memory:");

            _connection.Open();

            var connection = _connection;

            services.AddDbContext<SicotycDbContext>(
                options =>
                {
                    options.UseSqlite(connection);
                });

            services.RemoveAll<ICompanyAdministrationLock>();

            services.AddScoped<
                ICompanyAdministrationLock,
                TestCompanyAdministrationLock>();
        });        
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}