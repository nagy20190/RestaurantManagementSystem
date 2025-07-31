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
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public int OrderID { get; set; }
        public string TransactionID { get; set; } = string.Empty;

        // Additional fields for better tracking
        public string? PaymentToken { get; set; } // For tokenized payments
        public string? BillingAddress { get; set; }
        public string? GatewayResponse { get; set; } // Response from payment gateway
        public string? GatewayTransactionId { get; set; } // Gateway's transaction ID
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int CreatedBy { get; set; } // User who initiated payment
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual User CreatedByUser { get; set; }

        public Payment()
        {
            PaymentDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Status = PaymentStatus.Pending;
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
