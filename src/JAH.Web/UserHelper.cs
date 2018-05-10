using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JAH.Web
{
    public class UserHelper
    {
        private readonly HttpClient _client;

        public UserHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<bool> IsSignedIn()
        {
            HttpResponseMessage responseMessage = await _client.GetAsync("api/auth/signedIn");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                return Convert.ToBoolean(responseData);
            }

            return false;
        }
    }
}
