using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jarasoft.Sicotyc.Infraestructure.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");

        builder.HasMany(entity => entity.Users)
            .WithOne(entity => entity.ApplicationRole)
            .HasForeignKey(entity => entity.ApplicationRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(entity => entity.MenuOptionRoles)
            .WithOne(entity => entity.Role)
            .HasForeignKey(entity => entity.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");
        builder.Ignore(entity => entity.UserDetailId);
        builder.Property(entity => entity.Img).HasMaxLength(512);

        builder.OwnsOne(entity => entity.FirstName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.MiddleName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("MiddleName")
                .HasMaxLength(100);
        });

        builder.OwnsOne(entity => entity.LastName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.MaidenName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("MaidenName")
                .HasMaxLength(100);
        });

        builder.OwnsOne(entity => entity.DocumentIdentity, owned =>
        {
            owned.Property(value => value.DocumentType)
                .HasColumnName("DocumentType")
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            owned.Property(value => value.DocumentNumber)
                .HasColumnName("DocumentNumber")
                .HasMaxLength(50)
                .IsRequired();
        });

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.UserDetail)
            .WithOne(entity => entity.User)
            .HasForeignKey<UserDetail>(entity => entity.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Quotes)
            .WithOne(entity => entity.RequestedByUser)
            .HasForeignKey(entity => entity.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.QuoteTransportOffers)
            .WithOne(entity => entity.RequestedByUser)
            .HasForeignKey(entity => entity.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.UserCompanies)
            .WithOne(entity => entity.User)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CompanyConfiguration : EntityConfiguration<Company>
{
    public override void Configure(EntityTypeBuilder<Company> builder)
    {
        base.Configure(builder);
        builder.ToTable("Companies");

        builder.OwnsOne(entity => entity.BusinessName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("BusinessName")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.TradeName, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("TradeName")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.RUC, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("RUC")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.Address, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Address")
                .HasMaxLength(300)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.Email, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.Phone, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Phone")
                .HasMaxLength(50)
                .IsRequired();
        });

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.Companies)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.CompanyZones)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Districts)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(entity => entity.Drivers)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.FreightRates)
            .WithOne(entity => entity.TransportCompany)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.ClientNegotiatedFreightRates)
            .WithOne(entity => entity.ClientCompany)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.TransportNegotiatedFreightRates)
            .WithOne(entity => entity.TransportCompany)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Quotes)
            .WithOne(entity => entity.ClientCompany)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.ClientQuoteTransportOffers)
            .WithOne(entity => entity.ClientCompany)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.TransportQuoteTransportOffers)
            .WithOne(entity => entity.TransportCompany)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.UserCompanies)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Vehicles)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Warehouses)
            .WithOne(entity => entity.Company)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CompanyTypeConfiguration : EntityConfiguration<CompanyType>
{
    public override void Configure(EntityTypeBuilder<CompanyType> builder)
    {
        base.Configure(builder);
        builder.ToTable("CompanyTypes");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(500);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasMany(entity => entity.CompanyZones)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Drivers)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.FreightRates)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.NegotiatedFreightRates)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Quotes)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.QuoteTransportOffers)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Vehicles)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Warehouses)
            .WithOne(entity => entity.CompanyType)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CompanyZoneConfiguration : EntityConfiguration<CompanyZone>
{
    public override void Configure(EntityTypeBuilder<CompanyZone> builder)
    {
        base.Configure(builder);
        builder.ToTable("CompanyZones");

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.CompanyZones)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.CompanyZones)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Zone)
            .WithMany(entity => entity.CompanyZones)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DistrictConfiguration : EntityConfiguration<District>
{
    public override void Configure(EntityTypeBuilder<District> builder)
    {
        base.Configure(builder);
        builder.ToTable("Districts");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Ubigeo).HasMaxLength(32).IsRequired();

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.Districts)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(entity => entity.Warehouses)
            .WithOne(entity => entity.District)
            .HasForeignKey(entity => entity.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DriverConfiguration : EntityConfiguration<Driver>
{
    public override void Configure(EntityTypeBuilder<Driver> builder)
    {
        base.Configure(builder);
        builder.ToTable("Drivers");
        builder.Property(entity => entity.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.LastName).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(entity => entity.LicenseNumber).HasMaxLength(50).IsRequired();

        builder.OwnsOne(entity => entity.Phone, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Phone")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.Email, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
        });

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.Drivers)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.Drivers)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.LicenseType)
            .WithMany(entity => entity.Drivers)
            .HasForeignKey(entity => entity.LicenseTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FreightRateConfiguration : EntityConfiguration<FreightRate>
{
    public override void Configure(EntityTypeBuilder<FreightRate> builder)
    {
        base.Configure(builder);
        builder.ToTable("FreightRates");
        builder.Property(entity => entity.Price).HasPrecision(18, 2);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.TransportCompany)
            .WithMany(entity => entity.FreightRates)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.FreightRates)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ServiceType)
            .WithMany(entity => entity.FreightRates)
            .HasForeignKey(entity => entity.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.OriginZone)
            .WithMany(entity => entity.OriginFreightRates)
            .HasForeignKey(entity => entity.OriginZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.DestinationZone)
            .WithMany(entity => entity.DestinationFreightRates)
            .HasForeignKey(entity => entity.DestinationZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.VehicleType)
            .WithMany(entity => entity.FreightRates)
            .HasForeignKey(entity => entity.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.NegotiatedFreightRates)
            .WithOne(entity => entity.FreightRate)
            .HasForeignKey(entity => entity.FreightRateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LicenseTypeConfiguration : EntityConfiguration<LicenseType>
{
    public override void Configure(EntityTypeBuilder<LicenseType> builder)
    {
        base.Configure(builder);
        builder.ToTable("LicenseTypes");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(50).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(500);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);
    }
}

