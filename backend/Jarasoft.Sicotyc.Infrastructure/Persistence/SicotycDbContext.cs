using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence;

public class SicotycDbContext 
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public SicotycDbContext(
        DbContextOptions<SicotycDbContext> options)
        : base(options)
    {
    }

    // Add DbSet properties for your entities here
    public DbSet<Ubigeo> Ubigeos => Set<Ubigeo>();
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SicotycDbContext).Assembly);
    }
}