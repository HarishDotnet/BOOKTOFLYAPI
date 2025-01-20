using System;
using System.ComponentModel.DataAnnotations;

namespace BookToFlyAPI.Models
{
    public class FlightDetails
    {
        [Key]
        [Required]
        [RegularExpression(@"^(DF|IF)[0-9]{1,4}$", ErrorMessage = "Flight number must start with 'DF' (Domestic Flight) or 'IF' (International Flight) followed by 1 to 4 digits.")]
        public string FlightNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Flight name cannot exceed 100 characters.")]
        public string FlightName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Source location cannot exceed 50 characters.")]
        public string Source { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Destination location cannot exceed 50 characters.")]
        public string Destination { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "Available seats should be between 1 and 500.")]
        public int AvailableSeats { get; set; }

        [Required]
        [Range(1000, 50000, ErrorMessage = "Ticket price should be between 1000 and 50000.")]
        public decimal TicketPrice { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ArrivalTime { get; set; }

        // Flight type (Domestic or International)
        [Required]
        public string FlightType{ get; set;}
        
    }
}
