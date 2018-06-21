using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace JAH.Logger.Middleware
{
    public static class CustomExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder,
                                                                    string product,
                                                                    string layer,
                                                                    string errorHandlingPath)
        {
            return builder.UseMiddleware<CustomWebExceptionHandlerMiddleware>(product,
                                                                              layer,
                                                                              Options.Create(new ExceptionHandlerOptions
                                                                              {
                                                                                  ExceptionHandlingPath = new PathString(errorHandlingPath)
                                                                              }));
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder, string product, string layer)
        {
            return builder.UseMiddleware<CustomApiExceptionHandlerMiddleware>(product, layer, Options.Create(new ExceptionHandlerOptions()));
        }
    }
}
