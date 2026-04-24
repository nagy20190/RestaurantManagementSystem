using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Inventory
    {
        public int ID { get; set; }
        public int MealID { get; set; }
        public int RestaurantID { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        //public bool IsAvailable { get; set; } = true;  
        //public string Unit { get; set; }  // "kg", "liters", "pieces"
        //public string Name { get; set; }
        public Meal Meal { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
