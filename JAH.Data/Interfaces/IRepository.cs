using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobApplicationHelper.Data.Interfaces
{
    public interface IRepository<T>
        where T : new()
    {
        Task<IQueryable<T>> FindAll();
    }
}
