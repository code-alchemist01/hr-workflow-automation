namespace NoteApp.DataAccess.Entities
{
    public class NoteTag
    {
        public int NoteId { get; set; }
        public Note Note { get; set; } = null!; // Bu 'null!' önemli, "Bana güven, EF bunu dolduracak" demek.
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!; // Bu da ayný þekilde.
    }
} 