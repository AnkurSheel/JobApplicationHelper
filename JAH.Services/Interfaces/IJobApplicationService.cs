using JAH.DomainModels;
using System.Linq;
using System.Threading.Tasks;

namespace JAH.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<IQueryable<JobApplication>> ReadAllAsync();
    }
}