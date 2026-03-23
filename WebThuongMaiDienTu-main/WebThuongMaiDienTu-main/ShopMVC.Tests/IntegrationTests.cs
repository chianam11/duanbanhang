using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services;
using Xunit;

namespace ShopMVC.Tests
{
    public class IntegrationTestBase : IAsyncLifetime
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected AppDbContext DbContext { get; private set; }
        protected UserManager<NguoiDung> UserManager { get; private set; }
        protected RoleManager<IdentityRole> RoleManager { get; private set; }

        public async Task InitializeAsync()
        {
            var services = new ServiceCollection();

            // Register DbContext with in-memory SQLite
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=:memory:"));

            // Add Identity
            services.AddIdentity<NguoiDung, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Add Services
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddMemoryCache();
            services.AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            using var scope = ServiceProvider.CreateScope();
            DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            UserManager = scope.ServiceProvider.GetRequiredService<UserManager<NguoiDung>>();
            RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create database
            await DbContext.Database.EnsureCreatedAsync();

            // Seed data
            await SeedDataAsync();
        }

        public async Task DisposeAsync()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            ServiceProvider?.Dispose();
        }

        protected virtual async Task SeedDataAsync()
        {
            // Create roles
            await RoleManager.CreateAsync(new IdentityRole("QuanTri"));
            await RoleManager.CreateAsync(new IdentityRole("NhanVien"));
            await RoleManager.CreateAsync(new IdentityRole("Khach"));

            // Create test user
            var user = new NguoiDung
            {
                UserName = "testuser@test.local",
                Email = "testuser@test.local",
                EmailConfirmed = true,
                HoTen = "Test User"
            };
            await UserManager.CreateAsync(user, "Test@123");
            await UserManager.AddToRoleAsync(user, "Khach");
        }
    }

    public class AnalyticsServiceTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetDashboardStatsAsync_ReturnsDashboardStatistics()
        {
            // Arrange
            var analyticsService = ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Act
            var stats = await analyticsService.GetDashboardStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.IsType<DashboardStatistics>(stats);
            Assert.True(stats.TotalOrders >= 0);
            Assert.True(stats.TotalProducts >= 0);
            Assert.True(stats.TotalCustomers >= 0);
        }

        [Fact]
        public async Task GetSalesAnalyticsAsync_ReturnsSalesAnalytics()
        {
            // Arrange
            var analyticsService = ServiceProvider.GetRequiredService<IAnalyticsService>();
            var startDate = DateTime.Today.AddDays(-30);
            var endDate = DateTime.Today;

            // Act
            var sales = await analyticsService.GetSalesAnalyticsAsync(startDate, endDate);

            // Assert
            Assert.NotNull(sales);
            Assert.IsType<SalesAnalytics>(sales);
            Assert.True(sales.TotalSales >= 0);
        }

        [Fact]
        public async Task GetProductAnalyticsAsync_ReturnsProductAnalytics()
        {
            // Arrange
            var analyticsService = ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Act
            var products = await analyticsService.GetProductAnalyticsAsync();

            // Assert
            Assert.NotNull(products);
            Assert.IsType<ProductAnalytics>(products);
            Assert.True(products.TotalProducts >= 0);
        }

        [Fact]
        public async Task GetCustomerAnalyticsAsync_ReturnsCustomerAnalytics()
        {
            // Arrange
            var analyticsService = ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Act
            var customers = await analyticsService.GetCustomerAnalyticsAsync();

            // Assert
            Assert.NotNull(customers);
            Assert.IsType<CustomerAnalytics>(customers);
            Assert.True(customers.TotalCustomers >= 0);
        }
    }

    public class CategoryServiceTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetActiveCategoriesAsync_ReturnsCachedCategories()
        {
            // Arrange
            var categoryService = ServiceProvider.GetRequiredService<ICategoryService>();
            
            // Add test category
            var category = new DanhMuc
            {
                Ten = "Test Category",
                Slug = "test-category",
                HienThi = true,
                ThuTu = 1
            };
            DbContext.DanhMucs.Add(category);
            await DbContext.SaveChangesAsync();

            // Act
            var categories = await categoryService.GetActiveCategoriesAsync();

            // Assert
            Assert.NotNull(categories);
            Assert.NotEmpty(categories);
            Assert.Contains(categories, c => c.Slug == "test-category");
        }
    }

    public class UserManagementTests : IntegrationTestBase
    {
        [Fact]
        public async Task CreateAdminUser_SuccessfullyCreated()
        {
            // Arrange
            var adminUser = new NguoiDung
            {
                UserName = "admin@test.local",
                Email = "admin@test.local",
                EmailConfirmed = true,
                HoTen = "Admin User"
            };

            // Act
            var result = await UserManager.CreateAsync(adminUser, "Admin@123");

            // Assert
            Assert.True(result.Succeeded);
            var createdUser = await UserManager.FindByEmailAsync("admin@test.local");
            Assert.NotNull(createdUser);
            Assert.Equal("Admin User", createdUser.HoTen);
        }

        [Fact]
        public async Task AddRoleToUser_SuccessfullyAdded()
        {
            // Arrange
            var user = new NguoiDung
            {
                UserName = "roletest@test.local",
                Email = "roletest@test.local"
            };
            await UserManager.CreateAsync(user, "Test@123");

            // Act
            var result = await UserManager.AddToRoleAsync(user, "NhanVien");

            // Assert
            Assert.True(result.Succeeded);
            var isInRole = await UserManager.IsInRoleAsync(user, "NhanVien");
            Assert.True(isInRole);
        }

        [Fact]
        public async Task ChangePassword_Successfully()
        {
            // Arrange
            var user = new NguoiDung
            {
                UserName = "pwdtest@test.local",
                Email = "pwdtest@test.local"
            };
            await UserManager.CreateAsync(user, "OldPassword@123");

            // Act
            var result = await UserManager.ChangePasswordAsync(user, "OldPassword@123", "NewPassword@123");

            // Assert
            Assert.True(result.Succeeded);
            var signInResult = await UserManager.CheckPasswordAsync(user, "NewPassword@123");
            Assert.True(signInResult);
        }
    }
}
