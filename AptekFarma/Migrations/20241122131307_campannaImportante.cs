using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class campannaImportante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValido",
                table: "campanna",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Importante",
                table: "campanna",
                type: "longtext",
                nullable: false);

            migrationBuilder.AlterColumn<double>(
                name: "Points",
                table: "AspNetUsers",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaValido",
                table: "campanna");

            migrationBuilder.DropColumn(
                name: "Importante",
                table: "campanna");

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
