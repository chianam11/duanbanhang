using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Helpers;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannerController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _db.Banners.OrderBy(b => b.ThuTu).ToListAsync();
            return View(banners);
        }

        public IActionResult Create()
        {
            return View(new Banner { HienThi = true, ThuTu = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile? fileAnh)
        {
            banner.TenBanner = banner.TenBanner?.Trim();

            if (!FileUploadValidation.IsValidImage(fileAnh, out var imageError, required: true, maxSizeInMb: 5))
                ModelState.AddModelError("fileAnh", imageError);

            if (!ModelState.IsValid)
                return View(banner);

            if (fileAnh != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(fileAnh.FileName);
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "banners");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileAnh.CopyToAsync(fileStream);
                }

                banner.HinhAnh = "/images/banners/" + fileName;
            }

            _db.Add(banner);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            return banner == null ? NotFound() : View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? fileAnh)
        {
            banner.TenBanner = banner.TenBanner?.Trim();

            if (id != banner.Id)
                return NotFound();

            var bannerDb = await _db.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (bannerDb == null)
                return NotFound();

            if (!FileUploadValidation.IsValidImage(fileAnh, out var imageError, maxSizeInMb: 5))
                ModelState.AddModelError("fileAnh", imageError);

            if (!ModelState.IsValid)
            {
                banner.HinhAnh = bannerDb.HinhAnh;
                return View(banner);
            }

            if (fileAnh != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(fileAnh.FileName);
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "banners");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileAnh.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(bannerDb.HinhAnh))
                {
                    var oldPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        bannerDb.HinhAnh.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                banner.HinhAnh = "/images/banners/" + fileName;
            }
            else
            {
                banner.HinhAnh = bannerDb.HinhAnh;
            }

            try
            {
                _db.Update(banner);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Banners.AnyAsync(b => b.Id == id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner != null)
            {
                if (!string.IsNullOrEmpty(banner.HinhAnh))
                {
                    var oldPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        banner.HinhAnh.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                _db.Banners.Remove(banner);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
