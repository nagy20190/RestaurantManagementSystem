using System;
using System.Collections.Generic;

namespace DeliveryManagementSystem.Core.DTOs
{
    // Basic Table DTO
    public class TableDTO
    {
        public int ID { get; set; }
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsReserved { get; set; }
        public DateTime? NextAvailableTime { get; set; }
    }

    // DTO for creating a new table
    public class CreateTableDTO
    {
        public int RestaurantID { get; set; }
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    // DTO for updating table
    public class UpdateTableDTO
    {
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    // DTO for table with reservations
    public class TableWithReservationsDTO : TableDTO
    {
        public List<ReservationDTO> Reservations { get; set; }
        public List<TimeSlotDTO> AvailableTimeSlots { get; set; }
    }

    // DTO for time slot
    public class TimeSlotDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public int? ReservationID { get; set; }
    }

    // DTO for table availability
    public class TableAvailabilityDetailsDTO
    {
        public int TableID { get; set; }
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public DateTime Date { get; set; }
        public List<TimeSlotDTO> TimeSlots { get; set; }
        public bool IsFullyBooked { get; set; }
    }

    // DTO for restaurant tables overview
    public class RestaurantTablesDTO
    {
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public List<TableDTO> Tables { get; set; }
        public int TotalTables { get; set; }
        public int AvailableTables { get; set; }
        public int ReservedTables { get; set; }
    }

    // DTO for table search/filtering
    public class TableSearchDTO
    {
        public int? RestaurantID { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string SortBy { get; set; } // "capacity", "number", "availability"
        public bool SortDescending { get; set; }
    }

}