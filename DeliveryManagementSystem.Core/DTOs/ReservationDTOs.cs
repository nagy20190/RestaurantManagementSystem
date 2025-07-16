using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Reservation DTO
    public class ReservationDTO
    {
        public DateTime ReservationDate { get; set; }
        public int TableID { get; set; }
        public string TableNumber { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int NumberOfPeople { get; set; }
        public string QRCode { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
    }

    // DTO for creating a new reservation
    public class CreateReservationDTO
    {
        public int UserID { get; set; }
        public int RestaurantID { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public TimeSpan Duration { get; set; }
    }

    // DTO for updating reservation
    public class UpdateReservationDTO
    {
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public string SpecialRequests { get; set; }
        public TimeSpan Duration { get; set; }
    }

    // DTO for reservation with details
    public class ReservationDetailsDTO : ReservationDTO
    {
        public UserDTO Customer { get; set; }
        public TableDTO Table { get; set; }
        public RestaurantDTO Restaurant { get; set; }
        public List<ReservationStatusUpdateDTO> StatusUpdates { get; set; }
    }

    // DTO for reservation status update
    public class ReservationStatusUpdateDTO
    {
        public ReservationStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
        public int UpdatedByUserID { get; set; }
    }

    // DTO for reservation search/filtering
    public class ReservationSearchDTO
    {
        public int? RestaurantID { get; set; }
        public int? UserID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ReservationStatus? Status { get; set; }
        public int? MinPeople { get; set; }
        public int? MaxPeople { get; set; }
        public string SortBy { get; set; } // "date", "status", "restaurant"
        public bool SortDescending { get; set; }
    }

    // DTO for table availability
    public class TableAvailabilityDTO
    {
        public int RestaurantID { get; set; }
        public DateTime Date { get; set; }
        public List<AvailableTableDTO> AvailableTables { get; set; }
        public List<ReservedTableDTO> ReservedTables { get; set; }
    }

    // DTO for available table
    public class AvailableTableDTO
    {
        public int TableID { get; set; }
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public List<TimeSpan> AvailableTimes { get; set; }
    }

    // DTO for reserved table
    public class ReservedTableDTO
    {
        public int TableID { get; set; }
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CustomerName { get; set; }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed,
        NoShow
    }
}