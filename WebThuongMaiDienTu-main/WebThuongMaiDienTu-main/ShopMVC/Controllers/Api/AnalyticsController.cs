using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopMVC.Models;
using ShopMVC.Models.Dto;
using ShopMVC.Services;

namespace ShopMVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "QuanTri")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics (revenue, orders, customers)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStatistics>>> GetDashboard()
        {
            _logger.LogInformation("Dashboard statistics requested");
            var stats = await _analyticsService.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatistics>.Ok(stats, "Dashboard data retrieved successfully"));
        }

        /// <summary>
        /// Get sales analytics for date range
        /// </summary>
        [HttpGet("sales")]
        public async Task<ActionResult<ApiResponse<SalesAnalytics>>> GetSalesAnalytics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            _logger.LogInformation($"Sales analytics requested for {startDate:d} - {endDate:d}");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            if (start > end)
            {
                return BadRequest(ApiResponse<SalesAnalytics>.BadRequest("Start date must be before end date"));
            }

            var analytics = await _analyticsService.GetSalesAnalyticsAsync(start, end);
            return Ok(ApiResponse<SalesAnalytics>.Ok(analytics, "Sales analytics retrieved successfully"));
        }

        /// <summary>
        /// Get product analytics (stock, inventory status)
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<ProductAnalytics>>> GetProductAnalytics()
        {
            _logger.LogInformation("Product analytics requested");
            var analytics = await _analyticsService.GetProductAnalyticsAsync();
            return Ok(ApiResponse<ProductAnalytics>.Ok(analytics, "Product analytics retrieved successfully"));
        }

        /// <summary>
        /// Get customer analytics (retention, spending)
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse<CustomerAnalytics>>> GetCustomerAnalytics()
        {
            _logger.LogInformation("Customer analytics requested");
            var analytics = await _analyticsService.GetCustomerAnalyticsAsync();
            return Ok(ApiResponse<CustomerAnalytics>.Ok(analytics, "Customer analytics retrieved successfully"));
        }

        /// <summary>
        /// Get complete analytics overview (all metrics)
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<object>>> GetCompleteOverview()
        {
            _logger.LogInformation("Complete analytics overview requested");

            var dashboard = await _analyticsService.GetDashboardStatsAsync();
            var sales = await _analyticsService.GetSalesAnalyticsAsync(DateTime.Today.AddDays(-30), DateTime.Today);
            var products = await _analyticsService.GetProductAnalyticsAsync();
            var customers = await _analyticsService.GetCustomerAnalyticsAsync();

            var overview = new
            {
                Dashboard = dashboard,
                Sales = sales,
                Products = products,
                Customers = customers,
                GeneratedAt = DateTime.Now
            };

            return Ok(ApiResponse<object>.Ok(overview, "Complete analytics overview retrieved successfully"));
        }
    }
}