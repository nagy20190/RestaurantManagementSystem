using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Payment DTO
    public class PaymentDTO
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod Method { get; set; }
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string TransactionID { get; set; }
        public PaymentStatus Status { get; set; }
        public string CardType { get; set; }
    }

    // DTO for creating a new payment
    public class CreatePaymentDTO
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public int OrderID { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string BillingAddress { get; set; }
    }

    // DTO for payment processing
    public class ProcessPaymentDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "OrderID must be greater than 0")]
        public int OrderID { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public string PaymentToken { get; set; } // Required for card/online payments

        [MaxLength(500)]
        public string BillingAddress { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        // Optional: Customer details for gateway
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
    }
    public class PaymentValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class PaymentGatewayResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string ResponseMessage { get; set; }
        public string GatewayTransactionId { get; set; }
    }
    // DTO for payment response
    public class PaymentResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string TransactionID { get; set; }
        public string Message { get; set; }
        public PaymentDTO Payment { get; set; }
    }

    // DTO for payment history
    public class PaymentHistoryDTO
    {
        public int UserID { get; set; }
        public List<PaymentDTO> Payments { get; set; }
        public decimal TotalPaid { get; set; }
        public int TotalTransactions { get; set; }
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