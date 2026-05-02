using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;

namespace ShopMVC.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CategoriesCacheKey = "Categories";

        public CategoryService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<DanhMuc>> GetActiveCategoriesAsync()
        {
            if (!_cache.TryGetValue(CategoriesCacheKey, out List<DanhMuc>? categories) || categories is null)
            {
                categories = await _context.DanhMucs
                    .Where(c => c.HienThi)
                    .OrderBy(c => c.ThuTu)
                    .ThenBy(c => c.Ten)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                _cache.Set(CategoriesCacheKey, categories, cacheEntryOptions);
            }

            return categories;
        }

        public void ClearCategoriesCache()
        {
            _cache.Remove(CategoriesCacheKey);
        }
    }
}
