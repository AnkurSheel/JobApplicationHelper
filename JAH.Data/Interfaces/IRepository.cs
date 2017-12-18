using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JAH.Data.Interfaces
{
    public interface IRepository<TEntity>
        where TEntity : new()
    {
        Task Create(TEntity entity);

        IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null);

        TEntity GetOne(Expression<Func<TEntity, bool>> filter = null);
    }
}
