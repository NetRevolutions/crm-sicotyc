using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jarasoft.Sicotyc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedApplicationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApplicationRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0001"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0001", "SuperAdministrador", "SUPERADMINISTRADOR" },
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0002"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0002", "AdministradorEmpresa", "ADMINISTRADOREMPRESA" },
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0003"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0003", "Usuario", "USUARIO" },
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0004"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0004", "Coordinador", "COORDINADOR" },
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0005"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0005", "Chofer", "CHOFER" },
                    { new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0006"), "e8b4b95e-7f0b-4c73-9e76-5e16bf7b0006", "Facturacion", "FACTURACION" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0001"));

            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0002"));

            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0003"));

            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0004"));

            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0005"));

            migrationBuilder.DeleteData(
                table: "ApplicationRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8b4b95e-7f0b-4c73-9e76-5e16bf7b0006"));
        }
    }
}
