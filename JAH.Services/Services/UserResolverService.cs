using System.Threading.Tasks;

using JAH.Data.Entities;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace JAH.Services.Services
{
    public class UserResolverService : IUserResolverService
    {
        private readonly UserManager<JobApplicationUser> _userManager;
        private readonly IHttpContextAccessor _context;

        public UserResolverService(UserManager<JobApplicationUser> userManager, IHttpContextAccessor context)
        {
            _userManager = userManager;
            _context = context;
        }

        public Task<JobApplicationUser> GetCurrentUser()
        {
            return _userManager.GetUserAsync(_context.HttpContext.User);
        }
    }
}
