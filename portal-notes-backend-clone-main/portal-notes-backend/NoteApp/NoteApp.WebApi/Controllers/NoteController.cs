using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using FluentValidation;
using System.Security.Claims;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NoteController : ControllerBase
    {
        private readonly NoteService _noteService;
        private readonly IValidator<NoteCreateDTO> _createValidator;
        private readonly IValidator<NoteUpdateDTO> _updateValidator;

        public NoteController(NoteService noteService, IValidator<NoteCreateDTO> createValidator, IValidator<NoteUpdateDTO> updateValidator)
        {
            _noteService = noteService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // --- Mevcut Not Endpoint'leri ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteResponseDTO>>> GetAllNotes()
        {
            // Not: Bu genellikle admin rolü için kullanılır.
            var notes = await _noteService.GetAllNotesAsync();
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteResponseDTO>> GetNoteById(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return Ok(note);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<NoteResponseDTO>> CreateNote(NoteCreateDTO noteDto)
        {
            var validationResult = await _createValidator.ValidateAsync(noteDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var note = await _noteService.CreateNoteAsync(noteDto);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoteResponseDTO>> UpdateNote(int id, NoteUpdateDTO noteDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(noteDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var note = await _noteService.UpdateNoteAsync(id, noteDto);
            if (note == null)
            {
                return NotFound();
            }
            return Ok(note);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNote(int id)
        {
            var result = await _noteService.DeleteNoteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<NoteResponseDTO>>> GetCurrentUserNotes()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            var notes = await _noteService.GetUserNotesAsync(userId);
            return Ok(notes);
        }

        // --- YENİ EKLENEN ARŞİV ENDPOINT'LERİ ---

        [HttpPost("{id}/archive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ArchiveNote(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            var result = await _noteService.SetNoteArchiveStatusAsync(id, userId, true);
            if (!result)
            {
                return NotFound(new { message = "Not bulunamadı veya bu nota erişim yetkiniz yok." });
            }
            return Ok(new { message = "Not başarıyla arşivlendi." });
        }

        [HttpPost("{id}/unarchive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnarchiveNote(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            var result = await _noteService.SetNoteArchiveStatusAsync(id, userId, false);
            if (!result)
            {
                return NotFound(new { message = "Not bulunamadı veya bu nota erişim yetkiniz yok." });
            }
            return Ok(new { message = "Not arşivden başarıyla çıkarıldı." });
        }

        [HttpGet("archived")]
        [ProducesResponseType(typeof(IEnumerable<NoteResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NoteResponseDTO>>> GetArchivedNotes()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            var notes = await _noteService.GetArchivedNotesAsync(userId);
            return Ok(notes);
        }
        [HttpPost("copy")]
        public async Task<IActionResult> CopyNoteToFolder([FromBody] NoteCopyDTO copyDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı doğrulanamadı.");
            }

            var result = await _noteService.CopyNoteToFolderAsync(userId, copyDto.NoteId, copyDto.TargetFolderId);

            if (!result)
            {
                return NotFound("Not bulunamadı veya bu not size ait değil.");
            }

            return Ok(new { message = "Not başarıyla kopyalandı." });
        }

    }
}