using Microsoft.EntityFrameworkCore;
using NoteApp.Business.DTOs.Tag;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Entities;
using AutoMapper;

namespace NoteApp.Business.Services
{
    public class TagService
    {
        private readonly NoteAppDbContext _context;
        private readonly IMapper _mapper;

        public TagService(NoteAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TagResponseDTO>> GetAllTagsAsync(int userId)
        {
            var tags = await _context.Tags
                .Where(t => t.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<TagResponseDTO>>(tags);
        }

        public async Task<TagResponseDTO> GetTagByIdAsync(int id, int userId)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            return _mapper.Map<TagResponseDTO>(tag);
        }

        public async Task<TagResponseDTO> CreateTagAsync(TagCreateDto tagDto, int userId)
        {
            var tag = _mapper.Map<Tag>(tagDto);
            tag.UserId = userId;
            tag.CreatedAt = DateTime.UtcNow;

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return _mapper.Map<TagResponseDTO>(tag);
        }

        public async Task<TagResponseDTO?> UpdateTagAsync(TagUpdateDto tagDto, int userId)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagDto.Id && t.UserId == userId);
                
            if (tag == null)
                return null;

            _mapper.Map(tagDto, tag);
            await _context.SaveChangesAsync();
            
            return _mapper.Map<TagResponseDTO>(tag);
        }

        public async Task<bool> DeleteTagAsync(int id, int userId)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                
            if (tag == null)
                return false;

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 