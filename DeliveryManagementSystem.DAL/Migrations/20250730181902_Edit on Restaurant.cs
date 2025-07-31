using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditonRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ClosingTime",
                table: "Resturants",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Resturants",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinimumOrderAmount",
                table: "Resturants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpeningTime",
                table: "Resturants",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "PreparationTime",
                table: "Resturants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders",
                column: "ResturantID",
                principalTable: "Resturants",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClosingTime",
                table: "Resturants");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Resturants");

            migrationBuilder.DropColumn(
                name: "MinimumOrderAmount",
                table: "Resturants");

            migrationBuilder.DropColumn(
                name: "OpeningTime",
                table: "Resturants");

            migrationBuilder.DropColumn(
                name: "PreparationTime",
                table: "Resturants");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Resturants_ResturantID",
                table: "Orders",
                column: "ResturantID",
                principalTable: "Resturants",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
