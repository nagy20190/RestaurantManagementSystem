using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DeliveryManagementSystem.BLL.Healpers
{
    public class JWTReader(IGenericRepository<User> userRepository, IHttpContextAccessor httpContextAccessor)
    {
        private readonly IGenericRepository<User> _userRepository = userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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

        public async Task<bool> IsInRoleAsyn(string role)
        {
            var roles = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value);
            return roles != null && roles.Contains(role);
        }
    }
}
