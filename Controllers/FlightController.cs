using Microsoft.AspNetCore.Mvc;
using BookToFlyAPI.Data;
using BookToFlyAPI.Models;
using Microsoft.AspNetCore.Authorization;
using BookToFlyAPI.DTO.FlightDTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace BookToFlyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public FlightDetailsController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet("DisplayFlightsByType/{FlightType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllFlights(string FlightType)
        {
            if (!(FlightType.Trim().ToUpper().Equals("IF") || FlightType.Trim().ToUpper().Equals("DF")))
            {
                return BadRequest(new { message = "Enter IF for International Flight or DF for Domestic Flight." });
            }

            var flights = await _dbContext.flightDetails
                .Where(f => f.FlightNumber.StartsWith(FlightType.Trim().ToUpper()))
                .ToListAsync();

            if (!flights.Any())
            {
                return NotFound("FlightType Not Found.");
            }

            return Ok(flights);
        }

        [HttpGet("GetFlightDetails/{flightNumber}")]
        public async Task<IActionResult> GetFlightByNumber(string flightNumber)
        {
            var flight = await _dbContext.flightDetails.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (flight == null)
            {
                return NotFound(new { Message = "Flight not found." });
            }

            return Ok(flight);
        }

        [HttpPost("AddFlight")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FlightDetails>> CreateFlight([FromBody] FlightInputDTO flightDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flightDetail = _mapper.Map<FlightDetails>(flightDetails);

            if (flightDetail.FlightNumber.StartsWith("DF"))
            {
                flightDetail.FlightType = "Domestic Flight";
            }
            else if (flightDetail.FlightNumber.StartsWith("IF"))
            {
                flightDetail.FlightType = "International Flight";
            }
            else
            {
                return BadRequest(new { Message = "Invalid FlightNumber format. It must start with 'DF' or 'IF'." });
            }

            await _dbContext.flightDetails.AddAsync(flightDetail);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFlightByNumber), 
            new { flightNumber = flightDetail.FlightNumber },
            new {success = true,
                 message = "Flight created successfully.",
                 data = flightDetail
            });
        }

        [HttpPut("UpdateFlight/{flightNumber}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFlight(string flightNumber, [FromBody] FlightDetails updatedFlight)
        {
            if (flightNumber != updatedFlight.FlightNumber)
            {
                return BadRequest("Flight number in the URL does not match the flight number in the body.");
            }

            var existingFlight = await _dbContext.flightDetails.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (existingFlight == null)
            {
                return NotFound($"Flight with flight number {flightNumber} not found.");
            }

            existingFlight.FlightName = updatedFlight.FlightName;
            existingFlight.Source = updatedFlight.Source;
            existingFlight.Destination = updatedFlight.Destination;
            existingFlight.AvailableSeats = updatedFlight.AvailableSeats;
            existingFlight.TicketPrice = updatedFlight.TicketPrice;
            existingFlight.DepartureTime = updatedFlight.DepartureTime;
            existingFlight.ArrivalTime = updatedFlight.ArrivalTime;
            existingFlight.FlightType = updatedFlight.FlightType;

            await _dbContext.SaveChangesAsync();

            return Ok("Flight updated successfully.");
        }

        [HttpDelete("DeleteFlight/{flightNumber}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFlight(string flightNumber)
        {
            var flight = await _dbContext.flightDetails.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (flight == null)
            {
                return NotFound(new { Message = "Flight not found." });
            }

            _dbContext.flightDetails.Remove(flight);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Flight deleted successfully." });
        }
    }
}
