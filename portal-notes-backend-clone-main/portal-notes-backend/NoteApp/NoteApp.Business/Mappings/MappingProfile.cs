using AutoMapper;
using NoteApp.Business.DTOs;
using NoteApp.Business.DTOs.Tag;
using NoteApp.DataAccess.Entities;

namespace NoteApp.Business.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Note mappings
            CreateMap<Note, NoteResponseDTO>();
            CreateMap<NoteCreateDTO, Note>();
            CreateMap<NoteUpdateDTO, Note>();

            // Folder mappings
            CreateMap<Folder, FolderResponseDTO>();
            CreateMap<FolderCreateDTO, Folder>();
            CreateMap<FolderUpdateDTO, Folder>();

            // User mappings
            CreateMap<User, UserResponseDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>();

            // NoteImage mappings
            CreateMap<NoteImage, NoteImageResponseDTO>();
            CreateMap<NoteImageCreateDTO, NoteImage>();
            CreateMap<NoteImageUpdateDTO, NoteImage>();

            // Reminder mappings
            CreateMap<Reminder, ReminderResponseDTO>();
            CreateMap<ReminderCreateDTO, Reminder>();
            CreateMap<ReminderUpdateDTO, Reminder>();

            // Audio mappings
            CreateMap<NoteAudio, AudioResponseDTO>();
            CreateMap<AudioCreateDTO, NoteAudio>();
            CreateMap<AudioUpdateDTO, NoteAudio>();

            // Tag mappings
            CreateMap<Tag, TagResponseDTO>();
            CreateMap<TagCreateDto, Tag>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<TagUpdateDto, Tag>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
} 