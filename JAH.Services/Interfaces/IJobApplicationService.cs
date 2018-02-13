using System.Collections.Generic;
using System.Threading.Tasks;
using JAH.DomainModels;

namespace JAH.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<JobApplication> AddNewApplication(JobApplication jobApplication);

        Task<IEnumerable<JobApplication>> GetAllApplications();

        JobApplication GetApplication(string companyName);

        Task<JobApplication> UpdateApplication(string companyName, JobApplication newApplication);
    }
}
