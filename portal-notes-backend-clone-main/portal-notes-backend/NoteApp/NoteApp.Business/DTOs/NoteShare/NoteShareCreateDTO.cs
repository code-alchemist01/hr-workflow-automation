using System;

namespace NoteApp.Business.DTOs.NoteShare
{
    public class NoteShareCreateDTO
    {
        public int NoteId { get; set; }
        public int SharedWithUserId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
} 