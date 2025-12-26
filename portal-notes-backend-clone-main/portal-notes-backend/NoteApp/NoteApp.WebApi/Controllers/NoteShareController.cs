using Microsoft.AspNetCore.Mvc;
using NoteApp.Business.DTOs.NoteShare;
using NoteApp.Business.Services;
using NoteApp.Business.Validators.NoteShare;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NoteShareController : ControllerBase
    {
        private readonly NoteShareService _noteShareService;
        private readonly IValidator<NoteShareCreateDTO> _createValidator;
        private readonly IValidator<NoteShareUpdateDTO> _updateValidator;

        public NoteShareController(
            NoteShareService noteShareService,
            IValidator<NoteShareCreateDTO> createValidator,
            IValidator<NoteShareUpdateDTO> updateValidator)
        {
            _noteShareService = noteShareService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteShareResponseDTO>>> GetAllNoteShares()
        {
            var noteShares = await _noteShareService.GetAllNoteSharesAsync();
            return Ok(noteShares);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteShareResponseDTO>> GetNoteShareById(int id)
        {
            var noteShare = await _noteShareService.GetNoteShareByIdAsync(id);
            if (noteShare == null) return NotFound();
            return Ok(noteShare);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNoteShare([FromBody] NoteShareCreateDTO noteShareDto)
        {
            var validationResult = await _createValidator.ValidateAsync(noteShareDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Validasyon hatası", 
                    errors = validationResult.Errors.Select(e => e.ErrorMessage) 
                });
            }

            var (success, message, data) = await _noteShareService.CreateNoteShareAsync(noteShareDto);
            
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message, data });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoteShareResponseDTO>> UpdateNoteShare(int id, NoteShareUpdateDTO noteShareDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(noteShareDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var noteShare = await _noteShareService.UpdateNoteShareAsync(id, noteShareDto);
            if (noteShare == null) return NotFound();
            return Ok(noteShare);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNoteShare(int id)
        {
            var result = await _noteShareService.DeleteNoteShareAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<NoteShareResponseDTO>>> GetCurrentUserSharedNotes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var sharedNotes = await _noteShareService.GetUserSharedNotesAsync(userId);
            return Ok(sharedNotes);
        }
    }
} 