using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.DomainModels;
using JobApplicationHelper.Services.Interfaces;

namespace JobApplicationHelper.Services.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        public Task<IQueryable<JobApplication>> ReadAllAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}