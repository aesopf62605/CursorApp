using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using InvoiceApi.Services;

namespace InvoiceApi.Middleware
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMetricsService _metricsService;

        public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService)
        {
            _next = next;
            _metricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                stopwatch.Stop();
                context.Response.Body = originalBodyStream;

                var endpoint = context.Request.Path.Value ?? "unknown";
                var statusCode = context.Response.StatusCode;

                _metricsService.RecordApiRequest(endpoint, statusCode, stopwatch.Elapsed);
            }
        }
    }

    public static class MetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricsMiddleware>();
        }
    }
} 