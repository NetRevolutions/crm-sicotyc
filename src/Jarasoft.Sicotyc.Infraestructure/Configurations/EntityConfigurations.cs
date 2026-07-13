using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jarasoft.Sicotyc.Infraestructure.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");
    }
}

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");
    }
}

public sealed class CompanyConfiguration : EntityConfiguration<Company>
{
    public override void Configure(EntityTypeBuilder<Company> builder)
    {
        base.Configure(builder);
        builder.ToTable("Companies");
    }
}

public sealed class CompanyTypeConfiguration : EntityConfiguration<CompanyType>
{
    public override void Configure(EntityTypeBuilder<CompanyType> builder)
    {
        base.Configure(builder);
        builder.ToTable("CompanyTypes");
    }
}

public sealed class CompanyZoneConfiguration : EntityConfiguration<CompanyZone>
{
    public override void Configure(EntityTypeBuilder<CompanyZone> builder)
    {
        base.Configure(builder);
        builder.ToTable("CompanyZones");
    }
}

public sealed class DistrictConfiguration : EntityConfiguration<District>
{
    public override void Configure(EntityTypeBuilder<District> builder)
    {
        base.Configure(builder);
        builder.ToTable("Districts");
    }
}

public sealed class DriverConfiguration : EntityConfiguration<Driver>
{
    public override void Configure(EntityTypeBuilder<Driver> builder)
    {
        base.Configure(builder);
        builder.ToTable("Drivers");
    }
}

public sealed class FreightRateConfiguration : EntityConfiguration<FreightRate>
{
    public override void Configure(EntityTypeBuilder<FreightRate> builder)
    {
        base.Configure(builder);
        builder.ToTable("FreightRates");
    }
}

public sealed class LicenseTypeConfiguration : EntityConfiguration<LicenseType>
{
    public override void Configure(EntityTypeBuilder<LicenseType> builder)
    {
        base.Configure(builder);
        builder.ToTable("LicenseTypes");
    }
}

public sealed class MenuOptionConfiguration : IEntityTypeConfiguration<MenuOption>
{
    public void Configure(EntityTypeBuilder<MenuOption> builder)
    {
        builder.ToTable("MenuOptions");
        builder.HasKey(entity => entity.OptionId);
        builder.Property(entity => entity.OptionId).ValueGeneratedNever();
    }
}

public sealed class MenuOptionRoleConfiguration : IEntityTypeConfiguration<MenuOptionRole>
{
    public void Configure(EntityTypeBuilder<MenuOptionRole> builder)
    {
        builder.ToTable("MenuOptionRoles");
        builder.HasKey(entity => new { entity.OptionId, entity.RoleId });
    }
}

public sealed class NegotiatedFreightRateConfiguration : EntityConfiguration<NegotiatedFreightRate>
{
    public override void Configure(EntityTypeBuilder<NegotiatedFreightRate> builder)
    {
        base.Configure(builder);
        builder.ToTable("NegotiatedFreightRates");
    }
}

public sealed class QuoteConfiguration : EntityConfiguration<Quote>
{
    public override void Configure(EntityTypeBuilder<Quote> builder)
    {
        base.Configure(builder);
        builder.ToTable("Quotes");
    }
}

public sealed class QuoteTransportOfferConfiguration : EntityConfiguration<QuoteTransportOffer>
{
    public override void Configure(EntityTypeBuilder<QuoteTransportOffer> builder)
    {
        base.Configure(builder);
        builder.ToTable("QuoteTransportOffers");
    }
}

public sealed class ServiceTypeConfiguration : EntityConfiguration<ServiceType>
{
    public override void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        base.Configure(builder);
        builder.ToTable("ServiceTypes");
    }
}

public sealed class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable("UserCompanies");
        builder.HasKey(entity => new { entity.UserId, entity.CompanyId });
    }
}

public sealed class UserDetailConfiguration : EntityConfiguration<UserDetail>
{
    public override void Configure(EntityTypeBuilder<UserDetail> builder)
    {
        base.Configure(builder);
        builder.ToTable("UserDetails");
    }
}

public sealed class VehicleConfiguration : EntityConfiguration<Vehicle>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        base.Configure(builder);
        builder.ToTable("Vehicles");
    }
}

public sealed class VehicleTypeConfiguration : EntityConfiguration<VehicleType>
{
    public override void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        base.Configure(builder);
        builder.ToTable("VehicleTypes");
    }
}

public sealed class WarehouseConfiguration : EntityConfiguration<Warehouse>
{
    public override void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        base.Configure(builder);
        builder.ToTable("Warehouses");
    }
}

public sealed class ZoneConfiguration : EntityConfiguration<Zone>
{
    public override void Configure(EntityTypeBuilder<Zone> builder)
    {
        base.Configure(builder);
        builder.ToTable("Zones");
    }
}
