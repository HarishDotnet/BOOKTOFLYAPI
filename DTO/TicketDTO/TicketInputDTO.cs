
using System.ComponentModel.DataAnnotations;
namespace BookToFlyAPI.DTO.TicketDTO
{
    public class TicketInputDTO
    {
        public string FlightNumber { get; set; }
        public string PassangerName { get; set; }
        public int PassangerAge { get; set; }
        public DateTime DateOfJourney { get; set; }
    }
}
