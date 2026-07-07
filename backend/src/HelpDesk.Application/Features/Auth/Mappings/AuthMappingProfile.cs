using AutoMapper;
using HelpDesk.Application.Features.Auth.Dtos;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Application.Features.Auth.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
}
