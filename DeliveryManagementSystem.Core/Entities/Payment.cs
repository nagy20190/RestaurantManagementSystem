using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Entities
{
    public class Payment
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } // e.g., Credit Card, Cash, etc.
        public PaymentStatus Status { get; set; } // e.g., Pending, Confirmed, Failed, etc.
        public int OrderID { get; set; } // Foreign key to Order
        public string TransactionID { get; set; } // Unique identifier for the transaction
       
        // Navigation property for relationships
        public virtual Order Order { get; set; }
        public Payment()
        {
            PaymentDate = DateTime.Now; // Default to current date and time
        }
    }
    public enum PaymentStatus
    {
        Pending,
        Confirmed,
        Failed,
        Cancelled,
        Refunded
    }
    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        Cash,
        MobilePayment,
        PayPal,
        ApplePay,
    }
}
