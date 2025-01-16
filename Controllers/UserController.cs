using BookToFlyAPI.Data;
using BookToFlyAPI.Models;
using BookToFlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookToFlyAPI.DTO.TicketDTO;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace BookToFlyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JWTTokenService _jwtTokenService;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        public UserController(ApplicationDbContext dbContext, JWTTokenService jwtTokenService, ILogger<UserController> logger,IMapper mapper)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _mapper=mapper;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                if (await _dbContext.users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower()))
                {
                    _logger.LogWarning($"User {user.Username} already exists.");
                    return BadRequest(new { message = "Username already exists" });
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _dbContext.Add(user);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"User {user.Username} registered successfully.");
                return Ok(new { success = true, message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            try
            {
                var user = await _dbContext.users.SingleOrDefaultAsync(x => x.Username.ToLower() == request.Username.ToLower());
                if (user == null)
                {
                    _logger.LogWarning($"Login failed. Username {request.Username} not found.");
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogWarning($"Login failed. Incorrect password for user {request.Username}.");
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                var token = _jwtTokenService.GenerateJWTToken(user.Username,"User");

                _logger.LogInformation($"User {user.Username} logged in successfully.");
                return Ok(new AuthenticationResponse
                {
                    Token = token,
                    ExpiresIn = 3600 // Expiration time in seconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Username}", request.Username);
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("getTokenForPassword/{UserName}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTokenForPassword(string UserName)
        {
            try
            {
                var user = await _dbContext.users.SingleOrDefaultAsync(a => a.Username == UserName);
                if (user == null)
                {
                    _logger.LogWarning($"UserName {UserName} not found.");
                    return NotFound(new { message = "UserName not found" });
                }

                var token = _jwtTokenService.GenerateJWTToken(user.Username, "UserChange");

                _logger.LogInformation($"Password reset token generated for Admin {user.Username}.");
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
        [Authorize(Roles = "UserChange,User")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userName = User.Identity?.Name;

                var user = await _dbContext.users.SingleOrDefaultAsync(a => a.Username == userName);
                if (user == null)
                {
                    _logger.LogWarning($"Change password failed. UserName {userName} not found.");
                    return Unauthorized(new { message = "Unauthorized access" });
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _dbContext.Update(user);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"User {user.Username} changed password successfully.");
                return Ok(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change for User.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("BookTicket")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,User")]
        public IActionResult AddTicket([FromBody] TicketInputDTO ticketRequest)
        {
            // Validate if FlightNumber exists in the FlightDetails table
            var flightDetails = _dbContext.flightDetails.FirstOrDefault(f => f.FlightNumber == ticketRequest.FlightNumber);
            if (flightDetails == null)
            {
                return NotFound($"Flight with FlightNumber {ticketRequest.FlightNumber} not found.");
            }

            var ticket=_mapper.Map<Ticket>(ticketRequest);
            ticket.FlightName=flightDetails.FlightName;
            ticket.FlightNumber=flightDetails.FlightNumber;
            ticket.Source=flightDetails.Source;
            ticket.Destination=flightDetails.Destination;
            ticket.Username=User.Identity.Name;
            _dbContext.tickets.Add(ticket);
            _dbContext.SaveChanges();

            return Ok("Ticket added successfully.");
        }

        [HttpGet("GetTicketsByUsername/{username}")]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status200OK)]
public IActionResult GetTicketsByUsername(string username)
{
    // Retrieve the tickets for the given username
    var tickets = _dbContext.tickets.Where(t => t.Username == username).ToList();

    // If no tickets are found, return a not found response
    if (!tickets.Any()) // Simply check if the list is empty
    {
        return NotFound($"No tickets found for the user with username {username}.");
    }

    // Map the tickets to DTOs
    var ticketDtos = _mapper.Map<List<TicketOutputDTO>>(tickets);

    // Return the list of mapped tickets
    return Ok(ticketDtos);
}


    }
}