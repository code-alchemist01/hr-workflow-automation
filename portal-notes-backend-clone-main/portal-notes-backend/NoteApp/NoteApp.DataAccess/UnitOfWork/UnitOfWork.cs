using System.Threading.Tasks;
using NoteApp.DataAccess.Contexts;

namespace NoteApp.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NoteAppDbContext _context;

        public UnitOfWork(NoteAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 