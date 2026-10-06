using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jarasoft.Sicotyc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyAddressUbigeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Companies",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IdDist",
                table: "Companies",
                type: "char(6)",
                maxLength: 6,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IdDist",
                table: "Companies",
                column: "IdDist");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Ubigeos_IdDist",
                table: "Companies",
                column: "IdDist",
                principalTable: "Ubigeos",
                principalColumn: "IDDIST",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Ubigeos_IdDist",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_IdDist",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IdDist",
                table: "Companies");
        }
    }
}
