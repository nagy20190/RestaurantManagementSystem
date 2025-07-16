using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Meal DTO
    public class MealDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string URLPhoto { get; set; }
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public bool IsAvailable { get; set; }
        public int PreparationTime { get; set; } // in minutes
        public decimal? DiscountedPrice { get; set; }
        public bool IsPopular { get; set; }
    }

    // DTO for creating a new meal
    public class CreateMealDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string URLPhoto { get; set; }
        public int RestaurantID { get; set; }
        public int PreparationTime { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? MenuCategoryID { get; set; }
    }

    // DTO for updating meal
    public class UpdateMealDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string URLPhoto { get; set; }
        public int PreparationTime { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public bool IsAvailable { get; set; }
        public int? MenuCategoryID { get; set; }
    }

    // DTO for meal with ingredients/allergens
    public class MealDetailsDTO : MealDTO
    {
        public List<string> Ingredients { get; set; } //مكونات
        public List<string> Allergens { get; set; } // مسببات الحساسية
        public int Calories { get; set; } 
        public string NutritionalInfo { get; set; } // المعلومات الغذائية
        public int AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    // DTO for meal search/filtering
    public class MealSearchDTO
    {
        public string SearchTerm { get; set; }
        public int? RestaurantID { get; set; }
        public int? MenuCategoryID { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsAvailable { get; set; }
        public bool? HasDiscount { get; set; }
        public string SortBy { get; set; } // "price", "name", "popularity"
        public bool SortDescending { get; set; }
    }

    // DTO for popular meals
    public class PopularMealDTO : MealDTO
    {
        public int OrderCount { get; set; }
        public decimal AverageRating { get; set; }
    }
}