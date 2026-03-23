namespace ShopMVC.Models.Dto
{
    /// <summary>
    /// Standardized API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int? StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string[]>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> BadRequest(string message = "Yêu cầu không hợp lệ")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 400
            };
        }

        public static ApiResponse<T> BadRequest(Dictionary<string, string[]> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed",
                StatusCode = 400,
                Errors = errors
            };
        }

        public static ApiResponse<T> NotFound(string message = "Không tìm thấy")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Không được phép")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Cấm truy cập")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 403
            };
        }

        public static ApiResponse<T> Error(string message = "Lỗi hệ thống")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Paginated API response
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}