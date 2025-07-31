using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditMealsRestaurantMenuCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantMenuCategoryID",
                table: "Meals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_RestaurantMenuCategoryID",
                table: "Meals",
                column: "RestaurantMenuCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_RestaurantMenuCategories_RestaurantMenuCategoryID",
                table: "Meals",
                column: "RestaurantMenuCategoryID",
                principalTable: "RestaurantMenuCategories",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_RestaurantMenuCategories_RestaurantMenuCategoryID",
                table: "Meals");

            migrationBuilder.DropIndex(
                name: "IX_Meals_RestaurantMenuCategoryID",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "RestaurantMenuCategoryID",
                table: "Meals");
        }
    }
}
