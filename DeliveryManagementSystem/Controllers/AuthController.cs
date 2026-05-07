using AutoMapper;
using DeliveryManagementSystem.BLL.Services;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IGenericRepository<User> userRepository,
        EmailServices emailServices,
        IMapper mapper,
        UserManager<User> userManager,
        RoleManager<Roles> roleManager,
        SignInManager<User> signInManager, IOptions<JwtSettings> jwtOptions) : ControllerBase
    {
        private readonly IMapper _mapper = mapper;
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly RoleManager<Roles> _roleManager = roleManager;
        private readonly IGenericRepository<User> _userRepository = userRepository;
        private readonly EmailServices _emailServices = emailServices;
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;


        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUserDTO loginUserDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginUserDTO.Email);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginUserDTO.Password);
            if (user == null || !isPasswordValid)
            {
                // rate limiting
                await Task.Delay(1000); // Small delay to prevent timing attacks
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
                    token,
                    expiration = DateTime.Now.AddDays(3),
                    user = _mapper.Map<UserDTO>(user)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }
        
        // Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDTO dto)
        {
            try
            {
                if (await IsEmailExists(dto.Email))
                    return BadRequest(new { Message = "User already exists" });

                var user = _mapper.Map<User>(dto);

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return BadRequest(new
                    {
                        Message = "User registration failed",
                        Errors = result.Errors.Select(e => e.Description)
                    });

                await AssignUserRole(user);

                await SendConfirmationEmail(user);

                return Ok(new
                {
                    Message = "User registered successfully. Please check your email.",
                    UserId = user.Id,
                    Email = user.Email
                });
            }
            catch
            {
                return StatusCode(500,
                    new { Message = "An error occurred during registration." });
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

               bool emailSent = await _emailServices.SendEmailConfirmationAsync(user.Email, user.UserName, confirmationLink);

               if (!emailSent)
               {
                    return StatusCode(500, new { Message = "Failed to send confirmation email. Please try again later." });
               }

                return Ok(new { Message = "Confirmation email has been resent. Please check your email." });
            }
            catch 
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

            catch 
            {
                return Ok(new
                {
                    message = "Logout completed",
                    success = true
                });
            }

        }

        // private helper methods 
        private async Task<string> GenerateJwtTokenAsync(User user)
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
            var expiration = DateTime.UtcNow.AddDays(3);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: userClaims,
                expires: expiration,
                signingCredentials: credentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }


        private async Task<bool> IsEmailExists(string email)
        {
            return _userRepository
                .FindByCondition(u => u.Email == email)
                .Any();
        }

        private async Task AssignUserRole(User user)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new Roles
                {
                    Name = "User"
                });
            }

            await _userManager.AddToRoleAsync(user, "User");
        }


        private async Task SendConfirmationEmail(User user)
        {
            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(user.Email);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var confirmationLink =
                $"{baseUrl}/api/Auth/confirm-email?token={encodedToken}&email={encodedEmail}";

            await _emailServices.SendEmailConfirmationAsync(
                user.Email!,
                user.UserName!,
                confirmationLink
            );
        }

    }
}
