using System;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.DTOs.NoteShare
{
    public class NoteShareResponseDTO
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public NoteResponseDTO? Note { get; set; }
        public int SharedWithUserId { get; set; }
        public UserResponseDTO? SharedWithUser { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime SharedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
} 