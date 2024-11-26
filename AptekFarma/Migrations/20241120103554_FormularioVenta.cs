using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class FormularioVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "formulario_venta");

            // Crear nueva columna en lugar de renombrarla
            migrationBuilder.AddColumn<int>(
                name: "CampannaID",
                table: "formulario_venta",
                nullable: true);

            // Copiar datos de la columna antigua a la nueva
            migrationBuilder.Sql(
                "UPDATE formulario_venta SET CampannaID = ProductID");

            // Eliminar la columna antigua
            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "formulario_venta");

            // Crear la nueva tabla como está definido originalmente
            migrationBuilder.CreateTable(
                name: "venta_campanna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PorductoCampannaID = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FormularioID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_campanna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venta_campanna_formulario_venta_FormularioID",
                        column: x => x.FormularioID,
                        principalTable: "formulario_venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_venta_campanna_producto_campanna_PorductoCampannaID",
                        column: x => x.PorductoCampannaID,
                        principalTable: "producto_campanna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "TotalPuntos",
                table: "formulario_venta",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_formulario_venta_campanna_CampannaID",
                table: "formulario_venta");

            migrationBuilder.DropTable(
                name: "venta_campanna");

            migrationBuilder.DropColumn(
                name: "TotalPuntos",
                table: "formulario_venta");

            migrationBuilder.RenameColumn(
                name: "CampannaID",
                table: "formulario_venta",
                newName: "ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_formulario_venta_CampannaID",
                table: "formulario_venta",
                newName: "IX_formulario_venta_ProductID");

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "formulario_venta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_formulario_venta_producto_venta_ProductID",
                table: "formulario_venta",
                column: "ProductID",
                principalTable: "producto_venta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
