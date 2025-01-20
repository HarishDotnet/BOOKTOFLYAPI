namespace BookToFlyAPI.DTO.FlightDTO
{
    public class FlightInputDTO
    {
        public string FlightNumber { get; set; }
        public string FlightName { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public int AvailableSeats { get; set; }

        public decimal TicketPrice { get; set; }

        public TimeSpan DepartureTime { get; set; }

        public TimeSpan ArrivalTime { get; set; }

    }
}