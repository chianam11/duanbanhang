namespace ShopMVC.Helpers
{
    public static class FileUploadValidation
    {
        private static readonly string[] DefaultImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif"];

        public static bool IsValidImage(IFormFile? file, out string errorMessage, bool required = false, int maxSizeInMb = 5)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                if (required)
                {
                    errorMessage = "Vui lòng chọn tệp ảnh.";
                    return false;
                }

                return true;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!DefaultImageExtensions.Contains(extension))
            {
                errorMessage = "Chỉ hỗ trợ các định dạng ảnh JPG, JPEG, PNG, WEBP, GIF, AVIF.";
                return false;
            }

            var maxBytes = maxSizeInMb * 1024 * 1024L;
            if (file.Length > maxBytes)
            {
                errorMessage = $"Kích thước ảnh không được vượt quá {maxSizeInMb}MB.";
                return false;
            }

            return true;
        }
    }
}
