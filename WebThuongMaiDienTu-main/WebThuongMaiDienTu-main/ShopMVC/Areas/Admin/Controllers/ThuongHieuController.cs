using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "QuanTri")]
    public class ThuongHieuController : AdminBaseController
    {
        private readonly AppDbContext _db;

        public ThuongHieuController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string? q = null, string sort = "name_asc")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            IQueryable<ThuongHieu> query = _db.ThuongHieus.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim();
                query = query.Where(x => x.Ten.Contains(kw));
            }

            query = sort switch
            {
                "name_desc" => query.OrderByDescending(x => x.Ten),
                "id_desc" => query.OrderByDescending(x => x.Id),
                "id_asc" => query.OrderBy(x => x.Id),
                _ => query.OrderBy(x => x.Ten)
            };

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;
            ViewBag.Q = q;
            ViewBag.Sort = sort;

            return View(items);
        }

        public IActionResult Create() => View(new ThuongHieu { HienThi = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuongHieu m)
        {
            NormalizeBrandInput(m);

            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var name = m.Ten.ToLowerInvariant();
            var existed = await _db.ThuongHieus.AnyAsync(x => x.Ten.ToLower() == name);

            if (existed)
            {
                ModelState.AddModelError(nameof(m.Ten), "Ten thuong hieu da ton tai.");
                return View(m);
            }

            _db.Add(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var th = await _db.ThuongHieus.FindAsync(id);
            return th == null ? NotFound() : View(th);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThuongHieu m)
        {
            NormalizeBrandInput(m);

            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var name = m.Ten.ToLowerInvariant();
            var existed = await _db.ThuongHieus.AnyAsync(x => x.Id != m.Id && x.Ten.ToLower() == name);

            if (existed)
            {
                ModelState.AddModelError(nameof(m.Ten), "Ten thuong hieu da ton tai.");
                return View(m);
            }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var th = await _db.ThuongHieus.FindAsync(id);
            if (th == null)
            {
                TempData["Err"] = "Thuong hieu khong ton tai.";
                return RedirectToAction(nameof(Index));
            }

            var inUse = await _db.SanPhams.AnyAsync(p => p.IdThuongHieu == id);
            if (inUse)
            {
                TempData["Err"] = "Khong the xoa vi con san pham thuoc thuong hieu nay. Hay chuyen sang thuong hieu khac hoac xoa san pham truoc.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.ThuongHieus.Remove(th);
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Da xoa thuong hieu.";
            }
            catch
            {
                TempData["Err"] = "Xoa that bai do rang buoc du lieu.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeBrandInput(ThuongHieu model)
        {
            model.Ten = model.Ten?.Trim() ?? string.Empty;
            model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? null : model.Slug.Trim().ToLowerInvariant();
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
        }
    }
}
