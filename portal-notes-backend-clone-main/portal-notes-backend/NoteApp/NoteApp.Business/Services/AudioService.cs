using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;

namespace NoteApp.Business.Services
{
    public interface IAudioService
    {
        Task<NoteAudio> SaveAudioAsync(int noteId, IFormFile audioFile);
        Task<NoteAudio> UpdateAudioAsync(int audioId, IFormFile audioFile);
        Task<NoteAudio> GetAudioAsync(int audioId);
        Task<FileStream> GetAudioFileAsync(int audioId);
        Task DeleteAudioAsync(int audioId);
        Task<IEnumerable<NoteAudio>> GetAllAudiosAsync();
    }

    public class AudioService : IAudioService
    {
        private readonly IRepository<NoteAudio> _audioRepository;
        private readonly IRepository<Note> _noteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _audioStoragePath;
        private readonly string[] _allowedExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public AudioService(
            IRepository<NoteAudio> audioRepository,
            IRepository<Note> noteRepository,
            IUnitOfWork unitOfWork)
        {
            _audioRepository = audioRepository;
            _noteRepository = noteRepository;
            _unitOfWork = unitOfWork;
            // YENİ VE GÜVENLİ KOD BURAYA YAPIŞTIRILDI
            // Projenin çalıştığı ana dizini alıyoruz.
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Bu dizin null veya boş gelirse, varsayılan bir yol kullan veya hata fırlat.
            if (string.IsNullOrEmpty(baseDirectory))
            {
                // Bu, uygulamanın çalışmasını engelleyecek kritik bir hata olduğu için
                // bir exception fırlatmak en mantıklısıdır.
                throw new InvalidOperationException("Uygulamanın ana çalışma dizini alınamadı.");
            }

            // Güvenli bir şekilde üst dizine çıkmaya çalışalım.
            // Bu kod projenin bin/Debug/net9.0 klasöründen 3 seviye yukarı çıkarak ana proje klasörünü bulmayı hedefler.
            var projectRootPath = Directory.GetParent(baseDirectory)?.Parent?.Parent?.FullName;

            // Eğer bir şekilde proje kök dizini bulunamazsa, yine bir önlem alalım.
            if (string.IsNullOrEmpty(projectRootPath))
            {
                // Alternatif olarak, ana dizini kullanabiliriz veya yine hata fırlatabiliriz.
                // Şimdilik ana dizinin içinde bir klasör oluşturalım.
                projectRootPath = baseDirectory;
                // veya throw new InvalidOperationException("Proje kök dizini bulunamadı.");
            }

            // Artık projectRootPath'in null olmadığından eminiz.
            _audioStoragePath = Path.Combine(projectRootPath, "AudioFiles");

            if (!Directory.Exists(_audioStoragePath))
            {
                Directory.CreateDirectory(_audioStoragePath);
            }
        }

        public async Task<NoteAudio> SaveAudioAsync(int noteId, IFormFile audioFile)
        {
            var note = await _noteRepository.GetByIdAsync(noteId);
            if (note == null)
            {
                throw new ArgumentException("Note not found");
            }

            ValidateAudioFile(audioFile);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(audioFile.FileName)}";
            var filePath = Path.Combine(_audioStoragePath, fileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Create audio record
            var noteAudio = new NoteAudio
            {
                NoteId = noteId,
                AudioUrl = fileName,
                FileName = audioFile.FileName,
                FileSize = audioFile.Length,
                ContentType = audioFile.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            await _audioRepository.AddAsync(noteAudio);
            await _unitOfWork.SaveChangesAsync();
            return noteAudio;
        }

        public async Task<NoteAudio> UpdateAudioAsync(int audioId, IFormFile audioFile)
        {
            var existingAudio = await GetAudioAsync(audioId);
            ValidateAudioFile(audioFile);

            // Delete old file
            // --- YENİ GÜVENLİK KONTROLÜ BURADA ---
            if (!string.IsNullOrEmpty(existingAudio.AudioUrl))
            {
                var oldFilePath = Path.Combine(_audioStoragePath, Path.GetFileName(existingAudio.AudioUrl));
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            // Generate new filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(audioFile.FileName)}";
            var newFilePath = Path.Combine(_audioStoragePath, fileName);

            // Save new file
            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Update audio record
            existingAudio.AudioUrl = fileName;
            existingAudio.FileName = audioFile.FileName;
            existingAudio.FileSize = audioFile.Length;
            existingAudio.ContentType = audioFile.ContentType;

            await _audioRepository.UpdateAsync(existingAudio);
            await _unitOfWork.SaveChangesAsync();
            return existingAudio;
        }

        private void ValidateAudioFile(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new ArgumentException("Invalid audio file");
            }

            if (audioFile.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
            }

            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }
        }

        public async Task<NoteAudio> GetAudioAsync(int audioId)
        {
            var audio = await _audioRepository.GetByIdAsync(audioId);
            if (audio == null)
            {
                throw new ArgumentException("Audio not found");
            }
            return audio;
        }

        public async Task<FileStream> GetAudioFileAsync(int audioId)
        {
            var audio = await GetAudioAsync(audioId);
            // --- YENİ GÜVENLİK KONTROLÜ BURADA ---
            if (string.IsNullOrEmpty(audio.AudioUrl))
            {
                // Eğer dosyanın yolu veritabanında kayıtlı değilse, bu bir "bulunamadı" durumudur.
                throw new FileNotFoundException("Audio file URL is not available in the database.");
            }
            // ------------------------------------

            var filePath = Path.Combine(_audioStoragePath, audio.AudioUrl);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Audio file not found on disk");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public async Task DeleteAudioAsync(int audioId)
        {
            var audio = await GetAudioAsync(audioId);

            // Delete file from disk
            // --- YENİ GÜVENLİK KONTROLÜ BURADA ---
            if (!string.IsNullOrEmpty(audio.AudioUrl))
            {
                var filePath = Path.Combine(_audioStoragePath, Path.GetFileName(audio.AudioUrl));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            // ------------------------------------


            await _audioRepository.DeleteAsync(audio);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<NoteAudio>> GetAllAudiosAsync()
        {
            return await _audioRepository.GetAllAsync();
        }
    }
}