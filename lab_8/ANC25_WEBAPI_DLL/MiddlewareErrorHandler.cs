using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ANC25_WEBAPI_DLL
{
    public class MiddlewareErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly string _appName;
        private readonly ILogger<MiddlewareErrorHandler> _logger;

        public MiddlewareErrorHandler(RequestDelegate next, string appName, ILogger<MiddlewareErrorHandler> logger)
        {
            _next = next;
            _appName = appName;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled exception occurred in application '{_appName}'.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain";

                await context.Response.WriteAsync($"An unexpected error occurred in {_appName}. Details: {ex.Message}");
            }
        }
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCelebritiesErrorHandler(this IApplicationBuilder app, string appName)
        {
            return app.UseMiddleware<MiddlewareErrorHandler>(appName);
        }
    }
}