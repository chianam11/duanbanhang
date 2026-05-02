using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ShopMVC.Configuration;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;

namespace ShopMVC.Services
{
    public class ShopInfoService : IShopInfoService
    {
        private const string ShopNameCacheKey = "shop:name";

        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ShopSettings _fallbackSettings;

        public ShopInfoService(
            AppDbContext db,
            IMemoryCache cache,
            IOptions<ShopSettings> fallbackSettings)
        {
            _db = db;
            _cache = cache;
            _fallbackSettings = fallbackSettings.Value;
        }

        public async Task<string> GetShopNameAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<string>(ShopNameCacheKey, out var cachedName) &&
                !string.IsNullOrWhiteSpace(cachedName))
            {
                return cachedName;
            }

            var dbName = await _db.SystemSettings
                .AsNoTracking()
                .Where(x => x.SettingKey == SystemSetting.ShopNameKey)
                .Select(x => x.SettingValue)
                .FirstOrDefaultAsync(cancellationToken);

            var resolvedName = string.IsNullOrWhiteSpace(dbName)
                ? (_fallbackSettings.ShopName?.Trim() ?? "ShopMVC")
                : dbName.Trim();

            _cache.Set(ShopNameCacheKey, resolvedName, TimeSpan.FromMinutes(30));
            return resolvedName;
        }

        public async Task UpdateShopNameAsync(string shopName, CancellationToken cancellationToken = default)
        {
            var normalizedName = string.IsNullOrWhiteSpace(shopName)
                ? (_fallbackSettings.ShopName?.Trim() ?? "ShopMVC")
                : shopName.Trim();

            var setting = await _db.SystemSettings
                .FirstOrDefaultAsync(x => x.SettingKey == SystemSetting.ShopNameKey, cancellationToken);

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = SystemSetting.ShopNameKey,
                    SettingValue = normalizedName,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = normalizedName;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(cancellationToken);
            _cache.Set(ShopNameCacheKey, normalizedName, TimeSpan.FromMinutes(30));
        }
    }
}
