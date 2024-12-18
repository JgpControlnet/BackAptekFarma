using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class videoPdfCampanna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PDF",
                table: "campanna",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Video",
                table: "campanna",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PDF",
                table: "campanna");

            migrationBuilder.DropColumn(
                name: "Video",
                table: "campanna");
        }
    }
}
