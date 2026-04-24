using DeliveryManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.DTOs
{
    public class InventoryDto
    {
        public int MealID { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public RestaurantDTO Restaurant { get; set; }
        public MealDTO Meal { get; set; }
    }
    public class InventoryCreateDto
    {
        public int MealID { get; set; }
        public int RestaurantID { get; set; }
        public int Quantity { get; set; }
    }
    public class InventoryUpdateDto
    {
        public int MealID { get; set; }
        public int RestaurantID { get; set; }
        public int Quantity { get; set; }
        // public bool IsAvailable { get; set; }
        // functionality to update availability can be added later if needed

    }
}
