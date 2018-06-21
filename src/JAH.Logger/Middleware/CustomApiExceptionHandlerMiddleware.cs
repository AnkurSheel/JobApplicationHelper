using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace JAH.Logger.Middleware
{
    public sealed class CustomApiExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IJahLogger _logger;

        private readonly ExceptionHandlerOptions _options;

        private readonly string _product;

        private readonly string _layer;

        public CustomApiExceptionHandlerMiddleware(string product,
                                                   string layer,
                                                   RequestDelegate next,
                                                   IJahLogger logger,
                                                   IOptions<ExceptionHandlerOptions> options)
        {
            _product = product;
            _layer = layer;

            _next = next;
            _logger = logger;
            _options = options.Value;
            if (_options.ExceptionHandler == null)
            {
                _options.ExceptionHandler = _next;
            }
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorContext = context.Features.Get<IExceptionHandlerFeature>();
                if (errorContext != null)
                {
                    ex = errorContext.Error;
                }

                _logger.LogWebError(_product, _layer, ex, context);
                var errorId = Activity.Current?.Id ?? context.TraceIdentifier;
                var jsonResponse = JsonConvert.SerializeObject(new CustomErrorResponse { ErrorId = errorId, Message = "Error in the API layer" });
                await context.Response.WriteAsync(jsonResponse, Encoding.UTF8).ConfigureAwait(false);
            }
        }
    }
}
