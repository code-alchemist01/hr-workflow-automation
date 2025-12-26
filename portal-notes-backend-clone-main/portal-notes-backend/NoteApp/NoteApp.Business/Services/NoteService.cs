using System;
using System.Collections.Generic;
using System.Linq; // .Where() gibi metotlar için bu using gerekli.
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NoteApp.Business.DTOs;
using NoteApp.Business.DTOs.Tag;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;

namespace NoteApp.Business.Services
{
    public class NoteService
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly NoteAppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public NoteService(
            IRepository<Note> noteRepository,
            IRepository<Tag> tagRepository,
            NoteAppDbContext context,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService)
        {
            _noteRepository = noteRepository;
            _tagRepository = tagRepository;
            _context = context;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<NoteResponseDTO>> GetAllNotesAsync()
        {
            var notes = await _context.Notes
                .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
                .Include(n => n.NoteAudios)
                .ToListAsync();

            var noteDtos = _mapper.Map<IEnumerable<NoteResponseDTO>>(notes);

            foreach (var noteDto in noteDtos)
            {
                if (noteDto == null) continue;
                var note = notes.FirstOrDefault(n => n.Id == noteDto.Id);
                if (note == null) continue;

                noteDto.Tags = note.NoteTags?.Select(nt => _mapper.Map<TagResponseDTO>(nt.Tag)).ToList();
                noteDto.NoteAudios = note.NoteAudios?.Select(na => _mapper.Map<AudioResponseDTO>(na)).ToList();
            }

            return noteDtos;
        }

        public async Task<NoteResponseDTO?> GetNoteByIdAsync(int id)
        {
            var note = await _context.Notes
                .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
                .Include(n => n.NoteAudios)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null) return null;

            var noteDto = _mapper.Map<NoteResponseDTO>(note);
            noteDto.Tags = note.NoteTags?.Select(nt => _mapper.Map<TagResponseDTO>(nt.Tag)).ToList();
            noteDto.NoteAudios = note.NoteAudios?.Select(na => _mapper.Map<AudioResponseDTO>(na)).ToList();
            return noteDto;
        }

        public async Task<NoteResponseDTO> CreateNoteAsync(NoteCreateDTO createDto)
        {
            var note = _mapper.Map<Note>(createDto);

            if (createDto.TagIds != null && createDto.TagIds.Any())
            {
                note.NoteTags = new List<NoteTag>();
                foreach (var tagId in createDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        note.NoteTags.Add(new NoteTag { Note = note, Tag = tag });
                    }
                }
            }

            await _noteRepository.AddAsync(note);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyNoteUpdateAsync($"Yeni not oluşturuldu: {note.Title}");

