using System;
using System.Net;

namespace JAH.Web.Controllers
{
    public sealed class ApiCallException : Exception
    {
        public ApiCallException(Uri requestUri, HttpStatusCode statusCode)
            : base("There was an error on the API server")
        {
            Data.Add("Path", requestUri);
            Data.Add("Status Code", statusCode);
        }

        private ApiCallException()
        {
        }

        private ApiCallException(string message)
            : base(message)
        {
        }

        private ApiCallException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
