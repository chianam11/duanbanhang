using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Helpers;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.Controllers
{
    public class SanPhamController : AdminBaseController
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SanPhamController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private void LoadSelects()
        {
            ViewBag.DanhMuc = new SelectList(_db.DanhMucs.OrderBy(x => x.ThuTu), "Id", "Ten");
            ViewBag.ThuongHieu = new SelectList(_db.ThuongHieus.OrderBy(x => x.Ten), "Id", "Ten");

            ViewBag.Parents = new SelectList(
                _db.SanPhams
                    .Where(p => p.ParentId == null)
                    .OrderBy(p => p.Ten)
                    .Select(p => new { p.Id, Ten = p.Ten }),
                "Id",
                "Ten");
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 15, bool includeVariants = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 15;

            IQueryable<SanPham> q = _db.SanPhams.AsNoTracking();

            if (!includeVariants)
            {
                q = q.Where(p => p.ParentId == null).Include(p => p.Children);
            }

            q = q
                .Include(p => p.Anhs)
                .Include(p => p.Parent)
                    .ThenInclude(pa => pa!.Anhs)
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu);

            var total = await q.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var list = await q
                .OrderByDescending(p => p.NgayCapNhat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.IncludeVariants = includeVariants;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        public IActionResult Create()
        {
            LoadSelects();
            return View(new SanPham { TrangThai = TrangThaiHienThi.Hien });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham m, List<IFormFile>? files)
        {
            NormalizeProductInput(m);
            ValidateProductFiles(files);

            if (m.ParentId.HasValue)
            {
                var parentExists = await _db.SanPhams.AnyAsync(x => x.Id == m.ParentId && x.ParentId == null);
                if (!parentExists)
                {
                    ModelState.AddModelError(nameof(m.ParentId), "Nhom (cha) khong hop le hoac khong ton tai.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadSelects();
                return View(m);
            }

            m.NgayTao = m.NgayCapNhat = DateTime.UtcNow;
            _db.Add(m);
            await _db.SaveChangesAsync();

            await SaveImagesAsync(m.Id, files);
            TempData["toast"] = "Da them san pham moi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            LoadSelects();
            var sp = await _db.SanPhams
                .Include(p => p.Anhs)
                .Include(p => p.Parent)
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == id);
            return sp == null ? NotFound() : View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPham m, List<IFormFile>? files)
        {
            NormalizeProductInput(m);
            ValidateProductFiles(files);

            if (m.ParentId == m.Id)
            {
                m.ParentId = null;
            }

            if (m.ParentId.HasValue)
            {
                var parent = await _db.SanPhams.FirstOrDefaultAsync(x => x.Id == m.ParentId && x.ParentId == null);
                if (parent == null)
                {
                    ModelState.AddModelError(nameof(m.ParentId), "Nhom (cha) khong hop le hoac khong ton tai.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadSelects();
                return View(m);
            }

            m.NgayCapNhat = DateTime.UtcNow;

            _db.Update(m);
            await _db.SaveChangesAsync();
            await SaveImagesAsync(m.Id, files);

            TempData["toast"] = "Da luu san pham thanh cong.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var sp = await _db.SanPhams
                .Include(p => p.Children)
                .Include(p => p.Anhs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sp == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (sp.ParentId == null && sp.Children.Any())
            {
                TempData["Err"] = "Khong the xoa san pham cha khi van con bien the. Hay xoa hoac di chuyen cac bien the truoc.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var a in sp.Anhs)
            {
                TryDeleteFile(a.Url);
            }

            _db.SanPhams.Remove(sp);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Da xoa san pham.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> XoaAnh(int id)
        {
            var a = await _db.AnhSanPhams.FindAsync(id);
            if (a != null)
            {
                TryDeleteFile(a.Url);
                _db.AnhSanPhams.Remove(a);
                await _db.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> DatAnhChinh(int id)
        {
            var a = await _db.AnhSanPhams.FindAsync(id);
            if (a == null) return NotFound();

            var anhs = _db.AnhSanPhams.Where(x => x.IdSanPham == a.IdSanPham);
            await anhs.ForEachAsync(x => x.LaAnhChinh = false);
            a.LaAnhChinh = true;
            await _db.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Images(int id)
        {
            var p = await _db.SanPhams
                .Include(x => x.Anhs.OrderBy(a => a.ThuTu))
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int id, List<IFormFile> files)
        {
            ValidateProductFiles(files);
            if (!ModelState.IsValid)
            {
                TempData["Err"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Anh tai len khong hop le.";
                return RedirectToAction(nameof(Images), new { id });
            }

            var p = await _db.SanPhams.Include(x => x.Anhs).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadRoot);

            var nextOrder = p.Anhs.Any() ? p.Anhs.Max(a => a.ThuTu) + 1 : 0;

            foreach (var f in files.Where(f => f?.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var savePath = Path.Combine(uploadRoot, fileName);
                await using (var stream = System.IO.File.Create(savePath))
                {
                    await f.CopyToAsync(stream);
                }

                var relUrl = $"/uploads/products/{fileName}";
                _db.AnhSanPhams.Add(new AnhSanPham
                {
                    IdSanPham = id,
                    Url = relUrl,
                    ThuTu = nextOrder++,
                    LaAnhChinh = !p.Anhs.Any()
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Images), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int id, int imageId)
        {
            var p = await _db.SanPhams.Include(x => x.Anhs).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            foreach (var a in p.Anhs)
            {
                a.LaAnhChinh = a.Id == imageId;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Images), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var img = await _db.AnhSanPhams.FirstOrDefaultAsync(a => a.Id == imageId && a.IdSanPham == id);
            if (img == null) return RedirectToAction(nameof(Images), new { id });

            var physical = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);

            _db.AnhSanPhams.Remove(img);
            await _db.SaveChangesAsync();

            var remain = await _db.AnhSanPhams.Where(a => a.IdSanPham == id).OrderBy(a => a.ThuTu).ToListAsync();
            if (remain.Any() && !remain.Any(a => a.LaAnhChinh))
            {
                remain.First().LaAnhChinh = true;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Images), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveImage(int id, int imageId, string dir)
        {
            var list = await _db.AnhSanPhams.Where(a => a.IdSanPham == id).OrderBy(a => a.ThuTu).ToListAsync();

            var idx = list.FindIndex(a => a.Id == imageId);
            if (idx == -1) return RedirectToAction(nameof(Images), new { id });

            var swapWith = dir == "up" ? idx - 1 : idx + 1;
            if (swapWith < 0 || swapWith >= list.Count) return RedirectToAction(nameof(Images), new { id });

            (list[idx].ThuTu, list[swapWith].ThuTu) = (list[swapWith].ThuTu, list[idx].ThuTu);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Images), new { id });
        }

        private async Task SaveImagesAsync(int spId, List<IFormFile>? files)
        {
            if (files == null || files.Count == 0) return;

            var dir = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(dir);

            var first = !await _db.AnhSanPhams.AnyAsync(x => x.IdSanPham == spId);
            var order = (await _db.AnhSanPhams
                .Where(x => x.IdSanPham == spId)
                .Select(x => (int?)x.ThuTu)
                .MaxAsync()) ?? 0;

            foreach (var f in files.Where(f => f?.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var savePath = Path.Combine(dir, fileName);
                await using (var stream = System.IO.File.Create(savePath))
                {
                    await f.CopyToAsync(stream);
                }

                var url = $"/uploads/products/{fileName}";
                _db.AnhSanPhams.Add(new AnhSanPham
                {
                    IdSanPham = spId,
                    Url = url,
                    LaAnhChinh = first,
                    ThuTu = ++order
                });
                first = false;
            }

            await _db.SaveChangesAsync();
        }

        private void TryDeleteFile(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var full = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }

        private static void NormalizeProductInput(SanPham model)
        {
            model.Ten = model.Ten?.Trim() ?? string.Empty;
            model.DisplaySuffix = string.IsNullOrWhiteSpace(model.DisplaySuffix) ? null : model.DisplaySuffix.Trim();
            model.MoTaNgan = string.IsNullOrWhiteSpace(model.MoTaNgan) ? null : model.MoTaNgan.Trim();
            model.MoTaChiTiet = string.IsNullOrWhiteSpace(model.MoTaChiTiet) ? null : model.MoTaChiTiet.Trim();
            model.Mau = string.IsNullOrWhiteSpace(model.Mau) ? null : model.Mau.Trim();
            model.ThuocTinh2 = string.IsNullOrWhiteSpace(model.ThuocTinh2) ? null : model.ThuocTinh2.Trim();
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU.Trim().ToUpperInvariant();
        }

        private void ValidateProductFiles(List<IFormFile>? files)
        {
            if (files == null || files.Count == 0) return;

            foreach (var file in files.Where(f => f != null))
            {
                if (!FileUploadValidation.IsValidImage(file, out var error, maxSizeInMb: 5))
                {
                    ModelState.AddModelError("files", error);
                    break;
                }
            }
        }
    }
}
