using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using NoteApp.Business.Validators;
using FluentValidation;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NoteImageController : ControllerBase
    {
        private readonly NoteImageService _noteImageService;
        private readonly IValidator<NoteImageCreateDTO> _createValidator;
        private readonly IValidator<NoteImageUpdateDTO> _updateValidator;

        public NoteImageController(
            NoteImageService noteImageService,
            IValidator<NoteImageCreateDTO> createValidator,
            IValidator<NoteImageUpdateDTO> updateValidator)
        {
            _noteImageService = noteImageService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteImageResponseDTO>>> GetAllNoteImages()
        {
            var noteImages = await _noteImageService.GetAllNoteImagesAsync();
            return Ok(noteImages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteImageResponseDTO>> GetNoteImageById(int id)
        {
            var noteImage = await _noteImageService.GetNoteImageByIdAsync(id);
            if (noteImage == null) return NotFound();
            return Ok(noteImage);
        }

        [HttpGet("note/{noteId}")]
        public async Task<ActionResult<IEnumerable<NoteImageResponseDTO>>> GetNoteImagesByNoteId(int noteId)
        {
            var noteImages = await _noteImageService.GetNoteImagesByNoteIdAsync(noteId);
            return Ok(noteImages);
        }

        [HttpPost]
        public async Task<ActionResult<NoteImageResponseDTO>> CreateNoteImage(NoteImageCreateDTO noteImageDto)
        {
            var validationResult = await _createValidator.ValidateAsync(noteImageDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var noteImage = await _noteImageService.CreateNoteImageAsync(noteImageDto);
                return CreatedAtAction(nameof(GetNoteImageById), new { id = noteImage.Id }, noteImage);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoteImageResponseDTO>> UpdateNoteImage(int id, NoteImageUpdateDTO noteImageDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(noteImageDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var noteImage = await _noteImageService.UpdateNoteImageAsync(id, noteImageDto);
            if (noteImage == null) return NotFound();
            return Ok(noteImage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNoteImage(int id)
        {
            var result = await _noteImageService.DeleteNoteImageAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
} 