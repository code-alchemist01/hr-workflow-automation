using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NoteApp.Business.DTOs.NoteShare;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;

namespace NoteApp.Business.Services
{
    public class NoteShareService
    {
        private readonly IRepository<NoteShare> _noteShareRepository;
        private readonly NoteAppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public NoteShareService(
            IRepository<NoteShare> noteShareRepository,
            NoteAppDbContext context,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService)
        {
            _noteShareRepository = noteShareRepository;
            _context = context;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<NoteShareResponseDTO>> GetAllNoteSharesAsync()
        {
            var noteShares = await _context.NoteShares
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NoteShareResponseDTO>>(noteShares);
        }

        public async Task<NoteShareResponseDTO?> GetNoteShareByIdAsync(int id)
        {
            var noteShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .FirstOrDefaultAsync(ns => ns.Id == id);

            if (noteShare == null) return null;

            return _mapper.Map<NoteShareResponseDTO>(noteShare);
        }

        public async Task<IEnumerable<NoteShareResponseDTO>> GetUserSharedNotesAsync(int userId)
        {
            var noteShares = await _context.NoteShares
                .Include(ns => ns.Note)
                .ThenInclude(n => n!.Folder)
                .Include(ns => ns.SharedWithUser)
                .Where(ns => ns.SharedWithUserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NoteShareResponseDTO>>(noteShares);
        }

        public async Task<(bool Success, string Message, NoteShareResponseDTO? Data)> CreateNoteShareAsync(NoteShareCreateDTO noteShareDto)
        {
            try
            {
                // Önce not ve kullanıcının var olup olmadığını kontrol et
                var note = await _context.Notes.FindAsync(noteShareDto.NoteId);
                if (note == null)
                {
                    var errorMessage = $"ID'si {noteShareDto.NoteId} olan not bulunamadı.";
                    await _notificationService.NotifyNoteUpdateAsync(errorMessage);
                    return (false, errorMessage, null);
                }

                var user = await _context.Users.FindAsync(noteShareDto.SharedWithUserId);
                if (user == null)
                {
                    var errorMessage = $"ID'si {noteShareDto.SharedWithUserId} olan kullanıcı bulunamadı.";
                    await _notificationService.NotifyNoteUpdateAsync(errorMessage);
                    return (false, errorMessage, null);
                }

                // Aynı not ve kullanıcı için zaten paylaşım var mı kontrol et
                var existingShare = await _context.NoteShares
                    .FirstOrDefaultAsync(ns => ns.NoteId == noteShareDto.NoteId && 
                                             ns.SharedWithUserId == noteShareDto.SharedWithUserId);
                if (existingShare != null)
                {
                    var errorMessage = "Bu not zaten bu kullanıcı ile paylaşılmış.";
                    await _notificationService.NotifyNoteUpdateAsync(errorMessage);
                    return (false, errorMessage, null);
                }

                var noteShare = _mapper.Map<NoteShare>(noteShareDto);
                await _noteShareRepository.AddAsync(noteShare);
                await _unitOfWork.SaveChangesAsync();

                // Sadece paylaşılan kullanıcıya bildirim gönder
                var userNotificationMessage = $"{note.Id} ID'li not sizinle paylaşıldı";
                await _notificationService.NotifyUserAsync(noteShareDto.SharedWithUserId.ToString(), userNotificationMessage);

                var response = await GetNoteShareByIdAsync(noteShare.Id);
                return (true, "Not başarıyla paylaşıldı", response);
            }
            catch (DbUpdateException )
            {
                var errorMessage = "Not paylaşımı oluşturulamadı. Lütfen girdiğiniz bilgileri kontrol edin.";
                await _notificationService.NotifyNoteUpdateAsync(errorMessage);
                return (false, errorMessage, null);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyNoteUpdateAsync(ex.Message);
                return (false, ex.Message, null);
            }
        }

        public async Task<NoteShareResponseDTO?> UpdateNoteShareAsync(int id, NoteShareUpdateDTO noteShareDto)
        {
            try
            {
                var noteShare = await _noteShareRepository.GetByIdAsync(id);
                if (noteShare == null) return null;

                _mapper.Map(noteShareDto, noteShare);
                await _noteShareRepository.UpdateAsync(noteShare);
                await _unitOfWork.SaveChangesAsync();

                // Not paylaşımı güncellendiğinde bildirim gönder
                var note = await _context.Notes.FindAsync(noteShare.NoteId);
                if (note != null)
                {
                    await _notificationService.NotifyNoteUpdateAsync($"Not paylaşımı güncellendi: {note.Title}");
                }

                return await GetNoteShareByIdAsync(noteShare.Id);
            }
            catch (DbUpdateException )
            {
                throw new Exception("Not paylaşımı güncellenemedi. Lütfen girdiğiniz bilgileri kontrol edin.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Not paylaşımı güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<bool> DeleteNoteShareAsync(int id)
        {
            try
            {
                var noteShare = await _noteShareRepository.GetByIdAsync(id);
                if (noteShare == null) return false;

                // Not paylaşımı silinmeden önce not ve kullanıcı bilgilerini al
                var note = await _context.Notes.FindAsync(noteShare.NoteId);
                var sharedWithUserId = noteShare.SharedWithUserId;

                await _noteShareRepository.DeleteAsync(noteShare);
                await _unitOfWork.SaveChangesAsync();

                // Paylaşılan kullanıcıya özel bildirim gönder
                if (note != null)
                {
                    var userNotificationMessage = $"{note.Id} ID'li not artık sizinle paylaşılmıyor";
                    await _notificationService.NotifyUserAsync(sharedWithUserId.ToString(), userNotificationMessage);
                }

                return true;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Not paylaşımı silinemedi. Lütfen tekrar deneyin.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Not paylaşımı silinirken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 