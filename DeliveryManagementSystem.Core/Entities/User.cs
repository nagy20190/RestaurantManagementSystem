using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public string? Address { get; set; }
        public string? CreditCardNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ProfileImageUrl { get; set; } = "1.png";

        // Navigation properties for relationships
         public virtual ICollection<Order> Orders { get; set; }
         public virtual ICollection<Review> Reviews { get; set; }
         public virtual ICollection<Reservation> Reservations { get; set; }

        public User()
        {
            Orders = new HashSet<Order>();
            Reviews = new HashSet<Review>();
            Reservations = new HashSet<Reservation>();
        }

    }
}
