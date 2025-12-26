using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NoteApp.DataAccess.Contexts;

namespace NoteApp.DataAccess
{
    public class NoteAppDbContextFactory : IDesignTimeDbContextFactory<NoteAppDbContext>
    {
        public NoteAppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NoteAppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=noteapp;Username=postgres;Password=123456");

            return new NoteAppDbContext(optionsBuilder.Options);
        }
    }
}
