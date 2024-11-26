using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class CambiosSalesForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesForms_AspNetUsers_VendedorID",
                table: "SalesForms");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesForms_Products_ProductoID",
                table: "SalesForms");

            migrationBuilder.DropIndex(
                name: "IX_SalesForms_ProductoID",
                table: "SalesForms");

            migrationBuilder.DropIndex(
                name: "IX_SalesForms_VendedorID",
                table: "SalesForms");

            migrationBuilder.DropColumn(
                name: "ProductoID",
                table: "SalesForms");

            migrationBuilder.DropColumn(
                name: "VendedorID",
                table: "SalesForms");

            migrationBuilder.AlterColumn<string>(
                name: "SellerID",
                table: "SalesForms",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_ProductID",
                table: "SalesForms",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_SellerID",
                table: "SalesForms",
                column: "SellerID");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesForms_AspNetUsers_SellerID",
                table: "SalesForms",
                column: "SellerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesForms_Products_ProductID",
                table: "SalesForms",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesForms_AspNetUsers_SellerID",
                table: "SalesForms");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesForms_Products_ProductID",
                table: "SalesForms");

            migrationBuilder.DropIndex(
                name: "IX_SalesForms_ProductID",
                table: "SalesForms");

            migrationBuilder.DropIndex(
                name: "IX_SalesForms_SellerID",
                table: "SalesForms");

            migrationBuilder.AlterColumn<string>(
                name: "SellerID",
                table: "SalesForms",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<int>(
                name: "ProductoID",
                table: "SalesForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VendedorID",
                table: "SalesForms",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_ProductoID",
                table: "SalesForms",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_VendedorID",
                table: "SalesForms",
                column: "VendedorID");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesForms_AspNetUsers_VendedorID",
                table: "SalesForms",
                column: "VendedorID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesForms_Products_ProductoID",
                table: "SalesForms",
                column: "ProductoID",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
