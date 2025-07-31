using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Meal
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string URLPhoto { get; set; }
        public int ResturantID { get; set; }
        public int RestaurantMenuCategoryID { get; set; }
        // Navigation properties for relationships
        public virtual Restaurant Resturant { get; set; }
        public virtual RestaurantMenuCategory RestaurantMenuCategory { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
        public Meal()
        {
            OrderItems = new HashSet<OrderItem>();
            Inventories = new HashSet<Inventory>();
        }
        
    }
}
