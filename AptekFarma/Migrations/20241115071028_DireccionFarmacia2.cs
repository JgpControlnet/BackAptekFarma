using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class DireccionFarmacia2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProvinciaID",
                table: "Localidades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Localidades_ProvinciaID",
                table: "Localidades",
                column: "ProvinciaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Localidades_Provincias_ProvinciaID",
                table: "Localidades",
                column: "ProvinciaID",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Localidades_Provincias_ProvinciaID",
                table: "Localidades");

            migrationBuilder.DropIndex(
                name: "IX_Localidades_ProvinciaID",
                table: "Localidades");

            migrationBuilder.DropColumn(
                name: "ProvinciaID",
                table: "Localidades");
        }
    }
}
