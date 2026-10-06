using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jarasoft.Sicotyc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSunatFieldsToCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActualizadoDeSunat",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CondicionContribuyente",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoContribuyente",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacionSunat",
                table: "Companies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreComercial",
                table: "Companies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualizadoDeSunat",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CondicionContribuyente",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EstadoContribuyente",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FechaActualizacionSunat",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "NombreComercial",
                table: "Companies");
        }
    }
}
