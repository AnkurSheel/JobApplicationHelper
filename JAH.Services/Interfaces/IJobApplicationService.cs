using System.Collections.Generic;
using JAH.DomainModels;
using System.Threading.Tasks;

namespace JAH.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<IEnumerable<JobApplication>> ReadAllAsync();
    }
}