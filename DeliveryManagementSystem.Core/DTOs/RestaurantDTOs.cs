using DeliveryManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public List<MealDTO> Meals { get; set; } = new List<MealDTO>();
        public List<RestaurantMenuCategoryDTO> restaurantMenuCategories { get; set; } = new List<RestaurantMenuCategoryDTO>();
    }

    // DTO for creating a new restaurant

public class CreateRestaurantDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryID must be greater than 0.")]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Url(ErrorMessage = "Invalid photo URL.")]
        public string URLPhoto { get; set; }

        [Range(0, 1000, ErrorMessage = "DeliveryFee must be between 0 and 1000.")]
        public decimal DeliveryFee { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum order amount must be non-negative.")]
        public int MinimumOrderAmount { get; set; }

        [Range(0, 300, ErrorMessage = "Preparation time must be between 0 and 300 minutes.")]
        public int PreparationTime { get; set; }

        [Required]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        public TimeSpan ClosingTime { get; set; }
    }


    // DTO for updating restaurant
    public class UpdateRestaurantDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryID must be a positive number.")]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [Url]
        public string URLPhoto { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal DeliveryFee { get; set; }

        [Required]
        [Range(1, 10000)]
        public int MinimumOrderAmount { get; set; }

        [Required]
        [Range(1, 240)]
        public int PreparationTime { get; set; }

        [Required]
        public TimeSpan OpeningTime { get; set; }

        [Required]
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

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        // Optional: Add navigation URLs (useful for API consumers)
        public string? NextPageUrl { get; set; }
        public string? PreviousPageUrl { get; set; }
    }

}