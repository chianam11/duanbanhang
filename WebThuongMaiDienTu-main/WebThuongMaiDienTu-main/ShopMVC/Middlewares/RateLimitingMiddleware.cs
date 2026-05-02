using System.Collections.Concurrent;

namespace ShopMVC.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, RateLimit> RequestCounts = new();
        private const int RequestsPerMinute = 100;
        private const int RequestsPerHour = 5000;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIp(context);
            var endpoint = context.Request.Path.ToString();
            var key = $"{clientIp}:{endpoint}";

            if (!RequestCounts.TryGetValue(key, out var rateLimit))
            {
                rateLimit = new RateLimit { FirstRequest = DateTime.UtcNow };
                RequestCounts.TryAdd(key, rateLimit);
            }

            if (DateTime.UtcNow - rateLimit.FirstRequest > TimeSpan.FromHours(1))
            {
                RequestCounts.TryRemove(key, out _);
                rateLimit = new RateLimit { FirstRequest = DateTime.UtcNow };
                RequestCounts.TryAdd(key, rateLimit);
            }

            if (DateTime.UtcNow - rateLimit.FirstRequest < TimeSpan.FromMinutes(1))
            {
                rateLimit.RequestCount++;
                if (rateLimit.RequestCount > RequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = "60";
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Qua nhieu yeu cau. Vui long thu lai sau 1 phut.",
                        code = "RATE_LIMIT_EXCEEDED"
                    });
                    return;
                }
            }

            if (DateTime.UtcNow - rateLimit.FirstRequest < TimeSpan.FromHours(1))
            {
                if (rateLimit.HourlyCount > RequestsPerHour)
                {
                    _logger.LogWarning("Hourly rate limit exceeded for {ClientIp}", clientIp);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = "3600";
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Ban da vuot qua gioi han yeu cau hang gio.",
                        code = "HOURLY_LIMIT_EXCEEDED"
                    });
                    return;
                }

                rateLimit.HourlyCount++;
            }

            context.Response.Headers["X-RateLimit-Limit"] = RequestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, RequestsPerMinute - rateLimit.RequestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = rateLimit.FirstRequest.AddMinutes(1).ToString("O");

            await _next(context);
        }

        private static string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var ip = forwarded.ToString().Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                {
                    return ip;
                }
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private sealed class RateLimit
        {
            public DateTime FirstRequest { get; set; }
            public int RequestCount { get; set; } = 1;
            public int HourlyCount { get; set; } = 1;
        }
    }

    public static class RateLimitingExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
