using Microsoft.EntityFrameworkCore;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NoteApp.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly NoteAppDbContext _context;
        protected readonly DbSet<T> _dbSet;


        public Repository(NoteAppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
        public async Task<Note?> GetNoteWithDetailsAsync(int noteId)
        {
            return await _context.Notes
                .Include(n => n.Folder)
                .Include(n => n.NoteTags)
                .Include(n => n.NoteImages)
                .Include(n => n.NoteAudios)
                .FirstOrDefaultAsync(n => n.Id == noteId);
        }

    }
} 