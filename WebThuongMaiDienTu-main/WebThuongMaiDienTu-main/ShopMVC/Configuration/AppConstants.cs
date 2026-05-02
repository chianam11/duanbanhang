namespace ShopMVC.Configuration
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class AppConstants
    {
        // Pagination
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int MAX_PAGE_SIZE = 100;
        public const int MIN_PAGE_SIZE = 1;

        // Cache durations (in minutes)
        public const int CACHE_DURATION_PRODUCTS = 30;
        public const int CACHE_DURATION_CATEGORIES = 60;
        public const int CACHE_DURATION_SHORT = 5;
        public const int CACHE_DURATION_LONG = 120;

        // File uploads
        public const long MAX_FILE_SIZE = 5 * 1024 * 1024;
        public const string ALLOWED_IMAGE_EXTENSIONS = ".jpg,.jpeg,.png,.gif,.webp";
        public const string UPLOAD_DIRECTORY = "uploads";

        // Price validation
        public const decimal MIN_PRICE = 0;
        public const decimal MAX_PRICE = 999999999.99m;

        // Product
        public const int MIN_PRODUCT_NAME_LENGTH = 3;
        public const int MAX_PRODUCT_NAME_LENGTH = 250;
        public const int MIN_DESCRIPTION_LENGTH = 10;

        // Order
        public const int ORDER_PENDING_HOURS = 24;
        public const decimal SHIPPING_FEE_DEFAULT = 30000;

        // Roles
        public const string ROLE_ADMIN = "QuanTri";
        public const string ROLE_STAFF = "NhanVien";
        public const string ROLE_CHAT_SUPPORT = "HoTroChat";
        public const string ROLE_USER = "Khach";
        public const string ROLES_ADMIN_OR_STAFF = ROLE_ADMIN + "," + ROLE_STAFF;
        public const string ROLES_SUPPORT_CONSOLE = ROLE_ADMIN + "," + ROLE_CHAT_SUPPORT;

        // Email
        public const int EMAIL_VERIFICATION_TIMEOUT_MINUTES = 15;
        public const int PASSWORD_RESET_TIMEOUT_MINUTES = 30;

        // Security
        public const int FAILED_LOGIN_ATTEMPTS = 5;
        public const int LOCKOUT_DURATION_MINUTES = 15;
    }

    /// <summary>
    /// API response messages
    /// </summary>
    public static class ApiMessages
    {
        public const string SUCCESS = "Thanh cong";
        public const string CREATED = "Tao thanh cong";
        public const string UPDATED = "Cap nhat thanh cong";
        public const string DELETED = "Xoa thanh cong";

        public const string ERROR_NOT_FOUND = "Khong tim thay";
        public const string ERROR_INVALID_REQUEST = "Yeu cau khong hop le";
        public const string ERROR_UNAUTHORIZED = "Khong duoc phep truy cap";
        public const string ERROR_FORBIDDEN = "Cam truy cap";
        public const string ERROR_CONFLICT = "Du lieu bi xung dot";
        public const string ERROR_INTERNAL = "Loi he thong";
        public const string ERROR_VALIDATION = "Validation failed";

        public const string ERROR_INSUFFICIENT_STOCK = "Ton kho khong du";
        public const string ERROR_INVALID_VOUCHER = "Voucher khong hop le";
        public const string ERROR_EXPIRED_VOUCHER = "Voucher da het han";
        public const string ERROR_DUPLICATE_EMAIL = "Email da ton tai";
        public const string ERROR_INVALID_CREDENTIALS = "Ten dang nhap hoac mat khau sai";
    }

    /// <summary>
    /// API route definitions
    /// </summary>
    public static class ApiRoutes
    {
        private const string BASE = "api";

        public static class Products
        {
            private const string CONTROLLER = BASE + "/products";
            public const string GET_ALL = CONTROLLER;
            public const string GET_BY_ID = CONTROLLER + "/{id}";
            public const string CREATE = CONTROLLER;
            public const string UPDATE = CONTROLLER + "/{id}";
            public const string DELETE = CONTROLLER + "/{id}";
        }

        public static class Orders
        {
            private const string CONTROLLER = BASE + "/orders";
            public const string GET_ALL = CONTROLLER;
            public const string GET_BY_ID = CONTROLLER + "/{id}";
            public const string CREATE = CONTROLLER;
            public const string UPDATE_STATUS = CONTROLLER + "/{id}/status";
        }

        public static class Categories
        {
            private const string CONTROLLER = BASE + "/categories";
            public const string GET_ALL = CONTROLLER;
            public const string GET_BY_ID = CONTROLLER + "/{id}";
            public const string CREATE = CONTROLLER;
            public const string UPDATE = CONTROLLER + "/{id}";
            public const string DELETE = CONTROLLER + "/{id}";
        }

        public static class Auth
        {
            private const string CONTROLLER = BASE + "/auth";
            public const string LOGIN = CONTROLLER + "/login";
            public const string REGISTER = CONTROLLER + "/register";
            public const string REFRESH = CONTROLLER + "/refresh";
            public const string LOGOUT = CONTROLLER + "/logout";
        }
    }
}
