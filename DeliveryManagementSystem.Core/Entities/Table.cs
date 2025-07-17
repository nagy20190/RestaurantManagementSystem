using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Table
    {
        public int ID { get; set; }
        public int ResturantID { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        // Navigation property for relationships
        public virtual Resturant Resturant { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public Table()
        {
            IsAvailable = true; // Default value for availability
            Reservations = new HashSet<Reservation>(); // Initialize the collection to avoid null reference issues
        }


    }
}
