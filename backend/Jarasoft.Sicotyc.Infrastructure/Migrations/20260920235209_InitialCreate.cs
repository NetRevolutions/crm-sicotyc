using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jarasoft.Sicotyc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ubigeos",
                columns: table => new
                {
                    IDDIST = table.Column<string>(type: "char(6)", nullable: false),
                    NOMBDEP = table.Column<string>(type: "varchar(50)", nullable: false),
                    NOMBPROV = table.Column<string>(type: "varchar(50)", nullable: false),
                    NOMBDIST = table.Column<string>(type: "varchar(50)", nullable: false),
                    NOM_CAPITAL = table.Column<string>(type: "varchar(50)", nullable: false),
                    COD_REG_NAT = table.Column<int>(type: "int", nullable: false),
                    REGION_NATURAL = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubigeos", x => x.IDDIST);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ubigeos");
        }
    }
}
