using System.ComponentModel.DataAnnotations;
namespace BookToFlyAPI.DTO.TicketDTO
{
    public class TicketOutputDTO
    {
        public int BookingId { get; set; }
        public string FlightNumber { get; set; }
        public string PassangerName { get; set; }
        public int PassangerAge { get; set; }
        public DateTime DateOfJourney { get; set; }
        public string FlightName { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
    }
}