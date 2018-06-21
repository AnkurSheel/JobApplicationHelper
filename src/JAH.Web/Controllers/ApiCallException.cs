using System;
using System.Net;

using JAH.Logger;

namespace JAH.Web.Controllers
{
    public sealed class ApiCallException : Exception
    {
        public ApiCallException(Uri requestUri, HttpStatusCode statusCode, CustomErrorResponse error)
            : base("There was an error on the API server")
        {
            Data.Add("Path", requestUri);
            Data.Add("Status Code", statusCode);
            Data.Add("ApiErrorId", error.ErrorId);
            Data.Add("ApiErrorMessage", error.Message);
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
