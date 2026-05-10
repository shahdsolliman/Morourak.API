using AutoMapper;
using Morourak.Infrastructure.Identity;
using Morourak.Application.DTOs.Admin;

namespace Morourak.Infrastructure.Mapping
{
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(d => d.Role, opt => opt.Ignore()); // Roles are fetched separately in service
        }
    }
}
