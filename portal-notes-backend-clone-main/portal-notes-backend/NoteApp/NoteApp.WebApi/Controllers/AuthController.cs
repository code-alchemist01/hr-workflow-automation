using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteApp.Business.DTOs;
using NoteApp.Business.Services;
using System.Security.Claims;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenHelper _tokenHelper;

        public AuthController(UserService userService, TokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDto)
        {
            var user = await _userService.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            if (user == null)
                return Unauthorized("Geçersiz e-posta veya şifre.");

            var token = _tokenHelper.GenerateToken(user.Id, user.Email!);
            
            return Ok(new 
            { 
                token = token,
                user = user
            });
        }

        [HttpGet("verify")]
        [Authorize]
        public IActionResult VerifyToken() // 'async' ve 'Task<...>' kaldırıldı.
        {
            return Ok(new { valid = true });
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

        [HttpPost("verify-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDTO verifyCodeDto)
        {
            var (success, message, resetToken) = await _userService.VerifyPasswordResetCodeAsync(verifyCodeDto.Email, verifyCodeDto.Code);
            if (!success)
            {
                return BadRequest(new { success, message });
            }
            // Başarılı olursa, bir sonraki adımda kullanılacak olan token'ı frontend'e gönderiyoruz.
            return Ok(new { success, message, token = resetToken });
        }


        [HttpPost("forgot-password")]
        [AllowAnonymous] // Herkesin erişebilmesi için Authorize'ı kaldırıyoruz
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            var (success, message) = await _userService.GeneratePasswordResetCodeAsync(forgotPasswordDto.Email);
            return Ok(new { success, message });

            
            
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            var (success, message) = await _userService.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }
    }
} 