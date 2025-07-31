using AutoMapper;
using DeliveryManagementSystem.BLL.Healpers;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly IGenericRepository<Restaurant> _restaurantRepository;
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IMapper _mapper;
        private readonly JWTReader _jWTReader;

        public RestaurantController(IGenericRepository<Restaurant> restaurantRepository, 
            IMapper mapper, JWTReader jWTReader, IGenericRepository<Review> reviewRepository)
        {
            _restaurantRepository = restaurantRepository;
            _mapper = mapper;
            _jWTReader = jWTReader;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        ///  TODO :- Enhance this method by SearchDTo
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="meal"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>


        [HttpGet("search")]
        public async Task<ActionResult<List<Restaurant>>> SearchRestaurants(
      [FromQuery] string? name = null,
      [FromQuery] string? location = null,
      [FromQuery] string? meal = null,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10)
        {
            var query = _restaurantRepository.GetAll();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(r => r.Name.ToLower().Contains(name.ToLower()));
            }
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(r => r.Address.ToLower().Contains(location.ToLower()));
            }
            if (!string.IsNullOrEmpty(meal))
            {
                query = query.Where(r => r.Meals.Any(m => m.Name.ToLower().Contains(meal.ToLower())));
            }

            var totalCount = await query.CountAsync();
            var pagedRestaurants = await query
                .Include(r => r.Meals)
                .Include(r => r.ResturantCategories)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var restaurantDtos = _mapper.Map<List<RestaurantDTO>>(pagedRestaurants);

            var result = new PagedResult<RestaurantDTO>
            {
                Data = restaurantDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page * pageSize < totalCount,
                HasPreviousPage = page > 1
            };

            return Ok(result);
        }

        #region Done
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetAllRestaurants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            try
            {
                
                var totalCount = await _restaurantRepository.CountAsync();
                // Get paginated data (this would typically come from your repository/service)
                 var restaurants = _restaurantRepository.GetPaged(page, pageSize);

                var result = new PagedResult<Restaurant>
                {
                    Data = restaurants,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1
                };

                return Ok(result);
            }
            catch 
            (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving restaurants.");
            }
        }

        // TODO : Include -> "restaurantMenuCategories": [] 
        [HttpGet("{id}")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid restaurant ID.");
            try
            {
                var restaurant = await _restaurantRepository
                    .FindByCondition(r => r.ID == id)
                    .Include(r => r.Meals)
                    .Include(r => r.ResturantCategories)
                    .FirstOrDefaultAsync();

                if (restaurant == null)
                    return NotFound($"Restaurant with ID {id} not found.");
                var restaurantDto = _mapper.Map<RestaurantDTO>(restaurant);


                return Ok(restaurantDto);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the restaurant.");
            }

        }

        
        [HttpGet("{restaurantid}/menu")]
        public async Task<ActionResult<Meal>> GetRestaurantMenu(int restaurantid)
        {
            if (restaurantid <= 0)
                return BadRequest("Invalid restaurant ID.");

            try
            {
                var restaurant = await _restaurantRepository
                    .FindByCondition(r => r.ID == restaurantid)
                    .Include(r => r.Meals)
                    .FirstOrDefaultAsync();
                if (restaurant == null)
                    return NotFound($"Restaurant with ID {restaurantid} not found.");
                var restaurantMenuDto = _mapper.Map<RestaurantWithMenuDTO>(restaurant);
                return Ok(restaurantMenuDto);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the restaurant menu.");
            }
            
        }

        [HttpGet("{restaurantid}/reviews")]
        public async Task<IActionResult> GetRestaurantReviews(int restaurantid)
        {
            if (restaurantid <= 0) 
                  return BadRequest("Invalid restaurant ID.");

            try
            {
                var restaurant = await _restaurantRepository
                .FindByCondition(r => r.ID == restaurantid)
                .Include(r => r.Reviews)
                .ThenInclude(r => r.User)
                .AsNoTracking() // .AsNoTracking() to improve performance.
                .FirstOrDefaultAsync();

                if (restaurant == null)
                    return NotFound($"Restaurant with ID {restaurantid} not found.");

                var restaurantReviews = new
                {
                    RestaurantReviewsDTO = restaurant.Reviews.Select(r => new RestaurantReviewsDTO
                    {
                        AverageRating = (decimal)(restaurant.Reviews.Any() ? restaurant.Reviews.Average(r => r.Rating) : 0),
                        TotalReviews = restaurant.Reviews.Count,
                        RestaurantName = restaurant.Name,
                        RestaurantID = restaurant.ID,
                        Reviews = restaurant.Reviews.Select(review => new ReviewDTO
                        {
                            UserID = review.User.Id,
                            UserName = review.User.UserName,
                            UserProfileImage = review.User.ProfileImageUrl,
                            Comment = review.Comment,
                            Rating = review.Rating,
                            CreatedAt = review.CreatedAt,
                        }).ToList(),

                    }).ToList()

                };
                return Ok(restaurantReviews);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving restaurant reviews.");
            }
        }


        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        [HttpPost]
        public async Task<ActionResult<Restaurant>> CreateRestaurant([FromBody] CreateRestaurantDTO restaurantDto)
        {
            if (restaurantDto == null)
            {
                return BadRequest("data cannot be null");
            }
            if (!ModelState.IsValid)
            {
            return BadRequest(ModelState);
            }
            try
            {
                var restaurant = _mapper.Map<Restaurant>(restaurantDto);
                await _restaurantRepository.AddAsync(restaurant);
                return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.ID }, restaurantDto);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }

        }
        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Restaurant>> UpdateRestaurant(int id,
            [FromBody] UpdateRestaurantDTO restaurantDto)
        {
            if (id <= 0)
                return BadRequest("Invalid restaurant ID.");
            try
            {
                var restaurant = await _restaurantRepository.GetByIdAsync(id);
                if (restaurant == null)
                    return NotFound($"Restaurant with ID {id} not found.");
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Map DTO to entity

                _mapper.Map(restaurantDto, restaurant);
                // Update restaurant
                await _restaurantRepository.UpdateAsync(restaurant);
                return Ok(restaurant);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the restaurant.");
            }
        }

        [Authorize(Roles = "SuperAdmin,RestaurantOwner")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRestaurant(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid restaurant ID.");

            try
            {
                var restaurant = await _restaurantRepository.GetByIdAsync(id);

                if (restaurant == null)
                    return NotFound($"Restaurant with ID {id} not found.");

                await _restaurantRepository.DeleteAsync(restaurant);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                // Optional: log exception
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting restaurant: {ex.Message}");
            }
        }

        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetRestaurantCategories(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid restaurant ID.");

            try
            {
                var restaurant = await _restaurantRepository
                           .FindByCondition(r => r.ID == id)
                           .Include(r => r.ResturantCategories)
                           .Include(r => r.Meals)
                           .FirstOrDefaultAsync();
                if (restaurant == null)
                    return NotFound($"Restaurant with ID {id} not found.");

                var result = restaurant.ResturantCategories.Select
                    (rc => new RestaurantMenuCategoryDTO
                {
                    Name = rc.Name,
                    RestaurantName = restaurant.Name,
                    URLPhoto = restaurant.URLPhoto,
                    MealCount = restaurant.Meals.Count()
                }).ToList();
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving restaurant categories.");
            }
        }

        [Authorize]
        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<Review>> AddReview(int id, [FromBody] CreateReviewDTO reviewDto)
        {
            if (id <= 0)
                return BadRequest("Invalid restaurant ID.");
            if (reviewDto == null)
                return BadRequest("Data cannot be null");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userId = await _jWTReader.GetCurrentUserId();
                if (userId <= 0)
                    return Unauthorized("Invalid user ID.");

                var review = _mapper.Map<Review>(reviewDto);
                review.UserID = userId;
                review.ResturantID = id;

                await _reviewRepository.AddAsync(review);
                return Ok(reviewDto);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the review.");
            }
        }

        [HttpGet("{id}/reservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetRrestaurantReservations(int id)
        {
            if (id <= 0)
                return BadRequest($"Invalid ID {id}");
            try
            {
                var restaurant = await _restaurantRepository
                .FindByCondition(r => r.ID == id) 
                .Include(r => r.Tables)
                    .ThenInclude(t => t.Reservations)
                        .ThenInclude(res => res.User)
                .FirstOrDefaultAsync();

                if (restaurant == null)
                    return NotFound($"Restaurant with ID {id} not found");

                var reservations = restaurant.Tables
                   .SelectMany(t => t.Reservations)
                   .ToList();

                if (reservations == null || !reservations.Any())
                    return NotFound($"No reservations found for restaurant with ID {id}");


                var result = new
                {
                    RestaurantID = restaurant.ID,
                    RestaurantName = restaurant.Name,
                    TotalTables = restaurant.Tables.Count,
                    TotalReservations = reservations.Count,
                    Reservations = _mapper.Map<List<ReservationDTO>>(reservations)
                };

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "internal server error ");
            }
        }
        #endregion
       
        
        // POST: api/restaurant/{id}/reservations
        

    }
}
