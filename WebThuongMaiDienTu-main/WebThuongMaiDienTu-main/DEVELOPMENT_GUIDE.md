# Development Guide - ShopMVC

Hướng dẫn phát triển & tính năng nâng cao cho nhà phát triển của dự án ShopMVC.

## 📚 Table of Contents
- [Project Architecture](#project-architecture)
- [Développement Workflow](#development-workflow)
- [Adding New Features](#adding-new-features)
- [API Development](#api-development)
- [Testing](#testing)
- [Database Migrations](#database-migrations)
- [Logging & Debugging](#logging--debugging)
- [Best Practices](#best-practices)

## 🏗️ Project Architecture

### Layered Architecture
```
Presentation Layer (Controllers, Views)
    ↓
Business Logic Layer (Services)
    ↓
Data Access Layer (Entity Framework, DbContext)
    ↓
Database Layer (SQL Server)
```

### Dependency Injection
Dự án sử dụng ASP.NET Core's built-in DI container. Registrations trong `Program.cs`:

```csharp
// Service registration
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddMemoryCache();
```

## 🔄 Development Workflow

### 1. Creating a New Controller

**MVC Controller** (cho public-facing pages):
```csharp
using ShopMVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShopMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IOrderService _orderService;

        public ProductController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _orderService.GetProductsAsync();
            return View(products);
        }
    }
}
```

**API Controller** (cho REST endpoints):
```csharp
using ShopMVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopMVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public ProductsApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Product>>>> GetProducts()
        {
            try
            {
                var products = await _orderService.GetProductsAsync();
                return Ok(ApiResponse<List<Product>>.Ok(products));
            }
            catch (Exception ex)
            {
                // Exception automatically caught by GlobalExceptionMiddleware
                throw;
            }
        }
    }
}
```

### 2. Adding a New Service

```csharp
using ShopMVC.Data;
using ShopMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ShopMVC.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetActiveProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
    }

    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "Active_Products";

        public ProductService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            // Try to get from cache first
            if (!_cache.TryGetValue(CACHE_KEY, out List<Product> products))
            {
                products = await _context.SanPhams
                    .Include(p => p.DanhMuc)
                    .Include(p => p.Anhs)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Ten)
                    .ToListAsync();

                // Set cache for 30 minutes
                _cache.Set(CACHE_KEY, products, TimeSpan.FromMinutes(30));
            }

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.Anhs)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task CreateProductAsync(Product product)
        {
            product.NgayTao = DateTime.Now;
            product.NgayCapNhat = DateTime.Now;

            _context.SanPhams.Add(product);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove(CACHE_KEY);
        }

        public async Task UpdateProductAsync(Product product)
        {
            product.NgayCapNhat = DateTime.Now;
            _context.SanPhams.Update(product);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove(CACHE_KEY);
        }
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

### 3. Adding a View Model with Validation

```csharp
using ShopMVC.Validations;
using System.ComponentModel.DataAnnotations;

namespace ShopMVC.Models.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [ValidPrice(MinPrice = 1000, MaxPrice = 999999999)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
```

## 🔌 API Development

### Creating a New API Endpoint

```csharp
[HttpPost("search")]
[Authorize(Roles = "QuanTri,Staff")]
public async Task<ActionResult<ApiResponse<PaginatedResponse<Product>>>> SearchProducts(
    [FromQuery] string? keyword,
    [FromQuery] int categoryId = 0,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (pageSize > AppConstants.MAX_PAGE_SIZE)
        pageSize = AppConstants.MAX_PAGE_SIZE;

    var query = _context.SanPhams
        .Include(p => p.DanhMuc)
        .Where(p => p.IsActive);

    if (!string.IsNullOrEmpty(keyword))
        query = query.Where(p => p.Ten.Contains(keyword));

    if (categoryId > 0)
        query = query.Where(p => p.IdDanhMuc == categoryId);

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(p => p.NgayTao)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var response = new PaginatedResponse<Product>
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalItems = total,
        TotalPages = (int)Math.Ceiling(total / (double)pageSize)
    };

    return Ok(ApiResponse<PaginatedResponse<Product>>.Ok(response));
}
```

## 🧪 Testing

### Unit Testing Example

```csharp
using Xunit;
using ShopMVC.Controllers.Api;
using ShopMVC.Data;
using ShopMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ShopMVC.Tests.Controllers
{
    public class ProductsApiControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            var context = new AppDbContext(options);

            // Seed test data
            context.DanhMucs.Add(new DanhMuc { Id = 1, Ten = "Electronics" });
            context.SanPhams.Add(new SanPham
            {
                Id = 1,
                Ten = "Laptop",
                Gia = 1000000,
                IdDanhMuc = 1,
                IsActive = true
            });
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetProducts_ReturnsAllActiveProducts()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsApiController(context);

            // Act
            var result = await controller.GetProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<List<SanPham>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Single(response.Data);
        }
    }
}
```

Run tests:
```bash
dotnet test
dotnet test --filter Category=Integration
```

## 🗄️ Database Migrations

### Creating a Migration

```bash
# Create migration
dotnet ef migrations add AddNewProductFields

# Review migration file in Migrations/ folder
# Update if needed

# Apply migration
dotnet ef database update
```

### Seeding Data

```csharp
// In DbSeeder.cs
public static async Task SeedAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<AppDbContext>();

    // Run migrations
    await context.Database.MigrateAsync();

    // Check if already seeded
    if (await context.DanhMucs.AnyAsync())
        return;

    // Add categories
    var categories = new[]
    {
        new DanhMuc { Ten = "Electronics", HienThi = true },
        new DanhMuc { Ten = "Clothing", HienThi = true }
    };

    await context.DanhMucs.AddRangeAsync(categories);
    await context.SaveChangesAsync();

    // Add products...
}
```

## 🔍 Logging & Debugging

### Using Serilog

```csharp
using Serilog;

public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(int orderId)
    {
        try
        {
            _logger.LogInformation("Processing order {OrderId}", orderId);

            // Process logic...

            _logger.LogInformation("Order {OrderId} processed successfully", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", orderId);
            throw;
        }
    }
}
```

View logs in `logs/` folder (daily rotation).

## ✅ Best Practices

### 1. **Always Use Async/Await**
```csharp
// Good
public async Task<Product> GetProductAsync(int id)
{
    return await _context.SanPhams.FindAsync(id);
}

// Avoid
public Product GetProduct(int id)
{
    return _context.SanPhams.Find(id);
}
```

### 2. **Use Dependency Injection**
```csharp
// Good
public class ProductController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
}

// Avoid
public class ProductController
{
    public void GetProducts()
    {
        var context = new AppDbContext();
        // ...
    }
}
```

### 3. **Validate Input**
```csharp
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductViewModel model)
{
    if (!ModelState.IsValid)
        return BadRequest(ApiResponse<CreateProductViewModel>
            .BadRequest("Invalid model data"));

    // Process...
}
```

### 4. **Use Constants**
```csharp
// Use AppConstants for magic numbers
const int MAX_PAGE_SIZE = AppConstants.MAX_PAGE_SIZE;
const string CACHE_KEY = "Products";

// Instead of
const int MAX_PAGE_SIZE = 100;  // Magic number!
```

### 5. **Add Error Handling**
```csharp
try
{
    await _service.ProcessAsync();
}
catch (ArgumentNullException ex)
{
    _logger.LogError(ex, "Validation error");
    throw new ApplicationException("Invalid input", ex);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    throw new ApplicationException("Internal error", ex);
}
```

### 6. **Use Meaningful Names**
```csharp
// Good
var activeProducts = products.Where(p => p.IsActive);

// Bad
var p = products.Where(x => x.IsActive);
var stuff = products.Where(x => x.IsActive);
```

### 7. **Document Public Methods**
```csharp
/// <summary>
/// Retrieves all active products with optional filtering
/// </summary>
/// <param name="categoryId">Optional category filter</param>
/// <param name="page">Page number (1-based)</param>
/// <returns>Paginated list of products</returns>
public async Task<PaginatedResponse<Product>> GetProductsAsync(
    int? categoryId = null,
    int page = 1)
{
    // Implementation...
}
```

## 🚀 Deployment Checklist

- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Logging enabled
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Security headers enabled
- [ ] CORS configured properly
- [ ] Error handling in place
- [ ] API documentation updated
- [ ] Performance tested
- [ ] Security reviewed

---

For more information, see [README.md](README.md)