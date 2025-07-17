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

        public int Quantity { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public Meal Meal { get; set; }

    }
}
