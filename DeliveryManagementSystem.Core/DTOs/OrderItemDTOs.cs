using System;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Order Item DTO
    public class OrderItemDTO
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public int MealID { get; set; }
        public string MealName { get; set; }
        public string MealPhoto { get; set; }
        public int OrderID { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    // DTO for creating a new order item
    public class CreateOrderItemDTO
    {
        public int MealID { get; set; }
        public int Quantity { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    // DTO for updating order item
    public class UpdateOrderItemDTO
    {
        public int Quantity { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    // DTO for order item with meal details
    public class OrderItemWithMealDTO : OrderItemDTO
    {
        public MealDTO Meal { get; set; }
    }
}