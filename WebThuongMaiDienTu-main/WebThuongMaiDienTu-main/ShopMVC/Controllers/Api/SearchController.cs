using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Models.Dto;

namespace ShopMVC.Controllers.Api
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Advanced search with multiple filters
        /// </summary>
        [HttpGet("advanced")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SanPham>>>> AdvancedSearch(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? isOnSale,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "newest")
        {
            try
            {
                if (pageSize > 100) pageSize = 100;
                if (page < 1) page = 1;

                var query = _context.SanPhams
                    .Include(p => p.DanhMuc)
                    .Include(p => p.ThuongHieu)
                    .Include(p => p.Anhs)
                    .Where(p => p.IsActive);

                // Keyword filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(p =>
                        p.Ten.ToLower().Contains(keyword) ||
                        p.MoTaNgan.ToLower().Contains(keyword));
                }

                // Category filter
                if (categoryId > 0)
                {
                    query = query.Where(p => p.IdDanhMuc == categoryId);
                }

                // Brand filter
                if (brandId > 0)
                {
                    query = query.Where(p => p.IdThuongHieu == brandId);
                }

                // Price range filter
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Gia >= minPrice.Value);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Gia <= maxPrice.Value);
                }

                // On sale filter
                if (isOnSale.HasValue && isOnSale.Value)
                {
                    query = query.Where(p => p.GiaKhuyenMai.HasValue && p.GiaKhuyenMai < p.Gia);
                }

                // Sorting
                query = sortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Gia),
                    "price_desc" => query.OrderByDescending(p => p.Gia),
                    "rating" => query.OrderByDescending(p => p.LaNoiBat).ThenByDescending(p => p.NgayTao),
                    "popular" => query.OrderByDescending(p => p.LaNoiBat),
                    _ => query.OrderByDescending(p => p.NgayTao)  // newest (default)
                };

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new PaginatedResponse<SanPham>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                };

                _logger.LogInformation($"Advanced search: keyword={keyword}, category={categoryId}, results={total}");
                return Ok(ApiResponse<PaginatedResponse<SanPham>>.Ok(response, "Tìm kiếm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Advanced search error");
                throw;
            }
        }

        /// <summary>
        /// Get product categories for filter
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<DanhMuc>>>> GetCategories()
        {
            var categories = await _context.DanhMucs
                .Where(c => c.HienThi)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            return Ok(ApiResponse<List<DanhMuc>>.Ok(categories));
        }

        /// <summary>
        /// Get brands for filter
        /// </summary>
        [HttpGet("brands")]
        public async Task<ActionResult<ApiResponse<List<ThuongHieu>>>> GetBrands()
        {
            var brands = await _context.ThuongHieus
                .Where(b => b.HienThi)
                .OrderBy(b => b.Ten)
                .ToListAsync();

            return Ok(ApiResponse<List<ThuongHieu>>.Ok(brands));
        }

        /// <summary>
        /// Get price range for slider
        /// </summary>
        [HttpGet("price-range")]
        public async Task<ActionResult<ApiResponse<object>>> GetPriceRange()
        {
            var minPrice = await _context.SanPhams
                .Where(p => p.IsActive)
                .MinAsync(p => (decimal?)p.Gia) ?? 0;

            var maxPrice = await _context.SanPhams
                .Where(p => p.IsActive)
                .MaxAsync(p => (decimal?)p.Gia) ?? 999999999;

            var result = new { MinPrice = minPrice, MaxPrice = maxPrice };
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}