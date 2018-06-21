using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace JAH.Logger.Middleware
{
    public sealed class CustomWebExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IJahLogger _logger;

        private readonly ExceptionHandlerOptions _options;

        private readonly string _product;

        private readonly string _layer;

        public CustomWebExceptionHandlerMiddleware(string product,
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
                _logger.LogWebError(_product, _layer, ex, context);

                var originalPath = context.Request.Path;
                if (_options.ExceptionHandlingPath.HasValue)
                {
                    context.Request.Path = _options.ExceptionHandlingPath;
                }

                context.Response.Clear();
                var exceptionHandlerFeature = new ExceptionHandlerFeature { Error = ex, Path = originalPath.Value };

                context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
                context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
                context.Response.StatusCode = 500;
                context.Response.OnStarting(ClearCacheHeaders, context.Response);

                await _options.ExceptionHandler(context).ConfigureAwait(false);
            }
        }

        private static Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }
    }
}
