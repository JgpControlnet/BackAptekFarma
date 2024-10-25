using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AptekFarma.Migrations
{
    /// <inheritdoc />
    public partial class puntos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "Points",
            //    table: "AspNetUsers",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<bool>(
            //    name: "RememberMe",
            //    table: "AspNetUsers",
            //    type: "tinyint(1)",
            //    nullable: false,
            //    defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CampaignID",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CampaignID",
                table: "Sales",
                column: "CampaignID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Campaigns_CampaignID",
                table: "Sales",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pharmacies_PharmacyID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Campaigns_CampaignID",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CampaignID",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CampaignID",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Validated",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RememberMe",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "PharmacyID",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pharmacies_PharmacyID",
                table: "AspNetUsers",
                column: "PharmacyID",
                principalTable: "Pharmacies",
                principalColumn: "Id");
        }
    }
}
