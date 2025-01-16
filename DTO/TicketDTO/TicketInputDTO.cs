
using System.ComponentModel.DataAnnotations;
namespace BookToFlyAPI.DTO.TicketDTO
{
    public class TicketInputDTO
    {
        [Required]
        public string FlightNumber { get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z' -]{1,49}$", ErrorMessage = "Invalid name format. Only English letters, spaces, apostrophes, and hyphens are allowed.")]
        public string PassangerName { get; set; }
        [Required]
        [Range(18, 110, ErrorMessage = "Age Should be within 18 to 110")]
        public int PassangerAge { get; set; }
        [Required]
        public DateTime DateOfJourney { get; set; }
    }
}
