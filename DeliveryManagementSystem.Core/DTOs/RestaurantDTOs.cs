using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Restaurant DTO
    public class RestaurantDTO
    {
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string URLPhoto { get; set; }
        public int AverageRating { get; set; }
        public bool IsOpen { get; set; }
        public decimal DeliveryFee { get; set; }
        public int MinimumOrderAmount { get; set; }
        public int PreparationTime { get; set; } // in minutes
    }

    // DTO for creating a new restaurant
    public class CreateRestaurantDTO
    {
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string URLPhoto { get; set; }
        public decimal DeliveryFee { get; set; }
        public int MinimumOrderAmount { get; set; }
        public int PreparationTime { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
    }

    // DTO for updating restaurant
    public class UpdateRestaurantDTO
    {
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string URLPhoto { get; set; }
        public decimal DeliveryFee { get; set; }
        public int MinimumOrderAmount { get; set; }
        public int PreparationTime { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
    }

    // DTO for restaurant with menu categories
    public class RestaurantWithMenuDTO : RestaurantDTO
    {
        public List<RestaurantMenuCategoryDTO> MenuCategories { get; set; }
        public List<MealDTO> PopularMeals { get; set; }
    }

    // DTO for restaurant details with reviews
    public class RestaurantDetailsDTO : RestaurantDTO
    {
        public List<ReviewDTO> Reviews { get; set; }
        public List<RestaurantMenuCategoryDTO> MenuCategories { get; set; }
        public List<TableDTO> Tables { get; set; }
        public int TotalReviews { get; set; }
        public decimal AverageRatingDecimal { get; set; }
    }

    // DTO for restaurant search/filtering
    public class RestaurantSearchDTO
    {
        public string SearchTerm { get; set; }
        public int? CategoryID { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxDeliveryFee { get; set; }
        public bool? IsOpen { get; set; }
        public string SortBy { get; set; } // "rating", "deliveryFee", "name"
        public bool SortDescending { get; set; }
    }
}