public sealed class MenuOptionConfiguration : IEntityTypeConfiguration<MenuOption>
{
    public void Configure(EntityTypeBuilder<MenuOption> builder)
    {
        builder.ToTable("MenuOptions");
        builder.HasKey(entity => entity.OptionId);
        builder.Property(entity => entity.OptionId).ValueGeneratedNever();
        builder.Property(entity => entity.Title).HasMaxLength(150);
        builder.Property(entity => entity.Icon).HasMaxLength(150);
        builder.Property(entity => entity.Url).HasMaxLength(300);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasMany(entity => entity.MenuOptionRoles)
            .WithOne(entity => entity.Option)
            .HasForeignKey(entity => entity.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MenuOptionRoleConfiguration : IEntityTypeConfiguration<MenuOptionRole>
{
    public void Configure(EntityTypeBuilder<MenuOptionRole> builder)
    {
        builder.ToTable("MenuOptionRoles");
        builder.HasKey(entity => new { entity.OptionId, entity.RoleId });

        builder.HasOne(entity => entity.Option)
            .WithMany(entity => entity.MenuOptionRoles)
            .HasForeignKey(entity => entity.OptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Role)
            .WithMany(entity => entity.MenuOptionRoles)
            .HasForeignKey(entity => entity.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NegotiatedFreightRateConfiguration : EntityConfiguration<NegotiatedFreightRate>
{
    public override void Configure(EntityTypeBuilder<NegotiatedFreightRate> builder)
    {
        base.Configure(builder);
        builder.ToTable("NegotiatedFreightRates");
        builder.Property(entity => entity.SpecialPrice).HasPrecision(18, 2);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.FreightRate)
            .WithMany(entity => entity.NegotiatedFreightRates)
            .HasForeignKey(entity => entity.FreightRateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.TransportCompany)
            .WithMany(entity => entity.TransportNegotiatedFreightRates)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.NegotiatedFreightRates)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ServiceType)
            .WithMany(entity => entity.NegotiatedFreightRates)
            .HasForeignKey(entity => entity.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.OriginZone)
            .WithMany(entity => entity.OriginNegotiatedFreightRates)
            .HasForeignKey(entity => entity.OriginZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.DestinationZone)
            .WithMany(entity => entity.DestinationNegotiatedFreightRates)
            .HasForeignKey(entity => entity.DestinationZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.VehicleType)
            .WithMany(entity => entity.NegotiatedFreightRates)
            .HasForeignKey(entity => entity.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ClientCompany)
            .WithMany(entity => entity.ClientNegotiatedFreightRates)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class QuoteConfiguration : EntityConfiguration<Quote>
{
    public override void Configure(EntityTypeBuilder<Quote> builder)
    {
        base.Configure(builder);
        builder.ToTable("Quotes");
        builder.Property(entity => entity.EstimatedWeightTons).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AdditionalNotes).HasMaxLength(1000);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.RequestedByUser)
            .WithMany(entity => entity.Quotes)
            .HasForeignKey(entity => entity.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ClientCompany)
            .WithMany(entity => entity.Quotes)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.Quotes)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ServiceType)
            .WithMany(entity => entity.Quotes)
            .HasForeignKey(entity => entity.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.QuoteTransportOffers)
            .WithOne(entity => entity.Quote)
            .HasForeignKey(entity => entity.QuoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class QuoteTransportOfferConfiguration : EntityConfiguration<QuoteTransportOffer>
{
    public override void Configure(EntityTypeBuilder<QuoteTransportOffer> builder)
    {
        base.Configure(builder);
        builder.ToTable("QuoteTransportOffers");
        builder.Property(entity => entity.ProposedFreight).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasMaxLength(64).IsRequired();

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Quote)
            .WithMany(entity => entity.QuoteTransportOffers)
            .HasForeignKey(entity => entity.QuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.RequestedByUser)
            .WithMany(entity => entity.QuoteTransportOffers)
            .HasForeignKey(entity => entity.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ClientCompany)
            .WithMany(entity => entity.ClientQuoteTransportOffers)
            .HasForeignKey(entity => entity.ClientCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.QuoteTransportOffers)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ServiceType)
            .WithMany(entity => entity.QuoteTransportOffers)
            .HasForeignKey(entity => entity.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.TransportCompany)
            .WithMany(entity => entity.TransportQuoteTransportOffers)
            .HasForeignKey(entity => entity.TransportCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ServiceTypeConfiguration : EntityConfiguration<ServiceType>
{
    public override void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        base.Configure(builder);
        builder.ToTable("ServiceTypes");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(50).IsRequired();

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);
    }
}

public sealed class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable("UserCompanies");
        builder.HasKey(entity => new { entity.UserId, entity.CompanyId });

        builder.HasOne(entity => entity.User)
            .WithMany(entity => entity.UserCompanies)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.UserCompanies)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class UserDetailConfiguration : EntityConfiguration<UserDetail>
{
    public override void Configure(EntityTypeBuilder<UserDetail> builder)
    {
        base.Configure(builder);
        builder.ToTable("UserDetails");
        builder.Property(entity => entity.Id).HasColumnName("UserId").ValueGeneratedNever();
        builder.Ignore(entity => entity.UserId);

        builder.OwnsOne(entity => entity.DateOfBirth, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("DateOfBirth")
                .IsRequired();
        });

        builder.OwnsOne(entity => entity.Address, owned =>
        {
            owned.Property(value => value.Value)
                .HasColumnName("Address")
                .HasMaxLength(300)
                .IsRequired();
        });

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);
    }
}

