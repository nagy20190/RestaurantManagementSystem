using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Reservation
    {
        public int ID { get; set; }
        public DateTime ReservationDate { get; set; }
        public int TableID { get; set; }
        public int UserID { get; set; }
        public int NumberOfPeople { get; set; }
        public string QRCode { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation properties for relationships
        public virtual Table Table { get; set; }
        public virtual User User { get; set; }
        
    }
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

}
