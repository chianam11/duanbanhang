# E-Commerce Web Application - ShopMVC

Một ứng dụng web thương mại điện tử đầy đủ chức năng được xây dựng bằng ASP.NET Core MVC, phù hợp cho đồ án tốt nghiệp.

## 📋 Mục lục
- [Tính năng](#tính-năng)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt và chạy](#cài-đặt-và-chạy)
- [Cấu hình](#cấu-hình)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Deployment](#deployment)
- [Đóng góp](#đóng-góp)
- [Giấy phép](#giấy-phép)

## ✨ Tính năng

### 🛒 Quản lý sản phẩm
- Hiển thị danh sách sản phẩm với phân trang
- Chi tiết sản phẩm với hình ảnh, đánh giá
- Tìm kiếm và lọc sản phẩm theo danh mục, thương hiệu
- Quản lý kho hàng và tồn kho
- **Mới**: API RESTful cho quản lý sản phẩm

### 👤 Hệ thống người dùng
- Đăng ký/Đăng nhập với ASP.NET Identity
- Phân quyền Admin/Staff/User
- Quản lý hồ sơ cá nhân
- Lịch sử đơn hàng

### 🛍️ Giỏ hàng & Đặt hàng
- Thêm/xóa/sửa sản phẩm trong giỏ hàng
- Tính toán tổng tiền tự động
- Đặt hàng với thông tin giao hàng
- Theo dõi trạng thái đơn hàng

### 💬 Chat thời gian thực
- Chat giữa khách hàng và nhân viên hỗ trợ
- Sử dụng SignalR để giao tiếp real-time

### 🎫 Voucher & Khuyến mãi
- Hệ thống voucher giảm giá
- Flash sale với thời gian giới hạn
- Áp dụng voucher theo sản phẩm/danh mục/thương hiệu

### 📊 Quản trị viên
- Dashboard thống kê doanh thu, đơn hàng
- Quản lý sản phẩm, danh mục, thương hiệu
- Quản lý đơn hàng và trạng thái
- Quản lý người dùng và phân quyền

### 🔧 Tính năng nâng cao (Đồ án tốt nghiệp)
- **REST API**: API endpoints đầy đủ cho mobile/web integration
- **Unit Tests**: Bộ test tự động với xUnit (test suite ready)
- **Docker Support**: Containerization với Docker & Docker Compose
- **Caching**: Memory cache cho performance tối ưu
- **Structured Logging**: Logging với Serilog (file + console)
- **Global Exception Handling**: Middleware xử lý lỗi tập trung
- **API Response Wrapper**: Standardized response format
- **Custom Validation**: Attributes for phone, email, price validation
- **Extension Methods**: Helper utilities (JSON, DateTime, String, Collections)
- **Application Constants**: Centralized configuration (AppConstants, ApiRoutes)
- **CORS Configuration**: Cross-origin resource sharing setup ready
- **Security Headers**: XSS, Clickjacking, MIME-type protection
- **Error Logging**: Auto-logging của toàn bộ exceptions
- **Code Quality**: Nullability checks, proper error handling

## 🛠️ Công nghệ sử dụng

### Backend
- **ASP.NET Core 9.0** - Framework web chính
- **Entity Framework Core** - ORM cho database
- **SQL Server** - Cơ sở dữ liệu chính  
- **ASP.NET Identity** - Quản lý authentication & authorization
- **SignalR** - Real-time communication
- **Serilog** - Structured logging (structured logs)
- **Memory Cache** - In-memory caching cho performance

### Frontend
- **Razor Pages/Views** - Template engine
- **Bootstrap 5** - CSS framework responsive
- **jQuery** - JavaScript library
- **Font Awesome** - Icon library
- **Chart.js** - Data visualization

### Tools & Libraries
- **AutoMapper** - Object mapping
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI (sẵn sàng)
- **xUnit** - Unit testing framework
- **Moq** - Mocking for tests
- **Docker** - Containerization
- **FluentValidation** - Input validation

## 💻 Yêu cầu hệ thống

- **OS**: Windows 10/11, Linux, macOS
- **.NET SDK**: 9.0 trở lên
- **Database**: SQL Server 2019+ hoặc SQL Server LocalDB
- **Memory**: 4GB RAM tối thiểu
- **Storage**: 2GB dung lượng trống

## 🚀 Cài đặt và chạy

### 1. Clone repository
```bash
git clone https://github.com/your-username/ShopMVC.git
cd ShopMVC
```

### 2. Cài đặt .NET SDK
Tải và cài đặt .NET 9.0 SDK từ [microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)

### 3. Cài đặt SQL Server
#### Option A: SQL Server LocalDB (Khuyến nghị cho development)
LocalDB được cài đặt tự động với Visual Studio hoặc SQL Server Express.

#### Option B: SQL Server Express
Tải từ [microsoft.com](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### 4. Cấu hình Database
Mở `appsettings.json` và cập nhật connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ShopMVC;Trusted_Connection=True;"
  }
}
```

### 5. Chạy ứng dụng
```bash
# Restore packages
dotnet restore

# Run migrations (tạo database)
dotnet ef database update

# Chạy ứng dụng
dotnet run
```

Ứng dụng sẽ chạy tại: http://localhost:5018

### 6. Tài khoản mặc định
- **Admin**: admin@shopmvc.com / Admin123!
- **Staff**: staff@shopmvc.com / Staff123!

## ⚙️ Cấu hình

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ShopMVC;Trusted_Connection=True;"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "EnableSsl": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Email Configuration
Để gửi email (đặt hàng, quên mật khẩu), cấu hình SMTP trong `appsettings.json`.

## 📁 Cấu trúc dự án

```
ShopMVC/
├── Areas/
│   ├── Admin/          # Admin panel (controllers, services, views)
│   └── Identity/       # Authentication pages
├── Configuration/      # Constants, routes, settings
├── Controllers/        # MVC Controllers (public-facing)
├── Controllers/Api/    # REST API Controllers
├── Data/               # Database context & seeding
├── Extensions/         # Helper extension methods
├── Helpers/            # Utility classes
├── Hubs/               # SignalR hubs (real-time chat)
├── Middlewares/        # Custom middlewares (exception handling, security)
├── Models/             # Entity models
│   ├── Dto/            # Data transfer objects
│   └── ViewModels/     # View-specific models
├── Services/           # Business logic services
├── Validations/        # Custom validation attributes
├── ViewComponents/     # Razor view components
├── Views/              # Razor views (HTML templates)
├── wwwroot/            # Static files (CSS, JS, images)
├── appsettings.json    # Configuration
├── Program.cs          # Application entry point
├── Dockerfile          # Docker configuration
├── docker-compose.yml  # Docker Compose setup
└── ShopMVC.csproj      # Project file

ShopMVC.Tests/
├── Controllers/        # API controller tests
└── Services/           # Service layer tests
```

### Key Folders

- **Configuration/**: Centralized constants (`AppConstants.cs`), API routes, app settings
- **Middlewares/**: Custom middlewares like global exception handling
- **Extensions/**: Reusable extension methods for common operations
- **Validations/**: Custom validation attributes (phone, email, price validation)

## 🔌 API Documentation

### REST API Endpoints

#### Products API
```
GET    /api/products       # Lấy danh sách sản phẩm
GET    /api/products/{id}  # Chi tiết sản phẩm
POST   /api/products       # Tạo sản phẩm mới (Admin)
PUT    /api/products/{id}  # Cập nhật sản phẩm (Admin)
DELETE /api/products/{id}  # Xóa sản phẩm (Admin)
```

#### Orders API
```
GET    /api/orders         # Lấy đơn hàng của user
POST   /api/orders         # Tạo đơn hàng mới
PUT    /api/orders/{id}    # Cập nhật trạng thái (Admin)
```

#### Categories API
```
GET    /api/categories     # Lấy danh sách danh mục
POST   /api/categories     # Tạo danh mục mới (Admin)
```

### Authentication
Sử dụng JWT tokens cho API authentication:
```
POST /api/auth/login
POST /api/auth/register
```

## 🧪 Testing

### Unit Tests
```bash
# Chạy tất cả tests
dotnet test

# Chạy với coverage report
dotnet test --collect:"XPlat Code Coverage"

# Chạy tests cho project cụ thể
dotnet test ShopMVC.Tests/ShopMVC.Tests.csproj
```

### Test Coverage
- Products API endpoints
- Business logic services
- Data validation
- Authentication & authorization

### Integration Tests
```bash
# Chạy integration tests
dotnet test --filter Category=Integration
```

## 🐳 Docker Deployment

### Build Docker Image
```bash
# Build image
docker build -t shopmvc .

# Run container với LocalDB (development)
docker run -p 8080:80 shopmvc
```

### Docker Compose (Production với SQL Server)
```bash
# Khởi động toàn bộ stack
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng services
docker-compose down
```

Ứng dụng sẽ chạy tại: http://localhost:8080

### Environment Variables
```bash
# Database connection
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=ShopMVC;User=sa;Password=YourStrong!Passw0rd;

# Email settings
SmtpSettings__Host=smtp.gmail.com
SmtpSettings__Username=your-email@gmail.com
SmtpSettings__Password=your-app-password
```

## 📈 Performance Optimization

- **Caching**: Sử dụng Memory Cache cho dữ liệu tĩnh
- **Database Indexing**: Đã tối ưu indexes cho các truy vấn thường xuyên
- **Image Optimization**: Lazy loading và compression
- **CDN**: Sử dụng CDN cho static assets (khuyến nghị production)

## 🔒 Security Features

- **HTTPS Redirection**: Tự động chuyển hướng HTTP sang HTTPS
- **CSRF Protection**: Bảo vệ chống Cross-Site Request Forgery
- **XSS Prevention**: Encoding output và input validation
- **SQL Injection Prevention**: Sử dụng EF Core parameterized queries
- **Authentication & Authorization**: ASP.NET Identity với JWT
- **Rate Limiting**: Giới hạn request để chống DDoS

## 🤝 Đóng góp

1. Fork project
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

### Coding Standards
- Sử dụng PascalCase cho class names
- camelCase cho method parameters
- Thêm XML documentation cho public methods
- Viết unit tests cho business logic

## � Code Examples

### Using Global Exception Handling
```csharp
// Exceptions are automatically caught and formatted
throw new ArgumentNullException(nameof(id));  // Returns 400 Bad Request
throw new KeyNotFoundException();              // Returns 404 Not Found
throw new UnauthorizedAccessException();      // Returns 401 Unauthorized
```

### Using API Response Wrapper
```csharp
// Send standardized response
return Ok(ApiResponse<Product>.Ok(product, "Product retrieved successfully"));

// Error response
return BadRequest(ApiResponse<Product>.BadRequest("Invalid product ID"));

// With pagination
var response = new ApiResponse<PaginatedResponse<Product>>
{
    Success = true,
    Data = new PaginatedResponse<Product> {
        Items = products,
        Page = page,
        PageSize = pageSize,
        TotalItems = total
    }
};
```

### Using Extension Methods
```csharp
// String extensions
string slug = "Product Name".ToSlug();  // "product-name"
string truncated = "Long text...".Truncate(10);  // "Long te..."
bool isPhone = "0900123456".IsPhoneNumber();  // true

// DateTime extensions
int age = birthDate.GetAge();
string vietnamTime = DateTime.Now.ToVietnamTime();

// Collection extensions
var items = list.Shuffle();  // Random order
var chunks = list.Chunk(10);  // Split into chunks of 10
```

### Using Validation Attributes
```csharp
public class OrderRequest
{
    [Required]
    [StringLength(250)]
    public string ProductName { get; set; }

    [Required]
    [ValidEmail]
    public string CustomerEmail { get; set; }

    [Required]
    [VietnamesePhone]
    public string PhoneNumber { get; set; }

    [Required]
    [ValidPrice(MinPrice = 0, MaxPrice = 999999999)]
    public decimal Price { get; set; }
}
```

### Using Dependency Injection
```csharp
public class ProductController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IMemoryCache _cache;

    public ProductController(ICategoryService categoryService, IMemoryCache cache)
    {
        _categoryService = categoryService;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        return View(categories);
    }
}
```

## 👥 Tác giả

- **Tên của bạn** - *Đồ án tốt nghiệp* - [GitHub](https://github.com/your-username)

## 🙏 Lời cảm ơn

- ASP.NET Core documentation
- Entity Framework Core community
- Bootstrap & jQuery contributors
- Microsoft Learn resources

## 🐛 Troubleshooting

### Database connection failed
**Problem**: "Cannot connect to SQL Server instance"
**Solution**: 
- Check connection string in `appsettings.json`
- Ensure SQL Server LocalDB is running: `sqllocaldb info`
- Start LocalDB: `sqllocaldb start MSSQLLocalDB`

### Port already in use
**Problem**: "Address already in use - port 5018"
**Solution**:
- Kill the process on port 5018: `netstat -ano | findstr :5018`
- Change port in `launchSettings.json` under `Properties/`

### Cannot find tables in database
**Problem**: "Database tables not found"
**Solution**:
- Run migrations: `dotnet ef database update`
- Seed data: Delete `*.db` file in project root and restart
- Check `DbSeeder.cs` for seed logic

### Tests fail during build
**Problem**: "Test project build failed"
**Solution**:
- Clean build: `dotnet clean && dotnet build`
- Restore packages: `dotnet restore`
- Check that test project reference includes main project

### Swagger/API Documentation not showing
**Problem**: "/swagger" endpoint not found
**Solution**:
- Swagger is currently disabled in `Program.cs` due to dependency issues
- To enable: Uncomment Swagger configuration section in Program.cs
- Alternatively, API endpoints can be tested via Postman

---

**Lưu ý**: Đây là dự án mẫu cho mục đích học tập. Không sử dụng trực tiếp trong production mà chưa được audit security.