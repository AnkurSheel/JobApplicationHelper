using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

namespace JAH.Helper
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly TokenOptions _jwtOptions;

        public TokenGenerator(TokenOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        public TokenWithClaimsPrincipal GenerateAccessTokenWithClaimsPrincipal(string userName, IEnumerable<Claim> claims)
        {
            List<Claim> userClaimList = claims.ToList();
            var accessToken = GenerateAccessToken(userName, userClaimList);

            var claimsIdentity = new ClaimsIdentity(MergeUserClaimsWithDefaultClaims(userName, userClaimList), "Token");

            return new TokenWithClaimsPrincipal
                   {
                       AuthenticationProperties = CreateAuthenticationProperties(accessToken),
                       ClaimsPrincipal = new ClaimsPrincipal(claimsIdentity),
                       JwtResponse = GetJwtResponse(accessToken)
                   };
        }

        public string GetJwtToken(string userName, IEnumerable<Claim> claims)
        {
            List<Claim> userClaimList = claims.ToList();
            var accessToken = GenerateAccessToken(userName, userClaimList);
            return GetJwtResponse(accessToken);
        }

        private static AuthenticationProperties CreateAuthenticationProperties(string accessToken)
        {
            var authProps = new AuthenticationProperties();
            authProps.StoreTokens(new[] { new AuthenticationToken { Name = "Token", Value = accessToken } });
            return authProps;
        }

        private static IEnumerable<Claim> MergeUserClaimsWithDefaultClaims(string userName, IEnumerable<Claim> claims)
        {
            return new List<Claim>(claims)
                   {
                       new Claim(JwtRegisteredClaimNames.Sub, userName),
                       new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                       new Claim(JwtRegisteredClaimNames.Iat,
                                 DateTime.UtcNow.TimeOfDay.Ticks.ToString(CultureInfo.CurrentCulture),
                                 ClaimValueTypes.Integer64)
                   };
        }

        private string GetJwtResponse(string accessToken)
        {
            var jwt = new { auth_token = accessToken, expires_in = _jwtOptions.Expiration };
            return JsonConvert.SerializeObject(jwt);
        }

        private string GenerateAccessToken(string userName, IEnumerable<Claim> claims)
        {
            List<Claim> claimsList = claims.ToList();
            var jwt = new JwtSecurityToken(_jwtOptions.Issuer,
                                           _jwtOptions.Audience,
                                           MergeUserClaimsWithDefaultClaims(userName, claimsList),
                                           DateTime.UtcNow,
                                           _jwtOptions.Expiration,
                                           new SigningCredentials(_jwtOptions.SigningKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
