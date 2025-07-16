using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Restaurant Menu Category DTO
    public class RestaurantMenuCategoryDTO
    {
        public string Name { get; set; }
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string URLPhoto { get; set; }
        public int MealCount { get; set; }
    }

    // DTO for creating a new menu category
    public class CreateRestaurantMenuCategoryDTO
    {
        public string Name { get; set; }
        public int RestaurantID { get; set; }
        public string URLPhoto { get; set; }
    }

    // DTO for updating menu category
    public class UpdateRestaurantMenuCategoryDTO
    {
        public string Name { get; set; }
        public string URLPhoto { get; set; }
    }

    // DTO for menu category with meals
    public class MenuCategoryWithMealsDTO : RestaurantMenuCategoryDTO
    {
        public List<MealDTO> Meals { get; set; }
    }
}