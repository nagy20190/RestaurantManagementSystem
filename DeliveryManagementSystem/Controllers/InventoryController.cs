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
    //[Authorize]
    public class InventoryController(
        IGenericRepository<Inventory> repository,
        IGenericRepository<Restaurant> restaurantRepository,
        IGenericRepository<Meal> mealRepository,
        IMapper mapper,
        JWTReader jwtReader) : ControllerBase
    {
        private readonly IGenericRepository<Inventory> _repository = repository;
        private readonly IGenericRepository<Restaurant> _restaurantRepository = restaurantRepository;
        private readonly IGenericRepository<Meal> _mealRepository = mealRepository;
        private readonly IMapper _mapper = mapper;
        private readonly JWTReader _jwtReader = jwtReader;


        //- `GET /` — Get all inventory
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
           var inventories = await _repository.GetAll()
               .Include(i => i.Restaurant)
               .Include(i => i.Meal)
               .ToListAsync();
           var dtos = _mapper.Map<List<InventoryDto>>(inventories);
           return Ok(dtos);
        }

        //- `GET /{id}` — Get by ID
        [HttpGet("{id}")]
        [Authorize (Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetById(int id)
        {
            var inventory = await _repository.GetAll()
                .Include(i => i.Restaurant)
                .Include(i => i.Meal)
                .FirstOrDefaultAsync(i => i.ID == id);

            var isAuthorized = await CheckIsAuthorized(inventory!.RestaurantID);
            if (!isAuthorized)
              return StatusCode(403, "Unauthorized access");
            var dto = _mapper.Map<InventoryDto>(inventory);
            return Ok(dto);
        }

        //- `POST /` — Create entry(SuperAdmin, RestaurantOwner)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> Create([FromBody] InventoryCreateDto createDto)
        {
            try
            {
                var isAuthorized = await CheckIsAuthorized(createDto.RestaurantID);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");

                var inventory = _mapper.Map<Inventory>(createDto);
                inventory.LastUpdated = DateTime.Now;
                await _repository.AddAsync(inventory);
                return Ok("Inventory created successfully");
            }
            catch
            {
                return StatusCode(500, "An error occurred while creating the inventory");
            }
        }

        //- `PUT /{id}` — Update quantity(SuperAdmin, RestaurantOwner)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> Update(int id, [FromBody] InventoryUpdateDto updateDto)
        {
            try
            {
                var inventory = await _repository.GetByIdAsync(id);
                var isAuthorized = await CheckIsAuthorized(inventory.RestaurantID);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");

                inventory.Quantity = updateDto.Quantity;
                inventory.LastUpdated = DateTime.Now;
                await _repository.UpdateAsync(inventory);
                return Ok("Inventory updated successfully");
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the inventory");
            }
        }

        //- `DELETE /{id}` — Delete entry(SuperAdmin, RestaurantOwner)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var inventory = await _repository.GetByIdAsync(id);
                var isAuthorized = await CheckIsAuthorized(inventory.RestaurantID);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");
                await _repository.DeleteAsync(inventory);
                return Ok("Inventory deleted successfully");
            }
            catch
            {
                return StatusCode(500, "An error occurred while deleting the inventory");
            }
        }

        //- `GET / meal /{ mealId}` — Inventory for a meal
        [HttpGet("meal/{mealId}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetByMealId(int mealId)
        {
            var userId = await _jwtReader.GetCurrentUserId();
            if (userId == -1)
                return Unauthorized();

            var query = _repository.GetAll()
                .Include(i => i.Meal)
                .Include(i => i.Restaurant)
                .Where(i => i.MealID == mealId);

            // RestaurantOwner only sees inventory for restaurants they own
            if (!_jwtReader.IsInRole("SuperAdmin"))
                query = query.Where(i => i.Restaurant.OwnerID == userId);

            var inventories = await query.ToListAsync();
            var dtos = _mapper.Map<List<InventoryDto>>(inventories);
            return Ok(dtos);
        }

        //  GET /restaurant/{restaurantId} — list all inventory for a specific restaurant. 
        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetInventoriesByRestaurantId(int restaurantId)
        {
            var isAuthorized = await CheckIsAuthorized(restaurantId);
            if (!isAuthorized)
                return StatusCode(403, "Unauthorized access");
            var inventories = await _repository.GetAll()
                .Include(i => i.Meal)
                .Where(i => i.RestaurantID == restaurantId)
                .ToListAsync();
            var dtos = _mapper.Map<List<InventoryDto>>(inventories);
            return Ok(dtos);
        }


        //GET /restaurant/{restaurantId}/meal/{mealId} —fetch the single inventory record for a meal within a restaurant. 
        [HttpGet("restaurant/{restaurantId}/meal/{mealId}")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetInventoryByRestaurantAndMealId(int restaurantId, int mealId)
        {
            var isAuthorized = await CheckIsAuthorized(restaurantId);
            if (!isAuthorized)
                return StatusCode(403, "Unauthorized access");

            var inventory = await _repository.GetAll()
                .Include(i => i.Meal)
                .Include(i => i.Restaurant)
                .Where(i => i.RestaurantID == restaurantId && i.MealID == mealId)
                .FirstOrDefaultAsync();

            var dto = _mapper.Map<InventoryDto>(inventory);
            return Ok(dto);
        }


        //PATCH /{id}/adjust (or /increment, /decrement) — change quantity by a delta (+/- N). Critical for orders,
        [HttpPatch("{id}/adjust")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> AdjustQuantity(int id, [FromBody] InventoryAdjustDto adjustDto)
        {
            try
            {
                var inventory = 
                    await _repository.GetAll().FirstOrDefaultAsync( i => i.RestaurantID == adjustDto.RestaurantID 
                    && i.ID == id && i.MealID == adjustDto.MealID);

                var isAuthorized = await CheckIsAuthorized(inventory.RestaurantID);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");
                inventory.Quantity += adjustDto.Delta;
                inventory.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateAsync(inventory);
                return Ok("Inventory quantity adjusted successfully");
            }
            catch
            {
                return StatusCode(500, "An error occurred while adjusting the inventory quantity");
            }
        }

        //GET /low-stock?threshold=N (scoped by restaurant for owners, global for SuperAdmin).
        [HttpGet("low-stock")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold)
        {
            var userId = await _jwtReader.GetCurrentUserId();
            if (userId == -1)
                return Unauthorized();
            var query = _repository.GetAll()
                .Include(i => i.Meal)
                .Include(i => i.Restaurant)
                .Where(i => i.Quantity <= threshold);
            // RestaurantOwner only sees inventory for restaurants they own
            if (!_jwtReader.IsInRole("SuperAdmin"))
                query = query.Where(i => i.Restaurant.OwnerID == userId);
            var lowStockItems = await query.ToListAsync();
            var dtos = _mapper.Map<List<InventoryDto>>(lowStockItems);
            return Ok(dtos);
        }

        //GET /out-of-stock — items with Quantity == 0, again scoped by role.
        [HttpGet("out-of-stock")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetOutOfStock()
        {
            var userId = await _jwtReader.GetCurrentUserId();
            if (userId == -1)
                return Unauthorized();
            var query = _repository.GetAll()
                .Include(i => i.Meal)
                .Include(i => i.Restaurant)
                .Where(i => i.Quantity == 0);
            // RestaurantOwner only sees inventory for restaurants they own
            if (!_jwtReader.IsInRole("SuperAdmin"))
                query = query.Where(i => i.Restaurant.OwnerID == userId);
            var outOfStockItems = await query.ToListAsync();
            var dtos = _mapper.Map<List<InventoryDto>>(outOfStockItems);
            return Ok(dtos);
        }

        //POST /bulk — bulk seed of inventory rows when a restaurant onboards meals, otherwise an owner must call POST / for every meal.
        [HttpPost("bulk/{restaurantId}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> BulkCreate(int restaurantId, [FromBody] List<InventoryCreateDto> createDtos)
        {
            try
            {
                if (createDtos == null || !createDtos.Any())
                    return BadRequest("No inventory data provided");

                var isAuthorized = await CheckIsAuthorized(restaurantId);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");
                var inventories = _mapper.Map<List<Inventory>>(createDtos);
                foreach (var inventory in inventories)
                { 
                    inventory.LastUpdated = DateTime.Now;
                    inventory.RestaurantID = restaurantId;
                }
                await _repository.AddRangeAsync(inventories);
                return Ok("Bulk inventory created successfully");
            }
            catch(Exception ex) 
            {
                return StatusCode(500, "An error occurred while creating the bulk inventory");
            }
        }

        //Order‑integration hook / internal decrement — when an order is placed, nothing in this controller decrements stock.
        //Either this happens in the order service (confirm it does) or you need an explicit endpoint invoked by the order flow.
        [HttpPost("{id}/decrement")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DecrementStock(int id, [FromBody] InventoryAdjustDto decrementDto)
        {
            try
            {
                var inventory = await _repository.GetByIdAsync(id);
                var isAuthorized = await CheckIsAuthorized(inventory.RestaurantID);
                if (!isAuthorized)
                    return StatusCode(403, "Unauthorized access");
                if (inventory.Quantity < decrementDto.Delta)
                    return BadRequest("Insufficient stock to decrement");
                if (decrementDto.Delta <= 0)
                    return BadRequest("Invalid decrement amount");
                inventory.Quantity -= decrementDto.Delta;
                inventory.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateAsync(inventory);
                return Ok("Inventory stock decremented successfully");
            }
            catch
            {
                return StatusCode(500, "An error occurred while decrementing the inventory stock");
            }
        }


        //Restore/soft‑delete or audit — DELETE is hard delete; 
        //stock history is lost. If business rules require audit of stock changes,
        [HttpGet("{id}/history")]
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        public async Task<IActionResult> GetInventoryHistory(int id)
        {
            // This is a placeholder implementation. In a real application, would query an InventoryHistory table.
            return Ok("Inventory history endpoint is not implemented yet.");
        }


        // private helper methods 
        private async Task<bool> CheckIsAuthorized(int restaurantId)
        {
            var userId = await _jwtReader.GetCurrentUserId();
            if (userId == -1)
                return false;

            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant is null)
                return false;

            if (_jwtReader.IsInRole("SuperAdmin"))
                return true;

            return restaurant.OwnerID == userId;
        }



    }
}
