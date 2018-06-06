using System;

using Microsoft.IdentityModel.Tokens;

namespace JAH.Helper
{
    public class TokenOptions
    {
        public TokenOptions(string issuer, string audience, SecurityKey signingKey, int tokenExpiryInMinutes)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentNullException($"{nameof(Audience)} is mandatory in order to generate a JWT!");
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException($"{nameof(Issuer)} is mandatory in order to generate a JWT!");
            }

            Audience = audience;
            Issuer = issuer;
            SigningKey = signingKey ?? throw new ArgumentNullException($"{nameof(SigningKey)} is mandatory in order to generate a JWT!");
            TokenExpiryInMinutes = tokenExpiryInMinutes;
        }

        public string Issuer { get; }

        public string Audience { get; }

        public SecurityKey SigningKey { get; }

        public DateTime Expiration
        {
            get
            {
                var expiration = TimeSpan.FromMinutes(TokenExpiryInMinutes);
                return DateTime.UtcNow.Add(expiration);
            }
        }

        public int TokenExpiryInMinutes { get; }
    }
}
