using AutoMapper;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Features.Tickets.Mappings;

public class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.PriorityName, opt => opt.MapFrom(s => s.Priority.Name))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(s => s.Status.Name))
            .ForMember(d => d.CreatedByName, opt => opt.Ignore())
            .ForMember(d => d.AssignedToName, opt => opt.Ignore());

        CreateMap<Ticket, TicketListItemDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.PriorityName, opt => opt.MapFrom(s => s.Priority.Name))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(s => s.Status.Name))
            .ForMember(d => d.CreatedByName, opt => opt.Ignore())
            .ForMember(d => d.AssignedToName, opt => opt.Ignore());
    }
}
