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
    public class ReviewController(IGenericRepository<Review> repository, IMapper mapper, 
        JWTReader jwtReader, IGenericRepository<User> userRepository) : ControllerBase
    {
        private readonly IGenericRepository<Review> _repository = repository;
        private readonly IGenericRepository<User> _userRepository = userRepository; 
        private readonly IMapper _mapper = mapper;
        private readonly JWTReader _jwtReader = jwtReader;

        //`GET /{id}`,
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _repository.GetAll()
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.ID == id);
            var dto = _mapper.Map<ReviewDto>(review);
            return Ok(dto);
        }


        //`PUT /{id}`,
        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDto reviewDto)
        {
            try
            {
                var userId = await _jwtReader.GetCurrentUserId();
                var review = await _repository.GetByIdAsync(id);
                if (userId != review.UserID)
                {
                    return Forbid("You can only update your own reviews.");
                }
                _mapper.Map(reviewDto, review);
                await _repository.UpdateAsync(review);
                return Ok(reviewDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //`DELETE /{id}`
        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = await _jwtReader.GetCurrentUserId();
                var review = await _repository.GetByIdAsync(id);
                if (userId != review.UserID && !await _jwtReader.IsInRoleAsync("Admin"))
                {
                    return Forbid("You can only delete your own reviews.");
                }
                await _repository.DeleteAsync(review);
                return Ok("Review deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // `POST /`
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto reviewDto)
        {
            try
            {
                var userId = await _jwtReader.GetCurrentUserId();
                var review = _mapper.Map<Review>(reviewDto);

                review.UserID = userId; // Set the user ID from the JWT token
                var canReview = await CanReviewRestaurant(review.RestaurantID);
                if (!canReview)
                {
                    return BadRequest("You are not eligible to review this restaurant. Please complete an order or reservation first.");
                }

                await _repository.AddAsync(review);
                return Ok(reviewDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // 1) List all reviews (admin/moderation dashboards, paginated).
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var reviews = await _repository.GetPaged(pageIndex, pageSize)
            .Include(r => r.User)
            .Include(r => r.Restaurant)
            .ToListAsync();
            var dtos = _mapper.Map<List<ReviewDto>>(reviews);
            return Ok(dtos);
        }


        // 2) Get all reviews for a specific restaurant (public, used on restaurant page).
        //    GET /api/Review/restaurant/{restaurantId}
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetReviewsByRestaurant(int restaurantId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var reviews = await _repository.GetPaged(pageIndex, pageSize)
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Where(r => r.RestaurantID == restaurantId)
                .ToListAsync();
            var dtos = _mapper.Map<List<ReviewDto>>(reviews);
            return Ok(dtos);
        }


        // 3) Get aggregated rating summary for a restaurant (avg rating + total count + breakdown 1..5).
        [HttpGet("restaurant/{restaurantId}/summary")]
        public async Task<IActionResult> GetRestaurantRatingSummary(int restaurantId)
        {
            var reviews = await _repository.GetAll().Where(r => r.RestaurantID == restaurantId).ToListAsync();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            var totalReviews = reviews.Count;
            var ratingBreakdown = reviews.GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToList();
            var summary = new
            {
                RestaurantID = restaurantId,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingBreakdown = ratingBreakdown
            };
            return Ok(summary);
        }


        // 4) Get all reviews written by the currently authenticated user ("My reviews" page).
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = await _jwtReader.GetCurrentUserId();
            var reviews = await _repository.GetAll()
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Where(r => r.UserID == userId)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return Ok(dtos);
        }


        // 5) Get all reviews written by a specific user (admin only — moderation/profile audit).
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReviewsByUser(int userId)
        {
            if (!await _jwtReader.IsInRoleAsync("Admin"))
            {
                return Forbid("Only admins can access reviews by user.");
            }
            var reviews = await _repository.GetAll()
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Where(r => r.UserID == userId)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return Ok(dtos);
        }


        // 6) Filter / search reviews (by rating range, restaurant, date range, sort by newest/highest).
        [HttpGet("search")]
        public async Task<IActionResult> SearchReviews([FromQuery] int? restaurantId, 
            [FromQuery] int? minRating, [FromQuery] int? maxRating, 
            [FromQuery] string? sortBy)
        {
            var query = _repository.GetAll()
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .AsQueryable();

            if (restaurantId.HasValue)
            {
                query = query.Where(r => r.RestaurantID == restaurantId.Value);
            }
            if (minRating.HasValue)
            {
                query = query.Where(r => r.Rating >= minRating.Value);
            }
            if (maxRating.HasValue)
            { 
                query = query.Where(r => r.Rating <= maxRating.Value);
            }
            if (string.IsNullOrEmpty(sortBy))
            {
                query = query.OrderByDescending(r => r.Rating);
            }
            else
            {
                switch (sortBy.ToLower())
                {
                    case "newest":
                        query = query.OrderByDescending(r => r.CreatedAt);
                        break;
                    case "highest":
                        query = query.OrderByDescending(r => r.Rating);
                        break;
                }
            }
            var reviews = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return Ok(dtos);
        }


        // 7) Check whether the current user is eligible to review a restaurant
        //    (e.g. has a completed order/reservation and hasn't already reviewed it).
        //    GET /api/Review/can-review/{restaurantId}
        private async Task<bool> CanReviewRestaurant(int restaurantId)
        {
            var userId = await _jwtReader.GetCurrentUserId();
            var user = await _userRepository.GetAll()
                .Include(u => u.Orders)
                .Include(u => u.Reservations)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            var hasOrders = user.Orders.Any
                (o => o.RestaurantID == restaurantId
                && o.Status == OrderStatus.Delivered);

            var hasReservations = user.Reservations.Any
                (r => r.RestaurantID == restaurantId
                && r.Status == ReservationStatus.Confirmed);

            if (!hasOrders && !hasReservations)
            {
                return false; // User has no completed orders or reservations with this restaurant
            }
            return true; // User is eligible to review
        }





    }
}
