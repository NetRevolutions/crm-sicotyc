using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence.Configurations
{
    public sealed class CompanyConfiguration
    : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(x => x.Ruc)
                .HasColumnType("varchar(11)")
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(x => x.NombreEmpresa)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.NombreComercial)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200);

            builder.Property(x => x.EstadoContribuyente)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.CondicionContribuyente)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.ActualizadoDeSunat)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.FechaActualizacionSunat)
                .HasColumnType("datetime2");

            builder.Property(x => x.Direccion)
                .HasColumnType("nvarchar(300)")
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.IdDist)
                .HasColumnType("char(6)")
                .HasMaxLength(6)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(x => x.IsActive)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasIndex(x => x.Ruc)
                .IsUnique()
                .HasDatabaseName("UX_Companies_Ruc");

            builder.HasOne(x => x.Ubigeo)
                .WithMany()
                .HasForeignKey(x => x.IdDist)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
