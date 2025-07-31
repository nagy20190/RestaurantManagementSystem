using AutoMapper;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealController : ControllerBase
    {
        private readonly IGenericRepository<Meal> _mealRepository;
        private readonly IGenericRepository<Restaurant> _restaurantRepository;
        private readonly IGenericRepository<RestaurantMenuCategory> _categoryRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IMapper _mapper;

        public MealController(
            IGenericRepository<Meal> mealRepository,
            IGenericRepository<Restaurant> restaurantRepository,
            IGenericRepository<RestaurantMenuCategory> categoryRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IMapper mapper)
        {
            _mealRepository = mealRepository;
            _restaurantRepository = restaurantRepository;
            _categoryRepository = categoryRepository;
            _orderItemRepository = orderItemRepository;
            _mapper = mapper;
        }

        // GET: api/Meal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MealDTO>>> 
            GetMeals([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var meals = _mealRepository.GetPaged(page, pageSize)
                    .Include(m => m.Resturant)
                    .Include(m => m.RestaurantMenuCategory);

                var mealDTOs = _mapper.Map<IEnumerable<MealDTO>>(meals);
                return Ok(mealDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MealDetailsDTO>> GetMeal(int id)
        {
            try
            {
                var meal = await _mealRepository.GetByIdAsync(id);
                if (meal == null)
                {
                    return NotFound($"Meal with ID {id} not found");
                }

                var mealDetailsDTO = _mapper.Map<MealDetailsDTO>(meal);
                return Ok(mealDetailsDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Meal
        [Authorize(Roles = "SuperAdmin, RestaurantOwner")]
        [HttpPost]
        public async Task<ActionResult<MealDTO>> CreateMeal([FromBody] CreateMealDTO createMealDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate restaurant exists
                var restaurant = await _restaurantRepository.GetByIdAsync(createMealDTO.RestaurantID);
                if (restaurant == null)
                {
                    return BadRequest($"Restaurant with ID {createMealDTO.RestaurantID} not found");
                }

                // Validate category if provided
                if (createMealDTO.MenuCategoryID.HasValue)
                {
                    var category = await _categoryRepository.GetByIdAsync(createMealDTO.MenuCategoryID.Value);
                    if (category == null)
                    {
                        return BadRequest($"Menu category with ID {createMealDTO.MenuCategoryID.Value} not found");
                    }
                }

                var meal = _mapper.Map<Meal>(createMealDTO);
                meal.RestaurantMenuCategoryID = createMealDTO.MenuCategoryID ?? 0;

                await _mealRepository.AddAsync(meal);

                var mealDTO = _mapper.Map<MealDTO>(meal);
                return CreatedAtAction(nameof(GetMeal), new { id = meal.ID }, mealDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Meal/5
        [Authorize(Roles = "SuperAdmin, RestaurantOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeal(int id, [FromBody] UpdateMealDTO updateMealDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingMeal = await _mealRepository.GetByIdAsync(id);
                if (existingMeal == null)
                {
                    return NotFound($"Meal with ID {id} not found");
                }

                // Validate category if provided
                if (updateMealDTO.MenuCategoryID.HasValue)
                {
                    var category = await _categoryRepository.GetByIdAsync(updateMealDTO.MenuCategoryID.Value);
                    if (category == null)
                    {
                        return BadRequest($"Menu category with ID {updateMealDTO.MenuCategoryID.Value} not found");
                    }
                }

                // Update properties
                existingMeal.Name = updateMealDTO.Name;
                existingMeal.Description = updateMealDTO.Description;
                existingMeal.Price = updateMealDTO.Price;
                existingMeal.URLPhoto = updateMealDTO.URLPhoto;
                if (updateMealDTO.MenuCategoryID.HasValue)
                {
                    existingMeal.RestaurantMenuCategoryID = updateMealDTO.MenuCategoryID.Value;
                }

                await _mealRepository.UpdateAsync(existingMeal);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Meal/5
        [Authorize(Roles = "SuperAdmin, RestaurantOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            try
            {
                var meal = await _mealRepository.GetByIdAsync(id);
                if (meal == null)
                {
                    return NotFound($"Meal with ID {id} not found");
                }

                await _mealRepository.DeleteAsync(meal);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MealDTO>>>
            SearchMeals([FromQuery] MealSearchDTO searchDTO)
        {
            try
            {
                IQueryable<Meal> query = _mealRepository.GetAll();
                
                // Apply filters
                if (!string.IsNullOrEmpty(searchDTO.SearchTerm))
                {
                    query = query.Where(m => m.Name.Contains(searchDTO.SearchTerm) ||
                                           m.Description.Contains(searchDTO.SearchTerm));
                }

                if (searchDTO.RestaurantID.HasValue)
                {
                    query = query.Where(m => m.ResturantID == searchDTO.RestaurantID.Value);
                }

                if (searchDTO.MenuCategoryID.HasValue)
                {
                    query = query.Where(m => m.RestaurantMenuCategoryID == searchDTO.MenuCategoryID.Value);
                }

                if (searchDTO.MinPrice.HasValue)
                {
                    query = query.Where(m => m.Price >= searchDTO.MinPrice.Value);
                }

                if (searchDTO.MaxPrice.HasValue)
                {
                    query = query.Where(m => m.Price <= searchDTO.MaxPrice.Value);
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(searchDTO.SortBy))
                {
                    switch (searchDTO.SortBy.ToLower())
                    {
                        case "price":
                            query = searchDTO.SortDescending ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price);
                            break;
                        case "name":
                            query = searchDTO.SortDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name);
                            break;
                        default:
                            query = query.OrderBy(m => m.Name);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(m => m.Name);
                }

                var meals = await query.ToListAsync();
                var mealDTOs = _mapper.Map<IEnumerable<MealDTO>>(meals);

                return Ok(mealDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/restaurant/{restaurantId}
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<ActionResult<IEnumerable<MealDTO>>> GetMealsByRestaurant(int restaurantId)
        {
            try
            {
                var meals = _mealRepository.FindByCondition(m => m.ResturantID == restaurantId)
                    .Include(m => m.Resturant)
                    .Include(m => m.RestaurantMenuCategory);

                var mealDTOs = _mapper.Map<IEnumerable<MealDTO>>(meals);
                return Ok(mealDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<MealDTO>>> GetMealsByCategory(int categoryId)
        {
            try
            {
                var meals = _mealRepository.FindByCondition(m => m.RestaurantMenuCategoryID == categoryId)
                    .Include(m => m.Resturant)
                    .Include(m => m.RestaurantMenuCategory);

                var mealDTOs = _mapper.Map<IEnumerable<MealDTO>>(meals);
                return Ok(mealDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<PopularMealDTO>>> GetPopularMeals([FromQuery] int count = 10)
        {
            try
            {
                var popularMeals = _orderItemRepository.GetAll()
                    .GroupBy(oi => oi.MealId)
                    .Select(g => new
                    {
                        MealID = g.Key,
                        OrderCount = g.Count()
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(count)
                    .Join(_mealRepository.GetAll().Include(m => m.Resturant),
                          popular => popular.MealID,
                          meal => meal.ID,
                          (popular, meal) => new { popular, meal })
                    .Select(x => new PopularMealDTO
                    {
                        Name = x.meal.Name,
                        Description = x.meal.Description,
                        Price = x.meal.Price,
                        URLPhoto = x.meal.URLPhoto,
                        RestaurantID = x.meal.ResturantID,
                        RestaurantName = x.meal.Resturant.Name,
                        IsAvailable = true, // You might want to add this field to your entity
                        PreparationTime = 30, // Default value, you might want to add this field
                        OrderCount = x.popular.OrderCount,
                        AverageRating = 0 // Default value since reviews are not directly linked to meals
                    });

                var result = await popularMeals.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Meal/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<MealDTO>>> GetAvailableMeals()
        {
            try
            {
                // This assumes you have an IsAvailable field or you can determine availability
                // For now, returning all meals. You might want to add availability logic
                var meals = _mealRepository.GetAll()
                    .Include(m => m.Resturant)
                    .Include(m => m.RestaurantMenuCategory);

                var mealDTOs = _mapper.Map<IEnumerable<MealDTO>>(meals);
                return Ok(mealDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
