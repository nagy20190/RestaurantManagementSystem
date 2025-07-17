using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Resturant
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string URLPhoto { get; set; }
        public int AverageRating { get; set; }
        // navigation properties for relationships
        public virtual Category Category { get; set; }
        public virtual ICollection<RestaurantMenuCategory> ResturantCategories { get; set; }
        public virtual ICollection<Table> Tables { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }


    }
}
