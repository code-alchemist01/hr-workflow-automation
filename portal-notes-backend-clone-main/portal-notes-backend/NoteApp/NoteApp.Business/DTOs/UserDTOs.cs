namespace NoteApp.Business.DTOs
{
    public class UserCreateDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class UserUpdateDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
    public class ChangePasswordDTO
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
    public class VerifyCodeDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class LoginRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDTO
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
} 