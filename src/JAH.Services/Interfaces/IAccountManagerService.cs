using System;
using System.Threading.Tasks;

using JAH.DomainModels;
using JAH.Helper;

using Microsoft.AspNetCore.Identity;

namespace JAH.Services.Interfaces
{
    public interface IAccountManagerService : IDisposable
    {
        Task<IdentityResult> RegisterUser(RegisterModel model);

        Task<TokenWithClaimsPrincipal> GetTokenWithClaimsPrincipal(LoginModel model);

        Task<string> GetJwtToken(LoginModel model);
    }
}