            var createdNoteDto = await GetNoteByIdAsync(note.Id);
            if (createdNoteDto == null)
            {
                throw new InvalidOperationException("Yeni oluşturulan not veritabanında bulunamadı.");
            }
            return createdNoteDto;
        }

        public async Task<NoteResponseDTO?> UpdateNoteAsync(int id, NoteUpdateDTO updateDto)
        {
            var note = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                throw new ArgumentException("Note not found");

            _mapper.Map(updateDto, note);

            if (updateDto.FolderId != 0 && note.FolderId != updateDto.FolderId)
            {
                note.FolderId = updateDto.FolderId;
            }

            if (note.NoteTags != null)
            {
                _context.NoteTags.RemoveRange(note.NoteTags);
            }

            if (updateDto.TagIds != null && updateDto.TagIds.Any())
            {
                note.NoteTags = new List<NoteTag>();
                foreach (var tagId in updateDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        note.NoteTags.Add(new NoteTag { Note = note, Tag = tag });
                    }
                }
            }

            await _noteRepository.UpdateAsync(note);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyNoteUpdateAsync($"Not güncellendi: {note.Title}");

            return await GetNoteByIdAsync(note.Id);
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null) return false;

            await _noteRepository.DeleteAsync(note);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyNoteUpdateAsync($"Not silindi: {note.Title}");

            return true;
        }

        public async Task<IEnumerable<NoteResponseDTO>> GetUserNotesAsync(int userId)
        {
            var notes = await _context.Notes
                .Include(n => n.Folder)
                .Where(n => (n.Folder == null || n.Folder.UserId == userId) && !n.IsArchived)
                .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
                .Include(n => n.NoteAudios)
                .ToListAsync();

            var noteDtos = _mapper.Map<IEnumerable<NoteResponseDTO>>(notes);

            foreach (var noteDto in noteDtos)
            {
                if (noteDto == null) continue;
                var note = notes.FirstOrDefault(n => n.Id == noteDto.Id);
                if (note == null) continue;

                noteDto.Tags = note.NoteTags?.Select(nt => _mapper.Map<TagResponseDTO>(nt.Tag)).ToList();
                noteDto.NoteAudios = note.NoteAudios?.Select(na => _mapper.Map<AudioResponseDTO>(na)).ToList();
            }

            return noteDtos;
        }

        public async Task<bool> SetNoteArchiveStatusAsync(int noteId, int userId, bool isArchived)
        {
            var note = await _context.Notes
                .Include(n => n.Folder)
                .FirstOrDefaultAsync(n => n.Id == noteId && n.Folder != null && n.Folder.UserId == userId); // GÜVENLİK GÜNCELLEMESİ

            if (note == null)
            {
                return false;
            }

            note.IsArchived = isArchived;
            await _unitOfWork.SaveChangesAsync();

            var status = isArchived ? "arşivlendi" : "arşivden çıkarıldı";
            await _notificationService.NotifyNoteUpdateAsync($"Not '{note.Title}' {status}.");

            return true;
        }
        public async Task<bool> CopyNoteToFolderAsync(int userId, int noteId, int targetFolderId)
        {
            var originalNote = await _noteRepository.GetNoteWithDetailsAsync(noteId);
            if (originalNote == null || originalNote.Folder?.UserId != userId)
                return false;

            var newNote = new Note
            {
                Title = originalNote.Title + " (Kopya)",
                Content = originalNote.Content,
                Color = originalNote.Color,
                FolderId = targetFolderId,
                CreatedAt = DateTime.UtcNow,
                NoteTags = originalNote.NoteTags.Select(tag => new NoteTag { TagId = tag.TagId }).ToList(),
                NoteImages = originalNote.NoteImages?.Select(img => new NoteImage { ImageUrl = img.ImageUrl }).ToList(),
                NoteAudios = originalNote.NoteAudios.Select(audio => new NoteAudio { AudioUrl = audio.AudioUrl }).ToList()
            };

            await _noteRepository.AddAsync(newNote);
            await _unitOfWork.SaveChangesAsync(); // 🚨 BUNU EKLE

            return true;
        }




        public async Task<IEnumerable<NoteResponseDTO>> GetArchivedNotesAsync(int userId)
        {
            var notes = await _context.Notes
                .Include(n => n.Folder)
                .Where(n => n.Folder != null && n.Folder.UserId == userId && n.IsArchived) // GÜVENLİK GÜNCELLEMESİ
                .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
                .Include(n => n.NoteAudios)
                .ToListAsync();

            var noteDtos = _mapper.Map<IEnumerable<NoteResponseDTO>>(notes);

            foreach (var noteDto in noteDtos)
            {
                if (noteDto == null) continue;
                var note = notes.FirstOrDefault(n => n.Id == noteDto.Id);
                if (note == null) continue;

                noteDto.Tags = note.NoteTags?.Select(nt => _mapper.Map<TagResponseDTO>(nt.Tag)).ToList();
                noteDto.NoteAudios = note.NoteAudios?.Select(na => _mapper.Map<AudioResponseDTO>(na)).ToList();
            }

            return noteDtos;
        }
    }
}