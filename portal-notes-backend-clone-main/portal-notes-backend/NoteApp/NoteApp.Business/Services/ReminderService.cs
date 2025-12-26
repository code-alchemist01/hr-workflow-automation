using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using NoteApp.Business.DTOs;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using NoteApp.DataAccess.Contexts;

namespace NoteApp.Business.Services
{
    public class ReminderService
    {
        private readonly IRepository<Reminder> _reminderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly NoteAppDbContext _context;

        public ReminderService(IRepository<Reminder> reminderRepository, IUnitOfWork unitOfWork, IMapper mapper, NoteAppDbContext context)
        {
            _reminderRepository = reminderRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<ReminderResponseDTO>> GetAllRemindersAsync()
        {
            var reminders = await _reminderRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReminderResponseDTO>>(reminders);
        }

        public async Task<ReminderResponseDTO?> GetReminderByIdAsync(int id)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            return reminder == null ? null : _mapper.Map<ReminderResponseDTO>(reminder);
        }

        public async Task<ReminderResponseDTO> CreateReminderAsync(ReminderCreateDTO dto)
        {
            var reminder = _mapper.Map<Reminder>(dto);
            await _reminderRepository.AddAsync(reminder);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReminderResponseDTO>(reminder);
        }

        public async Task<ReminderResponseDTO?> UpdateReminderAsync(int id, ReminderUpdateDTO dto)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null) return null;

            _mapper.Map(dto, reminder);
            await _reminderRepository.UpdateAsync(reminder);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReminderResponseDTO>(reminder);
        }

        public async Task<bool> DeleteReminderAsync(int id)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null) return false;

            await _reminderRepository.DeleteAsync(reminder);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReminderResponseDTO>> GetRemindersByNoteIdAsync(int noteId)
        {
            var reminders = await _reminderRepository.FindAsync(r => r.NoteId == noteId);
            return _mapper.Map<IEnumerable<ReminderResponseDTO>>(reminders);
        }

        public async Task<IEnumerable<ReminderResponseDTO>> GetUserRemindersAsync(int userId)
        {
            var reminders = await _context.Reminders
                .Include(r => r.Note)
                .ThenInclude(n => n!.Folder)
                .Where(r => r.Note!.Folder!.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReminderResponseDTO>>(reminders);
        }
    }
}
