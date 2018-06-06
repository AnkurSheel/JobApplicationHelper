using Microsoft.IdentityModel.Tokens;

namespace JAH.Helper
{
    using System;

    public static class TokenValidationParametersExtensions
    {
        public static TokenOptions ToTokenOptions(this TokenValidationParameters tokenValidationParameters,
                                                  int tokenExpiryInMinutes)
        {
            return new TokenOptions(tokenValidationParameters.ValidIssuer,
                                    tokenValidationParameters.ValidAudience,
                                    tokenValidationParameters.IssuerSigningKey,
                                    tokenExpiryInMinutes);
        }
    }
}
