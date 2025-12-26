using AutoMapper;
using NoteApp.Business.DTOs.NoteShare;
using NoteApp.DataAccess.Entities;

namespace NoteApp.Business.Mappings
{
    public class NoteShareMappingProfile : Profile
    {
        public NoteShareMappingProfile()
        {
            CreateMap<NoteShare, NoteShareResponseDTO>();
            
            CreateMap<NoteShareCreateDTO, NoteShare>()
                .ForMember(dest => dest.SharedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            
            CreateMap<NoteShareUpdateDTO, NoteShare>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
} 