public sealed class VehicleConfiguration : EntityConfiguration<Vehicle>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        base.Configure(builder);
        builder.ToTable("Vehicles");
        builder.Property(entity => entity.PlateNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Brand).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Model).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.MaxWeight).HasPrecision(18, 2);
        builder.Property(entity => entity.MaxVolumeM3).HasPrecision(18, 2);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.Vehicles)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.Vehicles)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.VehicleType)
            .WithMany(entity => entity.Vehicles)
            .HasForeignKey(entity => entity.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VehicleTypeConfiguration : EntityConfiguration<VehicleType>
{
    public override void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        base.Configure(builder);
        builder.ToTable("VehicleTypes");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);
    }
}

public sealed class WarehouseConfiguration : EntityConfiguration<Warehouse>
{
    public override void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        base.Configure(builder);
        builder.ToTable("Warehouses");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Address).HasMaxLength(300).IsRequired();

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);

        builder.HasOne(entity => entity.Company)
            .WithMany(entity => entity.Warehouses)
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CompanyType)
            .WithMany(entity => entity.Warehouses)
            .HasForeignKey(entity => entity.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.District)
            .WithMany(entity => entity.Warehouses)
            .HasForeignKey(entity => entity.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ZoneConfiguration : EntityConfiguration<Zone>
{
    public override void Configure(EntityTypeBuilder<Zone> builder)
    {
        base.Configure(builder);
        builder.ToTable("Zones");
        builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(500);

        ConfigurationHelpers.ConfigureTracking(builder, entity => entity.Tracking);
    }
}

internal static class ConfigurationHelpers
{
    public static void ConfigureTracking<TEntity>(EntityTypeBuilder<TEntity> builder, System.Linq.Expressions.Expression<System.Func<TEntity, Tracking?>> expression)
        where TEntity : class
    {
        builder.OwnsOne(expression, owned =>
        {
            owned.Property(value => value.CreatedAt).HasColumnName("CreatedAt").IsRequired();
            owned.Property(value => value.CreatedBy).HasColumnName("CreatedBy");
            owned.Property(value => value.UpdatedAt).HasColumnName("UpdatedAt");
            owned.Property(value => value.UpdatedBy).HasColumnName("UpdatedBy");
            owned.Property(value => value.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
            owned.Property(value => value.DeletedAt).HasColumnName("DeletedAt");
            owned.Property(value => value.DeletedBy).HasColumnName("DeletedBy");
        });
    }
}
