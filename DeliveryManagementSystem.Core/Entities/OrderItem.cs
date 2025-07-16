using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class OrderItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public int MealId { get; set; }
        public int OrderId { get; set; }

        // Navigation properties for relationships
        public virtual Meal Meal { get; set; }
        public virtual Order Order { get; set; }

    }
}
