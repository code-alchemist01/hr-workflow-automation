// NoteApp.Business/Services/UserService.cs

using System; // Random için eklendi
using System.Collections.Generic;
using System.Linq; // FirstOrDefault için eklendi
using System.Threading.Tasks;
using AutoMapper;
using NoteApp.Business.DTOs;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace NoteApp.Business.Services
{
    public class UserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly NoteAppDbContext _context;
        private readonly IEmailService _emailService; // <-- EKLENDÝ

        public UserService(
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            NoteAppDbContext context,
            IEmailService emailService) // <-- emailService PARAMETRESÝ EKLENDÝ
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _emailService = emailService; // <-- EKLENDÝ
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDTO>>(users);
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO?> GetUserByEmailAsync(string email)
        {
            var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
            return user == null ? null : _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO?> AuthenticateUserAsync(string email, string password)
        {
            var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
            if (user == null) return null;
            if (!VerifyPassword(password, user.Password!)) return null;
            return _mapper.Map<UserResponseDTO>(user);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.Password = HashPassword(userDto.Password!);
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO?> UpdateUserAsync(int id, UserUpdateDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            _mapper.Map(userDto, user);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            await _userRepository.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserProfileImageUrlAsync(int userId, string imageUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return false;
            }
            user.ProfileImageUrl = imageUrl;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Kullanýcý bulunamadý.");
            }

            if (!VerifyPassword(oldPassword, user.Password!))
            {
                return (false, "Mevcut þifreniz yanlýþ.");
            }

            if (oldPassword == newPassword)
            {
                return (false, "Yeni þifreniz, mevcut þifreniz ile ayný olamaz.");
            }

            user.Password = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Þifre baþarýyla güncellendi.");
        }

        // Bu eski metot artýk kullanýlmýyor ama dursun zararý olmaz.
        public async Task<(bool Success, string Message)> GeneratePasswordResetTokenAsync(string email)
        {
            var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
            if (user == null)
            {
                return (true, "Eðer e-posta adresiniz sistemimizde kayýtlýysa, þifre sýfýrlama baðlantýsý gönderilmiþtir.");
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Bu metot artýk doðrudan _emailService kullanmýyor, bu yüzden Console.WriteLine olarak kalabilir.
            Console.WriteLine("---- ÞÝFRE SIFIRLAMA E-POSTASI ----");
            Console.WriteLine($"Kime: {email}");
            Console.WriteLine($"Sýfýrlama Linki: noteapp://reset-password/{token}");
            Console.WriteLine("----------------------------------");

            return (true, "Eðer e-posta adresiniz sistemimizde kayýtlýysa, þifre sýfýrlama baðlantýsý gönderilmiþtir.");
        }

        // --- YENÝ ÞÝFRE SIFIRLAMA AKIÞI ---

        public async Task<(bool Success, string Message)> GeneratePasswordResetCodeAsync(string email)
        {
            var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
            if (user == null)
            {
                return (true, "Eðer e-posta adresiniz kayýtlýysa, doðrulama kodu gönderilmiþtir.");
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.PasswordResetToken = code;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendPasswordResetCodeAsync(email, code); // _emailService artýk burada kullanýlýyor.

            return (true, "Eðer e-posta adresiniz kayýtlýysa, doðrulama kodu gönderilmiþtir.");
        }

        public async Task<(bool Success, string Message, string? ResetToken)> VerifyPasswordResetCodeAsync(string email, string code)
        {
            var user = (await _userRepository.FindAsync(u => u.Email == email &&
                                                            u.PasswordResetToken == code &&
                                                            u.ResetTokenExpires > DateTime.UtcNow)).FirstOrDefault();

            if (user == null)
            {
                return (false, "Geçersiz veya süresi dolmuþ doðrulama kodu.", null);
            }

            var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Kod baþarýyla doðrulandý.", resetToken);
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            var user = (await _userRepository.FindAsync(u => u.PasswordResetToken == token && u.ResetTokenExpires > DateTime.UtcNow)).FirstOrDefault();

            if (user == null)
            {
                return (false, "Geçersiz veya süresi dolmuþ þifre sýfýrlama anahtarý.");
            }

            user.Password = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Þifreniz baþarýyla güncellendi.");
        }
    }
}