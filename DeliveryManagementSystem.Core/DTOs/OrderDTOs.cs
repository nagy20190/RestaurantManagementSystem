using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Order DTO
    public class OrderDTO
    {
        public int ID { get; set; }
        public string OrderNumber { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal DeliveryFee { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
    }

    // DTO for creating a new order
    public class CreateOrderDTO
    {
        public int UserID { get; set; }
        public string DeliveryAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<CreateOrderItemDTO> OrderItems { get; set; }
    }

    // DTO for updating order status
    public class UpdateOrderStatusDTO
    {
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
    }

    // DTO for order with items
    public class OrderWithItemsDTO : OrderDTO
    {
        public List<OrderItemDTO> OrderItems { get; set; }
        public PaymentDTO Payment { get; set; }
        public UserDTO Customer { get; set; }
    }

    // DTO for order history
    public class OrderHistoryDTO
    {
        public int UserID { get; set; }
        public List<OrderDTO> Orders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // DTO for order tracking
    public class OrderTrackingDTO : OrderDTO
    {
        public List<OrderStatusUpdateDTO> StatusUpdates { get; set; }
        public string CurrentLocation { get; set; }
    }

    // DTO for order status update
    public class OrderStatusUpdateDTO
    {
        public OrderStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }

    // DTO for order statistics
    public class OrderStatisticsDTO
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        InPreparation,
        PickedUp,
        OutForDelivery,
        Delivered,
        Cancelled
    }
}