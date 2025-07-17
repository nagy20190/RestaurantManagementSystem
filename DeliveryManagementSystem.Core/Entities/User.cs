using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.Entities
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CreditCardNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ProfileImageUrl { get; set; }

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

    public enum Role
    {
        SuperAdmin,
        Admin,
        Customer,
    }
}
