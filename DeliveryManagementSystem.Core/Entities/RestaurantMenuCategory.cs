using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class RestaurantMenuCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ResturantID { get; set; }
        public string URLPhoto { get; set; }
        public virtual Resturant Resturant { get; set; }

    }
}
