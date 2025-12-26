using System.Threading.Tasks;

namespace NoteApp.DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
} 