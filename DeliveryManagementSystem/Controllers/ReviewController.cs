using AutoMapper;
using DeliveryManagementSystem.BLL.Healpers;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IGenericRepository<Review> repository, IMapper mapper, JWTReader jwtReader) : ControllerBase
    {
        private readonly IGenericRepository<Review> _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly JWTReader _jwtReader = jwtReader;

        //- Missing:
        //`GET /{id}`,
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _repository.GetByIdAsync(id);
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
                if (userId != review.UserID && !await _jwtReader.IsInRoleAsyn("Admin"))
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
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto reviewDto)
        {
            try
            {
                var userId = await _jwtReader.GetCurrentUserId();
                var review = _mapper.Map<Review>(reviewDto);

                review.UserID = userId; // Set the user ID from the JWT token
                await _repository.AddAsync(review);
                
                return Ok(reviewDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
    




    
    }
}
