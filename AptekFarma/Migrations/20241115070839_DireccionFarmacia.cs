using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class DireccionFarmacia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CP",
                table: "Pharmacies",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalidadID",
                table: "Pharmacies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProvinciaID",
                table: "Pharmacies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Localidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidades", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Provincias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provincias", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_LocalidadID",
                table: "Pharmacies",
                column: "LocalidadID");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_ProvinciaID",
                table: "Pharmacies",
                column: "ProvinciaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacies_Localidades_LocalidadID",
                table: "Pharmacies",
                column: "LocalidadID",
                principalTable: "Localidades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacies_Provincias_ProvinciaID",
                table: "Pharmacies",
                column: "ProvinciaID",
                principalTable: "Provincias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacies_Localidades_LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacies_Provincias_ProvinciaID",
                table: "Pharmacies");

            migrationBuilder.DropTable(
                name: "Localidades");

            migrationBuilder.DropTable(
                name: "Provincias");

            migrationBuilder.DropIndex(
                name: "IX_Pharmacies_LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropIndex(
                name: "IX_Pharmacies_ProvinciaID",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "CP",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "ProvinciaID",
                table: "Pharmacies");
        }
    }
}
