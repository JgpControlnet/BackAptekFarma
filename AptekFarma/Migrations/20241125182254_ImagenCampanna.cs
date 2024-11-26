using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class ImagenCampanna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imagen",
                table: "campanna",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagen",
                table: "campanna");
        }
    }
}
