using AutoMapper;
using DeliveryManagementSystem.BLL.Services;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Roles> _roleRepository;
        private readonly EmailServices _emailServices;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            IGenericRepository<User> userRepository,
            IGenericRepository<Roles> roleRepository,
            EmailServices emailServices,
            IMapper mapper,
            UserManager<User> userManager,
            RoleManager<Roles> roleManager,
            SignInManager<User> signInManager, IOptions<JwtSettings> jwtOptions)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _emailServices = emailServices;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtSettings = jwtOptions.Value;
        }
        // Register - Logout - addRole - forgetPassword - resetPassword - changePassword  

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUserDTO loginUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginUserDTO.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginUserDTO.Password))
            {
                // to prevent user enumeration attacks
                await Task.Delay(100); // Small delay to prevent timing attacks
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // Check if user account is locked or disabled
            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized(new { Message = "Account is temporarily locked" });
            }
            //if (!user.EmailConfirmed && _userManager.Options.SignIn.RequireConfirmedEmail)
            //{
            //    return Unauthorized(new { Message = "Please confirm your email address" });
            //}
            try
            {
                var token = await GenerateJwtTokenAsync(user);
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    token = token.TokenString,
                    expiration = token.Expiration,
                    user = _mapper.Map<UserDTO>(user)
                });
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }
        
        private async Task<(string TokenString, DateTime Expiration)> 
            GenerateJwtTokenAsync(User user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames
                .Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            userClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            // Get signing key from configuration (not hardcoded)
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: userClaims,
                expires: expiration,
                signingCredentials: credentials,
                notBefore: DateTime.UtcNow // Token not valid before this time
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiration);
        }

        // Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDTO registerUserDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var isUserExist =
                _userRepository.FindByCondition(u => u.Email == registerUserDTO.Email)
                .FirstOrDefault();

                if (isUserExist != null)
                {
                    return BadRequest(new { Message = "User already exists" });
                }
                var user = _mapper.Map<User>(registerUserDTO);
                var result = await _userManager.CreateAsync(user, registerUserDTO.Password);
                if (!result.Succeeded)
                {
                    // Return detailed validation errors
                    var errors = result.Errors.Select(e => e.Description).ToArray();
                    return BadRequest(new
                    {
                        Message = "User registration failed",
                        Errors = errors
                    });
                }
                else
                {
                    // Assign default role
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new Roles { Name = "User" });
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                }
                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                if (string.IsNullOrEmpty(emailConfirmationToken))
                {
                   // _logger.LogError("Failed to generate email confirmation token for user {UserId}", user.Id);
                    return StatusCode(500, new { Message = "Error generating email confirmation token" });
                }

                // URL encode the token to handle special characters
                var encodedToken = Uri.EscapeDataString(emailConfirmationToken);
                var encodedEmail = Uri.EscapeDataString(user.Email);

                // Create confirmation link with proper URL construction
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?token={encodedToken}&email={encodedEmail}";
                try
                {
                    await _emailServices.SendEmailConfirmationAsync(user.Email, user.UserName, confirmationLink);
                }
                catch (Exception ex)
                {
                  //  _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                    // Don't fail registration if email sending fails
                }
                // _logger.LogInformation("User {Email} registered successfully", user.Email);

                // Return success response (DON'T return the token in production for security)
                return Ok(new
                {
                    Message = "User registered successfully. Please check your email to confirm your registration.",
                    UserId = user.Id,
                    Email = user.Email
                    // Remove token from response for security - only send via email
                });
            }
            catch(Exception)
            {
                return StatusCode(500, new { Message = "An error occurred during registration. Please try again later." });
            }
        }
        
        // Email confirmation endpoint
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new { Message = "Invalid confirmation link" });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new { Message = "Invalid confirmation link" });
                }
                if (user.EmailConfirmed)
                {
                    return Ok(new { Message = "Email already confirmed" });
                }
                // Decode the token
                var decodedToken = Uri.UnescapeDataString(token);
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                   // _logger.LogInformation("Email confirmed successfully for user {UserId}", user.Id);
                    return Ok(new { Message = "Email confirmed successfully. You can now log in." });
                }
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { Message = "Email confirmation failed", Errors = errors });
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error confirming email for {Email}", email);
                return StatusCode(500, new { Message = "An error occurred during email confirmation" });
            }
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationDTO resendDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(resendDto.Email);
                if (user == null)
                {
                    // Don't reveal if user exists or not
                    return Ok(new { Message = "If the email exists, a confirmation link has been sent." });
                }

                if (user.EmailConfirmed)
                {
                    return BadRequest(new { Message = "Email is already confirmed" });
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var encodedEmail = Uri.EscapeDataString(user.Email);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?token={encodedToken}&email={encodedEmail}";

                await _emailServices.SendEmailConfirmationAsync(user.Email, user.UserName, confirmationLink);

                return Ok(new { Message = "Confirmation email has been resent. Please check your email." });
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error resending confirmation email for {Email}", resendDto.Email);
                return StatusCode(500, new { Message = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("logout")]
        [Authorize] // Ensure only authenticated users can logout
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out the user
                await _signInManager.SignOutAsync();

                // Optional: Log the logout event
                // _logger.LogInformation("User {UserId} ({UserName}) logged out successfully at {Timestamp}",
                // userId, userName, DateTime.UtcNow);

                foreach (var cookieName in new[] { "RememberMe", "PreferredLanguage", "Theme" })
                {
                    if (Request.Cookies.ContainsKey(cookieName))
                    {
                        Response.Cookies.Delete(cookieName);
                    }
                }
                return Ok(new
                {
                    message = "Logged out successfully",
                    timestamp = DateTime.UtcNow,
                    success = true
                });
            }

            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error occurred during logout for user {UserId}",
                // User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Even if logout fails, we should still return success to prevent user confusion
                // The client should treat this as successful logout
                return Ok(new
                {
                    message = "Logout completed",
                    success = true
                });
            }

        }   
    
    
    }
}
