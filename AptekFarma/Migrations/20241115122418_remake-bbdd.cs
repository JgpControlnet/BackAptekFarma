using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class remakebbdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pharmacies_PharmacyID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacies_Localidades_LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacies_Provincias_ProvinciaID",
                table: "Pharmacies");

            migrationBuilder.DropTable(
                name: "Localidades");

            migrationBuilder.DropTable(
                name: "PointsEarned");

            migrationBuilder.DropTable(
                name: "PointsRedeemded");

            migrationBuilder.DropTable(
                name: "SalesForms");

            migrationBuilder.DropTable(
                name: "Provincias");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacies",
                table: "Pharmacies");

            migrationBuilder.DropIndex(
                name: "IX_Pharmacies_LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropIndex(
                name: "IX_Pharmacies_ProvinciaID",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "LocalidadID",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "ProvinciaID",
                table: "Pharmacies");

            migrationBuilder.RenameTable(
                name: "Pharmacies",
                newName: "pharmacy");

            migrationBuilder.AddColumn<string>(
                name: "Imagen",
                table: "pharmacy",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "pharmacy",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "pharmacy",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_pharmacy",
                table: "pharmacy",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "campanna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(type: "longtext", nullable: false),
                    Descripcion = table.Column<string>(type: "longtext", nullable: false),
                    Nombre = table.Column<string>(type: "longtext", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanna", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "producto_venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false),
                    CodProducto = table.Column<int>(type: "int", nullable: false),
                    Imagen = table.Column<string>(type: "longtext", nullable: false),
                    PuntosNeceseraios = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CantidadMax = table.Column<int>(type: "int", nullable: false),
                    Laboratorio = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_venta", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "producto_campanna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "longtext", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    UnidadesMaximas = table.Column<int>(type: "int", nullable: false),
                    Laboratorio = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_campanna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_producto_campanna_campanna_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "campanna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "formulario_venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formulario_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_formulario_venta_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_formulario_venta_producto_venta_ProductID",
                        column: x => x.ProductID,
                        principalTable: "producto_venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_formulario_venta_ProductID",
                table: "formulario_venta",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_formulario_venta_UserID",
                table: "formulario_venta",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_producto_campanna_CampaignId",
                table: "producto_campanna",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_pharmacy_PharmacyID",
                table: "AspNetUsers",
                column: "PharmacyID",
                principalTable: "pharmacy",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_pharmacy_PharmacyID",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "formulario_venta");

            migrationBuilder.DropTable(
                name: "producto_campanna");

            migrationBuilder.DropTable(
                name: "producto_venta");

            migrationBuilder.DropTable(
                name: "campanna");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pharmacy",
                table: "pharmacy");

            migrationBuilder.DropColumn(
                name: "Imagen",
                table: "pharmacy");

            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "pharmacy");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "pharmacy");

            migrationBuilder.RenameTable(
                name: "pharmacy",
                newName: "Pharmacies");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacies",
                table: "Pharmacies",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true),
                    FechaCaducidad = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PointsEarned",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsEarned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsEarned_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CodigoNacional = table.Column<string>(type: "longtext", nullable: false),
                    Imagen = table.Column<string>(type: "longtext", nullable: false),
                    Nombre = table.Column<string>(type: "longtext", nullable: false),
                    Precio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CampaignID = table.Column<int>(type: "int", nullable: false),
                    CodigoNacional = table.Column<string>(type: "longtext", nullable: false),
                    Nventas = table.Column<int>(type: "int", nullable: false),
                    PonderacionPuntos = table.Column<int>(type: "int", nullable: false),
                    Referencia = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Campaigns_CampaignID",
                        column: x => x.CampaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PointsRedeemded",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsRedeemded", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsRedeemded_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointsRedeemded_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Localidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProvinciaID = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localidades_Provincias_ProvinciaID",
                        column: x => x.ProvinciaID,
                        principalTable: "Provincias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalesForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    SaleID = table.Column<int>(type: "int", nullable: false),
                    SellerID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Validated = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesForms_AspNetUsers_SellerID",
                        column: x => x.SellerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesForms_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesForms_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_Localidades_ProvinciaID",
                table: "Localidades",
                column: "ProvinciaID");

            migrationBuilder.CreateIndex(
                name: "IX_PointsEarned_UserID",
                table: "PointsEarned",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PointsRedeemded_ProductID",
                table: "PointsRedeemded",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_PointsRedeemded_UserID",
                table: "PointsRedeemded",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CampaignID",
                table: "Sales",
                column: "CampaignID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_ProductID",
                table: "SalesForms",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_SaleID",
                table: "SalesForms",
                column: "SaleID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForms_SellerID",
                table: "SalesForms",
                column: "SellerID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pharmacies_PharmacyID",
                table: "AspNetUsers",
                column: "PharmacyID",
                principalTable: "Pharmacies",
                principalColumn: "Id");

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
    }
}
