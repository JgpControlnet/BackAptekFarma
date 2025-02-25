using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false),
                    Ruta = table.Column<string>(type: "longtext", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videos", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "campanna",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_campanna_VideoId",
                table: "campanna",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_campanna_videos_VideoId",
                table: "campanna",
                column: "VideoId",
                principalTable: "videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campanna_videos_VideoId",
                table: "campanna");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropIndex(
                name: "IX_campanna_VideoId",
                table: "campanna");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "campanna");
        }
    }
}
