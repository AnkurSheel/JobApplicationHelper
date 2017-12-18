using System.Linq;
using System.Threading.Tasks;

namespace JAH.Data.Interfaces
{
    public interface IRepository<T>
        where T : new()
    {
        Task Create(T entity);

        IQueryable<T> FindAll();

        T Find(string id);
    }
}
