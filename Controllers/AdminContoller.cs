using BookToFlyAPI.Data;
using BookToFlyAPI.Models;
using BookToFlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookToFlyAPI.Controllers
{
    [ApiController]
    [Route("Api/BookToFly")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AdminController> _logger;
        private readonly JWTTokenService _jwtTokenService;

        public AdminController(ApplicationDbContext dbContext, ILogger<AdminController> logger, JWTTokenService jwtTokenService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] Admin admin)
        {
            try
            {
                if (await _dbContext.admins.AnyAsync(u => u.AdminName.ToLower() == admin.AdminName.ToLower()))
                {
                    _logger.LogWarning($"Admin {admin.AdminName} already exists.");
                    return BadRequest(new { message = "AdminName already exists" });
                }

                admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                await _dbContext.AddAsync(admin);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Admin {admin.AdminName} registered successfully.");
                return Ok(new { success = true, message = "Admin registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin registration");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            try
            {
                var admin = await _dbContext.admins.SingleOrDefaultAsync(x => x.AdminName.ToLower() == request.Username.ToLower());
                if (admin == null)
                {
                    _logger.LogWarning($"Login failed. AdminName {request.Username} not found.");
                    return Unauthorized(new { message = "Invalid adminname or password." });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
                {
                    _logger.LogWarning($"Login failed. Incorrect password for admin {request.Username}.");
                    return Unauthorized(new { message = "Invalid adminname or password." });
                }

                var token = _jwtTokenService.GenerateJWTToken(admin.AdminName, "Admin");

                _logger.LogInformation($"Admin {admin.AdminName} logged in successfully.");
                return Ok(new AuthenticationResponse
                {
                    Token = token,
                    ExpiresIn = 3600 // Expiration time in seconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for admin {adminname}", request.Username);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("getTokenForPassword/{adminName}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTokenForPassword(string adminName)
        {
            try
            {
                var admin = await _dbContext.admins.SingleOrDefaultAsync(a => a.AdminName == adminName);
                if (admin == null)
                {
                    _logger.LogWarning($"AdminName {adminName} not found.");
                    return NotFound(new { message = "AdminName not found" });
                }

                var token = _jwtTokenService.GenerateJWTToken(admin.AdminName, "AdminChange");

                _logger.LogInformation($"Password reset token generated for Admin {admin.AdminName}.");
                return Ok(new AuthenticationResponse
                {
                    Token = token,
                    ExpiresIn = 300 // Token valid for 5 minutes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating token for password reset.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("changePassword")]
        [Authorize(Roles = "AdminChange")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var adminName = User.Identity?.Name;

                var admin = await _dbContext.admins.SingleOrDefaultAsync(a => a.AdminName.ToLower() == adminName.ToLower());
                if (admin == null)
                {
                    _logger.LogWarning($"Change password failed. AdminName {adminName} not found.");
                    return Unauthorized(new { message = "Unauthorized access" });
                }

                admin.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _dbContext.Update(admin);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Admin {admin.AdminName} changed password successfully.");
                return Ok(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change for admin.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
