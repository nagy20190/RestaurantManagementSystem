using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.BLL.Healpers
{
    public class JWTReader
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JWTReader(IGenericRepository<User> userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> GetCurrentUserId()
        {
            // Get user ID from JWT token claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                //  _logger.LogWarning("User ID claim not found in token");
                return -1;
            }
            if (!int.TryParse(userIdClaim, out int userId))
            {
                // _logger.LogWarning("Invalid user ID format in token: {UserIdClaim}", userIdClaim);
                return -1;
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                //_logger.LogWarning("User not found in database with ID: {UserId}", userId);
                return -1;
            }

            // _logger.LogInformation("Current user profile retrieved successfully for user: {UserId}", userId);
            return user.Id;
        }

    }
}
