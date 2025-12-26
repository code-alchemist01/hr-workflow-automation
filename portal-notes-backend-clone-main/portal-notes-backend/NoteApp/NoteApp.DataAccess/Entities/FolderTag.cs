namespace NoteApp.DataAccess.Entities
{
    public class FolderTag
    {
        public int FolderId { get; set; }
        public Folder Folder { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}