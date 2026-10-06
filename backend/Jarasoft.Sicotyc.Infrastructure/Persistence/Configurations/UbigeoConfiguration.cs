using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence.Configurations;

public sealed class UbigeoConfiguration
    : IEntityTypeConfiguration<Ubigeo>
{
    public void Configure(EntityTypeBuilder<Ubigeo> builder)
    {
        builder.ToTable("Ubigeos");

        builder.HasKey(x => x.IdDist);

        builder.Property(x => x.IdDist)
            .HasColumnName("IDDIST")
            .HasColumnType("char(6)")
            .IsRequired();

        builder.Property(x => x.NombreDepartamento)
            .HasColumnName("NOMBDEP")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.NombreProvincia)
            .HasColumnName("NOMBPROV")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.NombreDistrito)
            .HasColumnName("NOMBDIST")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.NombreCapital)
            .HasColumnName("NOM_CAPITAL")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.CodigoRegionNatural)
            .HasColumnName("COD_REG_NAT")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.RegionNatural)
            .HasColumnName("REGION_NATURAL")
            .HasColumnType("varchar(50)")
            .IsRequired();
    }
}