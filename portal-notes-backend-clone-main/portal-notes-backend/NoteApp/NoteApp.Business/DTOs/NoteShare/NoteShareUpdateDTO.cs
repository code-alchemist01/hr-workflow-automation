using System;

namespace NoteApp.Business.DTOs.NoteShare
{
    public class NoteShareUpdateDTO
    {
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
} 