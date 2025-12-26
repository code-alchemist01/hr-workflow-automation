using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using NoteApp.Business.DTOs;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;

namespace NoteApp.Business.Services
{
    public class NoteImageService
    {
        private readonly IRepository<NoteImage> _noteImageRepository;
        private readonly IRepository<Note> _noteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NoteImageService(
            IRepository<NoteImage> noteImageRepository,
            IRepository<Note> noteRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _noteImageRepository = noteImageRepository;
            _noteRepository = noteRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NoteImageResponseDTO>> GetAllNoteImagesAsync()
        {
            var noteImages = await _noteImageRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<NoteImageResponseDTO>>(noteImages);
        }

        public async Task<NoteImageResponseDTO?> GetNoteImageByIdAsync(int id)
        {
            var noteImage = await _noteImageRepository.GetByIdAsync(id);
            return noteImage == null ? null : _mapper.Map<NoteImageResponseDTO>(noteImage);
        }

        public async Task<IEnumerable<NoteImageResponseDTO>> GetNoteImagesByNoteIdAsync(int noteId)
        {
            var noteImages = await _noteImageRepository.FindAsync(ni => ni.NoteId == noteId);
            return _mapper.Map<IEnumerable<NoteImageResponseDTO>>(noteImages);
        }

        public async Task<NoteImageResponseDTO> CreateNoteImageAsync(NoteImageCreateDTO noteImageDto)
        {
            // Note'un var olup olmadığını kontrol et
            var note = await _noteRepository.GetByIdAsync(noteImageDto.NoteId);
            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {noteImageDto.NoteId} not found.");
            }

            var noteImage = _mapper.Map<NoteImage>(noteImageDto);
            await _noteImageRepository.AddAsync(noteImage);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<NoteImageResponseDTO>(noteImage);
        }

        public async Task<NoteImageResponseDTO?> UpdateNoteImageAsync(int id, NoteImageUpdateDTO noteImageDto)
        {
            var noteImage = await _noteImageRepository.GetByIdAsync(id);
            if (noteImage == null) return null;

            _mapper.Map(noteImageDto, noteImage);
            await _noteImageRepository.UpdateAsync(noteImage);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<NoteImageResponseDTO>(noteImage);
        }

        public async Task<bool> DeleteNoteImageAsync(int id)
        {
            var noteImage = await _noteImageRepository.GetByIdAsync(id);
            if (noteImage == null) return false;

            await _noteImageRepository.DeleteAsync(noteImage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 