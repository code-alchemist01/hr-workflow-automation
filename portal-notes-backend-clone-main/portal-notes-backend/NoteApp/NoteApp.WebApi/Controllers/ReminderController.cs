using Microsoft.AspNetCore.Mvc;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly ReminderService _reminderService;
        private readonly IValidator<ReminderCreateDTO> _createValidator;
        private readonly IValidator<ReminderUpdateDTO> _updateValidator;

        public ReminderController(ReminderService reminderService,
                                  IValidator<ReminderCreateDTO> createValidator,
                                  IValidator<ReminderUpdateDTO> updateValidator)
        {
            _reminderService = reminderService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReminderResponseDTO>>> GetAllReminders()
        {
            var reminders = await _reminderService.GetAllRemindersAsync();
            return Ok(reminders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReminderResponseDTO>> GetReminderById(int id)
        {
            var reminder = await _reminderService.GetReminderByIdAsync(id);
            if (reminder == null)
                return NotFound();
            return Ok(reminder);
        }

        [HttpGet("reminder/{noteId}")]
        public async Task<ActionResult<IEnumerable<ReminderResponseDTO>>> GetRemindersByNoteId(int noteId)
        {
            var reminders = await _reminderService.GetRemindersByNoteIdAsync(noteId);
            return Ok(reminders);
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<ReminderResponseDTO>>> GetCurrentUserReminders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var reminders = await _reminderService.GetUserRemindersAsync(userId);
            return Ok(reminders);
        }

        [HttpPost]
        public async Task<ActionResult<ReminderResponseDTO>> CreateReminder(ReminderCreateDTO reminderDto)
        {
            var validationResult = await _createValidator.ValidateAsync(reminderDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var createdReminder = await _reminderService.CreateReminderAsync(reminderDto);
            return CreatedAtAction(nameof(GetReminderById), new { id = createdReminder.Id }, createdReminder);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReminderResponseDTO>> UpdateReminder(int id, ReminderUpdateDTO reminderDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(reminderDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var updatedReminder = await _reminderService.UpdateReminderAsync(id, reminderDto);
            if (updatedReminder == null)
                return NotFound();

            return Ok(updatedReminder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            var result = await _reminderService.DeleteReminderAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
