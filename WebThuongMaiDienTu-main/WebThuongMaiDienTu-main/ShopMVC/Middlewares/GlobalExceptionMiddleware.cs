namespace ShopMVC.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "Lỗi hệ thống. Vui lòng thử lại sau.",
                timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ArgumentNullException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { success = false, message = "Dữ liệu không hợp lệ.", timestamp = DateTime.UtcNow };
                    break;

                case ArgumentException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { success = false, message = exception.Message, timestamp = DateTime.UtcNow };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response = new { success = false, message = "Không tìm thấy dữ liệu.", timestamp = DateTime.UtcNow };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new { success = false, message = "Không có quyền truy cập.", timestamp = DateTime.UtcNow };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new { success = false, message = "Lỗi hệ thống: " + exception.Message, timestamp = DateTime.UtcNow };
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}