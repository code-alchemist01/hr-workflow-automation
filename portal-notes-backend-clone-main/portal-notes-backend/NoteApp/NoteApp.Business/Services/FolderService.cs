using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NoteApp.Business.DTOs;
using NoteApp.Business.DTOs.Tag;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Entities;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;

namespace NoteApp.Business.Services
{
    public class FolderService
    {
        private readonly IRepository<Folder> _folderRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly NoteAppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FolderService(
            IRepository<Folder> folderRepository,
            IRepository<Tag> tagRepository,
            NoteAppDbContext context,
            IUnitOfWork unitOfWork, 
            IMapper mapper)
        {
            _folderRepository = folderRepository;
            _tagRepository = tagRepository;
            _context = context;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FolderResponseDTO>> GetAllFoldersAsync()
        {
            var folders = await _context.Folders
                .Include(n => n.FolderTags!)
                .ThenInclude(nt => nt.Tag)
                .ToListAsync();

            var folderDtos = _mapper.Map<IEnumerable<FolderResponseDTO>>(folders);

            foreach (var folderDto in folderDtos)
            {
                var folder = folders.First(n => n.Id == folderDto.Id);
                folderDto.Tags = folder.FolderTags?.Select(nt => _mapper.Map<TagResponseDTO>(nt.Tag)).ToList();
            }

            return folderDtos;
        }

        public async Task<FolderResponseDTO?> GetFolderByIdAsync(int id)
        {
            var folder = await _context.Folders
                .Include(f => f.FolderTags!)
                .ThenInclude(ft => ft.Tag)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (folder == null) return null;

            var folderDto = _mapper.Map<FolderResponseDTO>(folder);
            folderDto.Tags = folder.FolderTags?
                .Select(ft => _mapper.Map<TagResponseDTO>(ft.Tag))
                .ToList();

            return folderDto;
        }

        public async Task<FolderResponseDTO> CreateFolderAsync(FolderCreateDTO folderDto)
        {
            var folder = _mapper.Map<Folder>(folderDto);

            if (folderDto.TagIds != null && folderDto.TagIds.Any())
            {
                folder.FolderTags = new List<FolderTag>();
                foreach (var tagId in folderDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        folder.FolderTags.Add(new FolderTag { Folder = folder, Tag = tag });
                    }
                }
            }

            await _folderRepository.AddAsync(folder);
            await _unitOfWork.SaveChangesAsync();

            // --- DÜZELTME BAÞLANGICI ---

            // GetFolderByIdAsync'in null döndürme ihtimaline karþý sonucu bir deðiþkene alalým.
            var createdFolderDto = await GetFolderByIdAsync(folder.Id);

            // Eðer bir sebepten ötürü (çok düþük ihtimal) null dönerse, bir hata fýrlatalým.
            // Bu, metodun 'null' döndürmeyeceði sözünü tutmamýzý saðlar.
            if (createdFolderDto == null)
            {
                // Bu durumun normalde olmamasý gerekir, ama olursa bir hata fýrlatmak en doðrusu.
                throw new InvalidOperationException("Yeni oluþturulan klasör veritabanýnda bulunamadý.");
            }

            return createdFolderDto;
            // --- DÜZELTME SONU ---
        }


        public async Task<FolderResponseDTO?> UpdateFolderAsync(int id, FolderUpdateDTO folderDto)
        {
            var folder = await _context.Folders
                .Include(f => f.FolderTags)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (folder == null) return null;

            _mapper.Map(folderDto, folder);

            if (folder.FolderTags != null)
            {
                _context.FolderTags.RemoveRange(folder.FolderTags);
            }

            if (folderDto.TagIds != null && folderDto.TagIds.Any())
            {
                folder.FolderTags = new List<FolderTag>();
                foreach (var tagId in folderDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        folder.FolderTags.Add(new FolderTag { Folder = folder, Tag = tag });
                    }
                }
            }

            await _folderRepository.UpdateAsync(folder);
            await _unitOfWork.SaveChangesAsync();

            return await GetFolderByIdAsync(folder.Id);
        }


        public async Task<bool> DeleteFolderAsync(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            if (folder == null) return false;

            await _folderRepository.DeleteAsync(folder);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FolderResponseDTO>> GetUserFoldersAsync(int userId)
        {
            var folders = await _context.Folders
                .Include(f => f.FolderTags!)
                .ThenInclude(ft => ft.Tag)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            var folderDtos = _mapper.Map<IEnumerable<FolderResponseDTO>>(folders);

            foreach (var folderDto in folderDtos)
            {
                var folder = folders.First(f => f.Id == folderDto.Id);
                folderDto.Tags = folder.FolderTags?
                    .Select(ft => _mapper.Map<TagResponseDTO>(ft.Tag))
                    .ToList();
            }

            return folderDtos;
        }
    }
} 