using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Order
    {
        public int ID { get; set; }
        public string OrderNumber { get; set; }
        public int UserID { get; set; }

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string DeliveryAddress { get; set; }
        public decimal DeliveryFee { get; set; }
        public string PaymentMethod { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("DriverID")]
        public virtual User Driver { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        InPreparation,
        PickedUp,
        InTransit,
        OutForDelivery,
        Delivered,
        Cancelled,
    }
}