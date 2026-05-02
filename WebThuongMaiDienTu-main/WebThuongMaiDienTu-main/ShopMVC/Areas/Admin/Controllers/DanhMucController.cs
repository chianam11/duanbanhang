using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Helpers;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "QuanTri")]
    public class DanhMucController : AdminBaseController
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DanhMucController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
            => View(await _db.DanhMucs.OrderBy(x => x.ThuTu).ToListAsync());

        public IActionResult Create() => View(new DanhMuc { HienThi = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc m, IFormFile? icon)
        {
            NormalizeCategoryInput(m);
            ValidateIcon(icon);

            if (!ModelState.IsValid) return View(m);

            var name = m.Ten.ToLowerInvariant();
            var slug = (m.Slug ?? string.Empty).ToLowerInvariant();

            bool existed = await _db.DanhMucs
                .AnyAsync(x => x.Ten.ToLower() == name || (!string.IsNullOrEmpty(slug) && x.Slug != null && x.Slug.ToLower() == slug));

            if (existed)
            {
                ModelState.AddModelError(nameof(m.Ten), "Tên hoặc slug danh mục đã tồn tại.");
                return View(m);
            }

            if (icon != null && icon.Length > 0)
            {
                var ext = Path.GetExtension(icon.FileName).ToLowerInvariant();
                var fileName = $"{(string.IsNullOrWhiteSpace(m.Slug) ? $"dm-{Guid.NewGuid():N}" : m.Slug.ToLowerInvariant())}{ext}";
                var folder = Path.Combine(_env.WebRootPath, "images", "categories");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, fileName);
                using var s = System.IO.File.Create(path);
                await icon.CopyToAsync(s);
                m.IconUrl = $"/images/categories/{fileName}";
            }

            _db.Add(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dm = await _db.DanhMucs.FindAsync(id);
            return dm == null ? NotFound() : View(dm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhMuc m, IFormFile? icon)
        {
            NormalizeCategoryInput(m);
            ValidateIcon(icon);

            if (!ModelState.IsValid) return View(m);

            var name = m.Ten.ToLowerInvariant();
            var slug = (m.Slug ?? string.Empty).ToLowerInvariant();

            bool existed = await _db.DanhMucs
                .AnyAsync(x => x.Id != m.Id && (x.Ten.ToLower() == name || (!string.IsNullOrEmpty(slug) && x.Slug != null && x.Slug.ToLower() == slug)));

            if (existed)
            {
                ModelState.AddModelError(nameof(m.Ten), "Tên hoặc slug danh mục đã tồn tại.");
                return View(m);
            }

            if (icon != null && icon.Length > 0)
            {
                var ext = Path.GetExtension(icon.FileName).ToLowerInvariant();
                var fileName = $"{(string.IsNullOrWhiteSpace(m.Slug) ? $"dm-{Guid.NewGuid():N}" : m.Slug.ToLowerInvariant())}{ext}";
                var folder = Path.Combine(_env.WebRootPath, "images", "categories");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, fileName);
                using var s = System.IO.File.Create(path);
                await icon.CopyToAsync(s);
                m.IconUrl = $"/images/categories/{fileName}";
            }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dm = await _db.DanhMucs.FindAsync(id);
            if (dm == null)
            {
                TempData["Error"] = "Danh mục không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            bool hasProducts = await _db.SanPhams.AnyAsync(p => p.IdDanhMuc == id);
            if (hasProducts)
            {
                TempData["Error"] = "Danh mục đang có sản phẩm, không thể xoá. Hãy chuyển sản phẩm sang danh mục khác hoặc xoá sản phẩm trước.";
                return RedirectToAction(nameof(Index));
            }

            _db.DanhMucs.Remove(dm);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã xoá danh mục.";
            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeCategoryInput(DanhMuc model)
        {
            model.Ten = model.Ten?.Trim() ?? string.Empty;
            model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? null : model.Slug.Trim().ToLowerInvariant();
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
        }

        private void ValidateIcon(IFormFile? icon)
        {
            if (!FileUploadValidation.IsValidImage(icon, out var error, maxSizeInMb: 3))
            {
                ModelState.AddModelError("icon", error);
            }
        }
    }
}
