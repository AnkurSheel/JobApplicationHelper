using System;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace JAH.Helper
{
    public class CustomJwtDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const string Algorithm = SecurityAlgorithms.HmacSha256;

        private readonly TokenValidationParameters _validationParameters;

        private readonly IDataSerializer<AuthenticationTicket> _ticketSerializer;

        private readonly IDataProtector _dataProtector;

        public CustomJwtDataFormat(TokenValidationParameters validationParameters,
                                   IDataSerializer<AuthenticationTicket> ticketSerializer,
                                   IDataProtector dataProtector)
        {
            _validationParameters = validationParameters ?? throw new ArgumentNullException($"{nameof(validationParameters)} cannot be null");
            _ticketSerializer = ticketSerializer ?? throw new ArgumentNullException($"{nameof(ticketSerializer)} cannot be null");
            _dataProtector = dataProtector ?? throw new ArgumentNullException($"{nameof(dataProtector)} cannot be null");
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            var authTicket = _ticketSerializer.Deserialize(_dataProtector.Unprotect(Base64UrlTextEncoder.Decode(protectedText)));

            var embeddedJwt = authTicket.Properties?.GetTokenValue("Token");

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(embeddedJwt, _validationParameters, out var token);

                var jwt = token as JwtSecurityToken;
                if (jwt == null)
                {
                    throw new SecurityTokenValidationException("JWT token was found to be invalid");
                }

                if (!jwt.Header.Alg.Equals(Algorithm, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Algorithm must be '{Algorithm}'");
                }
            }
            catch (Exception)
            {
                return null;
            }

            return authTicket;
        }

        public string Protect(AuthenticationTicket data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationTicket data, string purpose)
        {
            byte[] array = _ticketSerializer.Serialize(data);

            return Base64UrlTextEncoder.Encode(_dataProtector.Protect(array));
        }
    }
}
