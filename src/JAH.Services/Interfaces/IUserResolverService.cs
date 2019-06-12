using System.Threading.Tasks;

using JAH.Data.Entities;

namespace JAH.Services.Interfaces
{
    public interface IUserResolverService
    {
        Task<JobApplicationUser> GetCurrentUser();
    }
}
