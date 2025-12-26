using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using NoteApp.Business.Validators;
using FluentValidation;
using System.Security.Claims;
using NoteApp.DataAccess.Entities;
using NoteApp.Business.DTOs.Tag;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IValidator<UserCreateDTO> _createValidator;
        private readonly IValidator<UserUpdateDTO> _updateValidator;
        private readonly TagService _tagService;

        public UserController(UserService userService, IValidator<UserCreateDTO> createValidator, IValidator<UserUpdateDTO> updateValidator, TagService tagService)
        {
            _userService = userService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _tagService = tagService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetCurrentUser()
        {
            // Token'dan kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok(user);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetProfile()
        {
            // Token'dan kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDTO>> CreateUser(UserCreateDTO userDto)
        {
            var validationResult = await _createValidator.ValidateAsync(userDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var user = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(int id, UserUpdateDTO userDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(userDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var user = await _userService.UpdateUserAsync(id, userDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/tags")]
        public async Task<ActionResult<List<TagResponseDTO>>> GetMyTags()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tags = await _tagService.GetAllTagsAsync(userId);
            return Ok(tags);
        }
        [HttpPost("upload-profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen bir dosya seçin." });
            }

            // 2. Kullanıcı kimlik doğrulaması ve ID'sini alma
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }
            var userId = int.Parse(userIdString);

            // 3. Dosyayı sunucuya kaydetme
            // Güvenli ve proje kök dizinini baz alan bir yol oluşturalım.
            var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRootPath, "images", "profiles");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Dosya adı için güvenlik kontrolü
            if (string.IsNullOrEmpty(file.FileName))
            {
                return BadRequest(new { message = "Dosya adı geçersiz." });
            }

            var uniqueFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Veritabanını güncelleme
            var imageUrl = $"/images/profiles/{uniqueFileName}"; // Dışarıdan erişilecek URL yolu
            var result = await _userService.UpdateUserProfileImageUrlAsync(userId, imageUrl);

            if (!result)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı veya güncelleme başarısız oldu." });
            }

            // 5. Başarılı yanıtı frontend'e gönderme
            return Ok(new { profileImageUrl = imageUrl });
        }
        [HttpPost("change-password")]
        [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Geçersiz kullanıcı bilgisi.");
            }

            var (success, message) = await _userService.ChangePasswordAsync(userId, changePasswordDto.OldPassword!, changePasswordDto.NewPassword!);

            if (!success)
            {
                // Hatalı mesajı doğrudan servisten alıp frontend'e gönder
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
    }
} 