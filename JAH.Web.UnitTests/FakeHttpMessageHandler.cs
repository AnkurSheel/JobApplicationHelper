using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JAH.Web.UnitTests
{
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Now we can setup this method with our mocking framework");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request));
        }
    }
}