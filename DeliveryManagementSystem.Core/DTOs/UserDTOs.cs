using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic User DTO for general use
    public class UserDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    // DTO for creating a new user
    public class CreateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CreditCardNumber { get; set; }
    }

    // DTO for updating user information
    public class UpdateUserDTO
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CreditCardNumber { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    // DTO for user login
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO for user authentication response
    public class AuthResponseDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // DTO for user profile with related data
    public class UserProfileDTO : UserDTO
    {
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
        public int TotalReservations { get; set; }
        public List<OrderDTO> RecentOrders { get; set; }
        public List<ReviewDTO> RecentReviews { get; set; }
    }

    public enum Role
    {
        SuperAdmin,
        Admin,
        Customer
    }
}