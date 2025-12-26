using Microsoft.EntityFrameworkCore;
using NoteApp.DataAccess.Entities;

namespace NoteApp.DataAccess.Contexts
{
    public class NoteAppDbContext : DbContext
    {
        public NoteAppDbContext(DbContextOptions<NoteAppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteImage> NoteImages { get; set; }
        public DbSet<NoteAudio> NoteAudios { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NoteTag> NoteTags { get; set; }
        public DbSet<FolderTag> FolderTags { get; set; }
        public DbSet<NoteShare> NoteShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Folders)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Folder>()
                .HasMany(f => f.Notes)
                .WithOne(n => n.Folder)
                .HasForeignKey(n => n.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasMany(n => n.NoteImages)
                .WithOne(ni => ni.Note)
                .HasForeignKey(ni => ni.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasMany(n => n.NoteAudios)
                .WithOne(na => na.Note)
                .HasForeignKey(na => na.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasMany(n => n.Reminders)
                .WithOne(r => r.Note)
                .HasForeignKey(r => r.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteTag>()
                .HasKey(nt => new { nt.NoteId, nt.TagId });

            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoteShare ilişkileri
            modelBuilder.Entity<NoteShare>()
                .HasOne(ns => ns.Note)
                .WithMany(n => n.Shares)
                .HasForeignKey(ns => ns.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteShare>()
                .HasOne(ns => ns.SharedWithUser)
                .WithMany(u => u.SharedNotes)
                .HasForeignKey(ns => ns.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FolderTag ilişkileri
            modelBuilder.Entity<FolderTag>()
                .HasKey(ft => new { ft.FolderId, ft.TagId });

            modelBuilder.Entity<FolderTag>()
                .HasOne(ft => ft.Folder)
                .WithMany(f => f.FolderTags)
                .HasForeignKey(ft => ft.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FolderTag>()
                .HasOne(ft => ft.Tag)
                .WithMany(t => t.FolderTags)
                .HasForeignKey(ft => ft.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 