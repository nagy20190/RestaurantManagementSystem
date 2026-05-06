using AutoMapper;
using DeliveryManagementSystem.BLL.Healpers;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController(IGenericRepository<Table> repository,
        IGenericRepository<Restaurant> restaurant,
        IMapper mapper,
        JWTReader jwtReader) : ControllerBase
    {
        private readonly IGenericRepository<Table> _repository = repository;
        private readonly IGenericRepository<Restaurant> _restaurant = restaurant;
        private readonly IMapper _mapper = mapper;
        private readonly JWTReader _jwtReader = jwtReader;


        // - `GET /` — Get all tables
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            var tables = await _repository.GetAll().Include(t => t.Restaurant).ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TableDTO>>(tables);
            return Ok(dtos);
        }

        // - `GET /{id}` — Get table by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTableById(int id)
        {
            var table = await _repository.GetByIdAsync(id);
            return Ok(_mapper.Map<TableDTO>(table));
        }

        //- `POST /` — Create table(SuperAdmin, RestaurantOwner)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableDTO createDto)
        {
            try
            {
                bool isAuthorized = await CheckUserIsAuthorized(createDto.RestaurantID);
                if (!isAuthorized)
                {
                    return StatusCode(403, "You do not have permission to create a table for this restaurant");
                }
                var table = _mapper.Map<Table>(createDto);
                await _repository.AddAsync(table);
                return Ok("Table created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the table. {ex.Message}");
            }
        }

        //- `PUT /{id}` — Update table(SuperAdmin, RestaurantOwner)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDTO updateDto)
        {
            try
            {
                var existingTable = await _repository.GetByIdAsync(id);
                var isAuthorized = await CheckUserIsAuthorized(existingTable.RestaurantID);
                if (!isAuthorized)
                {
                    return StatusCode(403, "You do not have permission to update this table");
                }
                _mapper.Map(updateDto, existingTable);
                await _repository.UpdateAsync(existingTable);
                return Ok("Table updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the table. {ex.Message}");
            }
        }

        //- `DELETE /{ id}` — Delete table(SuperAdmin, RestaurantOwner)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var existingTable = await _repository.GetByIdAsync(id);
                var isAuthorized = await CheckUserIsAuthorized(existingTable.RestaurantID);
                if (!isAuthorized)
                {
                    return StatusCode(403, "You do not have permission to delete this table");
                }
                await _repository.DeleteAsync(existingTable);
                return Ok("Table deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the table. {ex.Message}");
            }
        }

        //- `GET / restaurant /{ restaurantId}` — Tables by restaurant
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetTablesByRestaurant(int restaurantId)
        {
            var restaurant = await _restaurant.GetByIdAsync(restaurantId);
            if (restaurant is null)
            {
                return NotFound("Restaurant not found");
            }
            var tables = await _repository.GetAll()
                .Where(t => t.RestaurantID == restaurantId)
                .Include(t => t.Restaurant)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TableDTO>>(tables);
            return Ok(dtos);
        }

        // - `GET / search` — Search tables by criteria (e.g., capacity, restaurant name)
        [HttpGet("search")]
        public async Task<IActionResult> SearchTables(int? capacity, string? restaurantName)
        {
            var query = _repository.GetAll().Include(t => t.Restaurant).AsQueryable();
            if (capacity.HasValue)
            {
                query = query.Where(t => t.Capacity >= capacity.Value);
            }
            if (!string.IsNullOrEmpty(restaurantName))
            {
                query = query.Where(t => t.Restaurant.Name.Contains(restaurantName));
            }
            var tables = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TableDTO>>(tables);
            return Ok(dtos);
        }


        // private helper method to check if the current user is authorized to manage the table
        private async Task<bool> CheckUserIsAuthorized(int restaurantId)
        {
            var userId = await _jwtReader.GetCurrentUserId();
            var restaurant = await _restaurant.GetByIdAsync(restaurantId);
            if (restaurant is null)
            {
                return false;
            }
            if (restaurant.OwnerID != userId && !await _jwtReader.IsInRoleAsync("SuperAdmin"))
            {
                return false;
            }
            return true;
        }





    }
}
