using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.DomainModels;

namespace JobApplicationHelper.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<IQueryable<JobApplication>> ReadAllAsync();
    }
}