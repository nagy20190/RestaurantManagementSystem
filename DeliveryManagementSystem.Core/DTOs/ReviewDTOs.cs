using DeliveryManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Review DTO
    public class ReviewDTO
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserProfileImage { get; set; }
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public int HelpfulCount { get; set; }
        public int UnhelpfulCount { get; set; }
    }

    // DTO for creating a new review

    public class CreateReviewDTO
    {


        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        // Optional: You can add a custom validation later if needed.
        public int? OrderID { get; set; }
    }


    // DTO for updating review
    public class UpdateReviewDTO
    {
        public string Comment { get; set; }
        public int Rating { get; set; }
    }

    // DTO for review with order details
    public class ReviewWithOrderDTO : ReviewDTO
    {
        public OrderDTO Order { get; set; }
        public List<MealDTO> OrderedMeals { get; set; }
    }

    // DTO for restaurant reviews
    public class RestaurantReviewsDTO
    {
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDTO> Reviews { get; set; }
    }

    // DTO for review search/filtering
    public class ReviewSearchDTO
    {
        public int? RestaurantID { get; set; }
        public int? UserID { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsVerified { get; set; }
        public string SortBy { get; set; } // "date", "rating", "helpful"
        public bool SortDescending { get; set; }
    }

    // DTO for review helpful/unhelpful
    public class ReviewFeedbackDTO
    {
        public int ReviewID { get; set; }
        public int UserID { get; set; }
        public bool IsHelpful { get; set; }
    }
}