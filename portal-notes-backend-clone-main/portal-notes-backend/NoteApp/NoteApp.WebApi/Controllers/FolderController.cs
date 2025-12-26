using Microsoft.AspNetCore.Mvc;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using NoteApp.Business.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FolderController : ControllerBase
    {
        private readonly FolderService _folderService;
        private readonly IValidator<FolderCreateDTO> _createValidator;
        private readonly IValidator<FolderUpdateDTO> _updateValidator;

        public FolderController(FolderService folderService, IValidator<FolderCreateDTO> createValidator, IValidator<FolderUpdateDTO> updateValidator)
        {
            _folderService = folderService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FolderResponseDTO>>> GetAllFolders()
        {
            var folders = await _folderService.GetAllFoldersAsync();
            return Ok(folders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FolderResponseDTO>> GetFolderById(int id)
        {
            var folder = await _folderService.GetFolderByIdAsync(id);
            if (folder == null) return NotFound();
            return Ok(folder);
        }

        [HttpPost]
        public async Task<ActionResult<FolderResponseDTO>> CreateFolder(FolderCreateDTO folderDto)
        {
            var validationResult = await _createValidator.ValidateAsync(folderDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var folder = await _folderService.CreateFolderAsync(folderDto);
            return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, folder);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FolderResponseDTO>> UpdateFolder(int id, FolderUpdateDTO folderDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(folderDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var folder = await _folderService.UpdateFolderAsync(id, folderDto);
            if (folder == null) return NotFound();
            return Ok(folder);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFolder(int id)
        {
            var result = await _folderService.DeleteFolderAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true, message = "Folder deleted successfully" });
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<FolderResponseDTO>>> GetCurrentUserFolders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var folders = await _folderService.GetUserFoldersAsync(userId);
            return Ok(folders);
        }
    }
} 