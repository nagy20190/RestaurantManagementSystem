using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Review
    {
        public int ID { get; set; }
        public int UserID { get; set; } // Foreign key to User
        public int ResturantID { get; set; } // Foreign key to Resturant
        public string Comment { get; set; }
        public int Rating { get; set; } // Rating out of 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation properties for relationships
        public virtual User User { get; set; }
        public virtual Resturant Resturant { get; set; }
        
    }
}
