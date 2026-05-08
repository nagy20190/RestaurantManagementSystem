using AutoMapper;
using DeliveryManagementSystem.BLL.Services;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IGenericRepository<User> userRepository,
        ILogger<UserController> logger, IMapper mapper, IPasswordHasher<User> passwordHasher,
        IGenericRepository<Roles> roleRepository, UserManager<User> userManager,
        RoleManager<Roles> roleManager, IConfiguration configuration, EmailServices emailService) : ControllerBase
    {
        private readonly IGenericRepository<User> _userRepo = userRepository;
        private readonly IGenericRepository<Roles> _roleRepository = roleRepository;
        private readonly ILogger<UserController> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<Roles> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly EmailServices _emailService = emailService;



        #region Not Completed
        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Get user ID from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID claim not found in token");
                    return Unauthorized(new { Message = "Invalid authentication token" });
                }

                // Parse user ID (assuming it's stored as string in claims)
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID format in token: {UserIdClaim}", userIdClaim);
                    return Unauthorized(new { Message = "Invalid user ID format" });
                }

                // Get user from database
                var user = await _userRepo.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found in database with ID: {UserId}", userId);
                    return NotFound(new { Message = "User not found" });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted to access profile: {UserId}", userId);
                    return Unauthorized(new { Message = "User account is inactive" });
                }

                // Map to DTO to avoid exposing sensitive information
                var userDTO = _mapper.Map<UserDTO>(user);

                _logger.LogInformation("Current user profile retrieved successfully for user: {UserId}", userId);

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user profile");
                return StatusCode(500, new { Message = "An error occurred while retrieving user profile" });
            }
        }

        [Authorize]
        [HttpPut("current")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserProfileDTO updateUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid authentication token" });
                }

                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Check for email uniqueness if email is being changed
                if (!string.IsNullOrEmpty(updateUserDTO.Email) &&
                    updateUserDTO.Email != user.Email)
                {
                    var existingUser = await _userRepo
                        .FindByCondition(u => u.Email.ToLower() == updateUserDTO.Email.ToLower()
                        && u.Id != userId)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        return BadRequest(new { Message = "Email address is already in use" });
                    }
                }

                // Map updates to user entity
                _mapper.Map(updateUserDTO, user);

                await _userRepo.UpdateAsync(user);

                var updatedUserDTO = _mapper.Map<UserDTO>(user);

                _logger.LogInformation("User profile updated successfully for user: {UserId}", userId);

                return Ok(updatedUserDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current user profile");
                return StatusCode(500, new { Message = "An error occurred while updating user profile" });
            }
        }

        [Authorize]
        [HttpPost("current/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid authentication token" });
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Verify current password (you'll need to implement password verification)

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { Message = "Password change failed", Errors = result.Errors.Select(e => e.Description) });
                }

                // Check if new password is the same as current password
                if (changePasswordDTO.CurrentPassword == changePasswordDTO.NewPassword)
                {
                    return BadRequest(new { Message = "New password cannot be the same as the current password" });
                }

                // Hash new password
                user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordDTO.NewPassword);

                await _userRepo.UpdateAsync(user);

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return Ok(new { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for current user");
                return StatusCode(500, new { Message = "An error occurred while changing password" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string  Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Assume you have a frontend link with token & email as query params
            var resetLink = $"{_configuration["Frontend:ResetPasswordUrl"]}?email={Email}&token={Uri.EscapeDataString(token)}";

            // Send the email (you can use SendGrid or SMTP here)
            await _emailService.SendEmailConfirmationAsync(Email, "Reset Password", $"Reset your password using this link: {resetLink}");

            return Ok(new { Message = "Reset password link sent to email" });
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Failed to reset password", Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Password reset successful" });
        }
        #endregion


        // soft Delete current user account
        [Authorize]
        [HttpDelete("current")]
        public async Task<IActionResult> DeactivateCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid authentication token" });
                }

                var user = await _userRepo.GetByIdAsync(userId);
                // Soft delete - mark as inactive
                user.IsActive = false;
                BackgroundJob.Schedule(() => _userRepo.DeleteAsync(user),
                    TimeSpan.FromDays(30)); // Schedule actual deletion after 30 days
                await _userRepo.UpdateAsync(user);

                return Ok(new { Message = "Account deactivated successfully" });
            }
            catch
            {
                return StatusCode(500, new { Message = "An error occurred while deactivating account" });
            }
        }

        #region Admin Operations

        [HttpGet("GetAll")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Min(100, Math.Max(1, pageSize));

                var query = _userRepo.GetAll();

                // Apply filters
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.UserName.ToLower().Contains(searchTermLower) ||
                        u.Email.ToLower().Contains(searchTermLower) ||
                        u.UserName.ToLower().Contains(searchTermLower) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
                }

                //if (!string.IsNullOrWhiteSpace(role))
                //{
                //    query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == role.ToLower()));
                //}

                // Apply sorting
                query = sortBy.ToLower() switch
                {
                    "name" => sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.UserName)
                        : query.OrderBy(u => u.UserName),
                    "email" => sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email),
                    "username" => sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.UserName)
                        : query.OrderBy(u => u.UserName),
                    "createdat" => sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderByDescending(u => u.CreatedAt)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDTOs = _mapper.Map<List<UserDTO>>(users);

                var result = new PaginatedResult<UserDTO>
                {
                    Items = userDTOs,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
                return Ok(result);
            }
            catch 
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetActiveUsers()
        {
           var users = await _userRepo
               .FindByCondition(u => u.IsActive)
               .ToListAsync();

           var userProfiles = users.Select(u => new UserSummaryDTO
           {
               Id = u.Id.ToString(),
               UserName = u.UserName,
               Email = u.Email,
           }).ToList();

           return Ok(userProfiles);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin, RestaurantOwner")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var user = await _userRepo.GetByIdAsync(id);

            if (!user.IsActive)
            {
                return NotFound(new { Message = $"User with ID {id} not found" });
            }

            var userDTO = _mapper.Map<UserDTO>(user);

            // Remove sensitive information for non-admin users viewing other profiles
            if (!User.IsInRole("SuperAdmin"))
            {
                userDTO.Email = string.Empty; // Hide email from non-admin users
                userDTO.Phone = null;   // Hide phone from non-admin users
            }
            return Ok(userDTO);
        }

        // soft delete user account
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            try
            {
                // A SuperAdmin could accidentally (or intentionally) delete their own account
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == id.ToString())
                    return BadRequest(new { Message = "You cannot delete your own account" });

                var user = await _userRepo.GetByIdAsync(id);
                // Soft delete - mark as inactive
                user.IsActive = false;
                await _userRepo.UpdateAsync(user);
                BackgroundJob.Schedule(() => _userRepo.DeleteAsync(user),
                    TimeSpan.FromDays(30)); // Schedule actual deletion after 30 days

                return Ok(new { Message = "User account deleted successfully" });
            }
            catch 
            {
                return StatusCode(500, new { Message = "An error occurred while deleting user account" });
            }
        }

        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RestoreUser([FromRoute] int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user.IsActive)
                {
                    return BadRequest(new { Message = "User is already active" });
                }
                user.IsActive = true;
                await _userRepo.UpdateAsync(user);
                BackgroundJob.Delete($"DeleteUser:{id}"); // Cancel scheduled deletion if it exists

                var userDTO = _mapper.Map<UserDTO>(user);

                return Ok(userDTO);
            }
            catch
            {
                return StatusCode(500, new { Message = "An error occurred while restoring user account" });
            }
        }


        [HttpPut("{id:int}/roles")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateUserRoles
            ([FromRoute] int id, [FromBody] UpdateUserRolesDTO updateRolesDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (updateRolesDTO.RoleNames == null || updateRolesDTO.RoleNames.Count == 0)
                {
                    return BadRequest(new { Message = "At least one role must be specified" });
                }

                var validRoles = new HashSet<string> { "User", "Admin", "SuperAdmin" };
                if (!updateRolesDTO.RoleNames.All(r => validRoles.Contains(r)))
                {
                    return BadRequest(new { Message = "One or more roles are invalid" });
                }
                // Ensure roles exist before adding
                foreach (var roleName in updateRolesDTO.RoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new Roles { Name = roleName });
                    }

                    await _userManager.AddToRoleAsync(user, roleName);
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, updateRolesDTO.RoleNames);

                return Ok(new { Message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating user roles" });
            }
        }

        #endregion


    }
}
