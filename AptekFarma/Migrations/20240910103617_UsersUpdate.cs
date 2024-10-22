using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class UsersUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "apellidos",
                table: "Users",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fecha_nacimiento",
                table: "Users",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nif",
                table: "Users",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                table: "Users",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apellidos",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "fecha_nacimiento",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "nif",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "nombre",
                table: "Users");
        }
    }
}
