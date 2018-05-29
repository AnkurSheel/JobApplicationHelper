using System.Collections.Generic;
using System.Security.Claims;

namespace JAH.Helper
{
    public interface ITokenGenerator
    {
        TokenWithClaimsPrincipal GenerateAccessTokenWithClaimsPrincipal(string userName, IEnumerable<Claim> claims);

        string GetJwtToken(string accessToken, IEnumerable<Claim> claims);
    }
}
