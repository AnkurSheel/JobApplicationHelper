using System.Collections.Generic;
using System.Threading.Tasks;
using JAH.DomainModels;

namespace JAH.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task AddNewApplication(JobApplication jobApplication);

        Task<IEnumerable<JobApplication>> GetAllApplications();
    }
}
