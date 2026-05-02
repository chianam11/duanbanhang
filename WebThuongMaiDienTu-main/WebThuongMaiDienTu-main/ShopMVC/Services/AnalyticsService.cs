using ShopMVC.Data;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Models;

namespace ShopMVC.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardStatistics> GetDashboardStatsAsync();
        Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<ProductAnalytics> GetProductAnalyticsAsync();
        Task<CustomerAnalytics> GetCustomerAnalyticsAsync();
    }

    public class DashboardStatistics
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersToday { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenue> DailyRevenueData { get; set; } = new();
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Orders { get; set; }
    }

    public class SalesAnalytics
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<CategorySales> TopCategories { get; set; } = new();
        public List<ProductSales> TopProducts { get; set; } = new();
    }

    public class CategorySales
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public int Orders { get; set; }
    }

    public class ProductSales
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductAnalytics
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<ProductStock> StockStatus { get; set; } = new();
    }

    public class ProductStock
    {
        public string ProductName { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerAnalytics
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public List<TopCustomer> TopCustomers { get; set; } = new();
    }

    public class TopCustomer
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStatistics> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // Orders with completed status (HoanTat = 3)
            var confirmedOrders = _context.DonHangs.Where(o => o.TrangThai == TrangThaiDonHang.HoanTat);

            var todayRevenue = await confirmedOrders
                .Where(o => o.NgayDat >= today)
                .SumAsync(o => o.TongThanhToan);

            var monthRevenue = await confirmedOrders
                .Where(o => o.NgayDat >= monthStart)
                .SumAsync(o => o.TongThanhToan);

            var yearRevenue = await confirmedOrders
                .Where(o => o.NgayDat >= yearStart)
                .SumAsync(o => o.TongThanhToan);

            var totalOrders = await _context.DonHangs.CountAsync();
            var todayOrders = await _context.DonHangs
                .Where(o => o.NgayDat >= today)
                .CountAsync();

            var totalProducts = await _context.SanPhams.CountAsync(p => p.IsActive);
            var totalCustomers = await _context.Users.CountAsync();
            var newCustomersToday = 0; // TODO: Add CreatedDate tracking to NguoiDung model

            var avgOrderValue = totalOrders > 0 ? yearRevenue / totalOrders : 0;

            // Daily revenue for chart
            var dailyData = await GetDailyRevenueAsync(today.AddDays(-30), today);

            return new DashboardStatistics
            {
                TodayRevenue = todayRevenue,
                MonthRevenue = monthRevenue,
                YearRevenue = yearRevenue,
                TotalOrders = totalOrders,
                TodayOrders = todayOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                NewCustomersToday = newCustomersToday,
                AverageOrderValue = avgOrderValue,
                DailyRevenueData = dailyData
            };
        }

        public async Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var orders = _context.DonHangs
                .Where(o => o.NgayDat >= startDate && o.NgayDat <= endDate && o.TrangThai == TrangThaiDonHang.HoanTat);

            var totalSales = await orders.SumAsync(o => o.TongThanhToan);
            var orderCount = await orders.CountAsync();

            // Top categories
            var topCategories = await _context.DonHangChiTiets
                .Join(_context.SanPhams, dt => dt.IdSanPham, p => p.Id, (dt, p) => new { dt, p })
                .Join(_context.DanhMucs, x => x.p.IdDanhMuc, c => c.Id, (x, c) => new { x.dt, x.p, c })
                .GroupBy(g => g.c.Ten)
                .Select(grp => new CategorySales
                {
                    CategoryName = grp.Key,
                    Sales = grp.Sum(x => x.dt.ThanhTien),
                    Orders = grp.Count()
                })
                .OrderByDescending(x => x.Sales)
                .Take(10)
                .ToListAsync();

            // Top products
            var topProducts = await _context.DonHangChiTiets
                .Join(_context.SanPhams, dt => dt.IdSanPham, p => p.Id, (dt, p) => new { dt, p })
                .GroupBy(g => new { g.p.Id, g.p.Ten })
                .Select(grp => new ProductSales
                {
                    ProductId = grp.Key.Id,
                    ProductName = grp.Key.Ten,
                    QuantitySold = grp.Sum(x => x.dt.SoLuong),
                    TotalRevenue = grp.Sum(x => x.dt.ThanhTien)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToListAsync();

            return new SalesAnalytics
            {
                TotalSales = totalSales,
                TotalOrders = orderCount,
                AverageOrderValue = orderCount > 0 ? totalSales / orderCount : 0,
                TopCategories = topCategories,
                TopProducts = topProducts
            };
        }

        public async Task<ProductAnalytics> GetProductAnalyticsAsync()
        {
            var totalProducts = await _context.SanPhams.CountAsync();
            var activeProducts = await _context.SanPhams.CountAsync(p => p.IsActive);
            var lowStockProducts = await _context.SanPhams.CountAsync(p => p.TonKho > 0 && p.TonKho <= 10);
            var outOfStockProducts = await _context.SanPhams.CountAsync(p => p.TonKho == 0);

            var stockStatus = await _context.SanPhams
                .Where(p => p.TonKho <= 20)
                .Select(p => new ProductStock
                {
                    ProductName = p.Ten,
                    Stock = p.TonKho,
                    Price = p.Gia
                })
                .OrderBy(p => p.Stock)
                .Take(20)
                .ToListAsync();

            return new ProductAnalytics
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                StockStatus = stockStatus
            };
        }

        public async Task<CustomerAnalytics> GetCustomerAnalyticsAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalCustomers = await _context.Users.CountAsync();
            var newThisMonth = 0; // TODO: Add CreatedDate tracking to NguoiDung model

            var returningCustomers = await _context.Users
                .Where(u => _context.DonHangs.Count(o => o.UserId == u.Id) > 1)
                .CountAsync();

            var avgCustomerValue = await _context.DonHangs
                .Where(o => o.TrangThai == TrangThaiDonHang.HoanTat)
                .GroupBy(o => o.UserId)
                .Select(g => g.Sum(o => o.TongThanhToan))
                .AverageAsync();

            var topCustomers = await _context.DonHangs
                .Where(o => o.TrangThai == TrangThaiDonHang.HoanTat)
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Orders = g.Count(),
                    TotalSpent = g.Sum(o => o.TongThanhToan)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            // Get user details
            var topCustomersWithDetails = new List<TopCustomer>();
            foreach (var customer in topCustomers)
            {
                var user = await _context.Users.FindAsync(customer.UserId);
                if (user != null)
                {
                    topCustomersWithDetails.Add(new TopCustomer
                    {
                        Email = user.Email ?? string.Empty,
                        Name = user.UserName ?? string.Empty,
                        Orders = customer.Orders,
                        TotalSpent = customer.TotalSpent
                    });
                }
            }

            return new CustomerAnalytics
            {
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newThisMonth,
                ReturningCustomers = returningCustomers,
                AverageCustomerValue = (decimal)avgCustomerValue,
                TopCustomers = topCustomersWithDetails
            };
        }

        private async Task<List<DailyRevenue>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DonHangs
                .Where(o => o.NgayDat >= startDate && o.NgayDat <= endDate && o.TrangThai == TrangThaiDonHang.HoanTat)
                .GroupBy(o => o.NgayDat.Date)
                .Select(g => new DailyRevenue
                {
                    Date = g.Key,
                    Amount = g.Sum(o => o.TongThanhToan),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }
    }
}
