using System.Collections.Concurrent;

namespace ShopMVC.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, RateLimit> _requestCounts = new();
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

            if (!_requestCounts.TryGetValue(key, out var rateLimit))
            {
                rateLimit = new RateLimit { FirstRequest = DateTime.UtcNow };
                _requestCounts.TryAdd(key, rateLimit);
            }

            // Clean up old entries
            if (DateTime.UtcNow - rateLimit.FirstRequest > TimeSpan.FromHours(1))
            {
                _requestCounts.TryRemove(key, out _);
                rateLimit = new RateLimit { FirstRequest = DateTime.UtcNow };
                _requestCounts.TryAdd(key, rateLimit);
            }

            // Check per-minute limit
            if (DateTime.UtcNow - rateLimit.FirstRequest < TimeSpan.FromMinutes(1))
            {
                rateLimit.RequestCount++;
                if (rateLimit.RequestCount > RequestsPerMinute)
                {
                    _logger.LogWarning($"Rate limit exceeded for {clientIp} on {endpoint}");
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", "60");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Quá nhiều yêu cầu. Vui lòng thử lại sau 1 phút.",
                        code = "RATE_LIMIT_EXCEEDED"
                    });
                    return;
                }
            }

            // Check per-hour limit
            if (DateTime.UtcNow - rateLimit.FirstRequest < TimeSpan.FromHours(1))
            {
                if (rateLimit.HourlyCount > RequestsPerHour)
                {
                    _logger.LogWarning($"Hourly rate limit exceeded for {clientIp}");
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", "3600");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Bạn đã vượt quá giới hạn yêu cầu hàng giờ.",
                        code = "HOURLY_LIMIT_EXCEEDED"
                    });
                    return;
                }
                rateLimit.HourlyCount++;
            }

            // Add rate limit info to response headers
            context.Response.Headers.Add("X-RateLimit-Limit", RequestsPerMinute.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", (RequestsPerMinute - rateLimit.RequestCount).ToString());
            context.Response.Headers.Add("X-RateLimit-Reset", rateLimit.FirstRequest.AddMinutes(1).ToString("O"));

            await _next(context);
        }

        private string GetClientIp(HttpContext context)
        {
            // Try to get IP from X-Forwarded-For header (for proxies)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var ip = forwarded.ToString().Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private class RateLimit
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
