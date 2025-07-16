using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Category DTO
    public class CategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int RestaurantCount { get; set; }
    }

    // DTO for creating a new category
    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }

    // DTO for updating category
    public class UpdateCategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }

   
}