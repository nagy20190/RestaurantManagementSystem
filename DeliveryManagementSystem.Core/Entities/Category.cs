using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public  class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        // Navigation properties for relationships
        public virtual ICollection<Restaurant> Restaurants { get; set; }
        public Category()
        {
            Restaurants = new HashSet<Restaurant>();
        }
    }
}
