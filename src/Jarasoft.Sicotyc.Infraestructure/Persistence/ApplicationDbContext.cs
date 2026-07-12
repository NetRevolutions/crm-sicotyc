using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infraestructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyType> CompanyTypes => Set<CompanyType>();
    public DbSet<CompanyZone> CompanyZones => Set<CompanyZone>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<FreightRate> FreightRates => Set<FreightRate>();
    public DbSet<LicenseType> LicenseTypes => Set<LicenseType>();
    public DbSet<MenuOption> MenuOptions => Set<MenuOption>();
    public DbSet<MenuOptionRole> MenuOptionRoles => Set<MenuOptionRole>();
    public DbSet<NegotiatedFreightRate> NegotiatedFreightRates => Set<NegotiatedFreightRate>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteTransportOffer> QuoteTransportOffers => Set<QuoteTransportOffer>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<UserDetail> UserDetails => Set<UserDetail>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
