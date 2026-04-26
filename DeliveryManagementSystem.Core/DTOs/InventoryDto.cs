using System.ComponentModel.DataAnnotations;

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

     public class InventoryAdjustDto
     {
        [Required]
        [Range(-10000, 10000, ErrorMessage = "Delta must be between -10000 and 10000")]
        public int Delta { get; set; }
        public int RestaurantID { get; set; }
        public int MealID { get; set; }
    }


}
