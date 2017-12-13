using System.Collections.Generic;
using System.Threading.Tasks;
using JAH.DomainModels;

namespace JAH.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task Add(JobApplication jobApplication);

        Task<IEnumerable<JobApplication>> ReadAllAsync();
    }
}
