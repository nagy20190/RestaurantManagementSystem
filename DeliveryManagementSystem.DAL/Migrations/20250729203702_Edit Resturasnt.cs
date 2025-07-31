using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditResturasnt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResturantID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ResturantID",
                table: "Orders",
                column: "ResturantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders",
                column: "ResturantID",
                principalTable: "Resturants",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ResturantID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ResturantID",
                table: "Orders");
        }
    }
}
