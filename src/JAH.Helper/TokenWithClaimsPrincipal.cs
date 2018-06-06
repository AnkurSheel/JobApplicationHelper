using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace JAH.Helper
{
    public class TokenWithClaimsPrincipal
    {
        public AuthenticationProperties AuthenticationProperties { get; set; }

        public ClaimsPrincipal ClaimsPrincipal { get; set; }

        public string JwtResponse { get; set; }
    }
}