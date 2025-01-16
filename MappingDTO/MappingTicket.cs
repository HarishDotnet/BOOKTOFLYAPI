using AutoMapper;
using BookToFlyAPI.Models;
using BookToFlyAPI.DTO.TicketDTO;
using BookToFlyAPI.DTO.FlightDTO;
namespace BookToFlyAPI.MappingDTO{
    public class MappingTicket : Profile
{
    public MappingTicket()
    {
        // Mapping from TicketInputDTO to Ticket
        CreateMap<TicketInputDTO, Ticket>();

        CreateMap<FlightInputDTO,FlightDetails>();

        // Mapping from Ticket to TicketOutputDTO
        CreateMap<Ticket, TicketOutputDTO>()
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightNumber))
            .ForMember(dest => dest.PassangerName, opt => opt.MapFrom(src => src.PassangerName))
            .ForMember(dest => dest.PassangerAge, opt => opt.MapFrom(src => src.PassangerAge))
            .ForMember(dest => dest.DateOfJourney, opt => opt.MapFrom(src => src.DateOfJourney))
            .ForMember(dest => dest.FlightName, opt => opt.MapFrom(src => src.FlightName))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination));
    }
}

}