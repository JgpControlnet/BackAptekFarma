using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class Estados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoFormularioID",
                table: "formulario_venta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstadoCampannaId",
                table: "campanna",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "estado_campanna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    estado = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado_campanna", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estado_formulario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    estado = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado_formulario", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_formulario_venta_EstadoFormularioID",
                table: "formulario_venta",
                column: "EstadoFormularioID");

            migrationBuilder.CreateIndex(
                name: "IX_campanna_EstadoCampannaId",
                table: "campanna",
                column: "EstadoCampannaId");

            migrationBuilder.AddForeignKey(
                name: "FK_campanna_estado_campanna_EstadoCampannaId",
                table: "campanna",
                column: "EstadoCampannaId",
                principalTable: "estado_campanna",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_formulario_venta_estado_formulario_EstadoFormularioID",
                table: "formulario_venta",
                column: "EstadoFormularioID",
                principalTable: "estado_formulario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campanna_estado_campanna_EstadoCampannaId",
                table: "campanna");

            migrationBuilder.DropForeignKey(
                name: "FK_formulario_venta_estado_formulario_EstadoFormularioID",
                table: "formulario_venta");

            migrationBuilder.DropTable(
                name: "estado_campanna");

            migrationBuilder.DropTable(
                name: "estado_formulario");

            migrationBuilder.DropIndex(
                name: "IX_formulario_venta_EstadoFormularioID",
                table: "formulario_venta");

            migrationBuilder.DropIndex(
                name: "IX_campanna_EstadoCampannaId",
                table: "campanna");

            migrationBuilder.DropColumn(
                name: "EstadoFormularioID",
                table: "formulario_venta");

            migrationBuilder.DropColumn(
                name: "EstadoCampannaId",
                table: "campanna");
        }
    }
}
