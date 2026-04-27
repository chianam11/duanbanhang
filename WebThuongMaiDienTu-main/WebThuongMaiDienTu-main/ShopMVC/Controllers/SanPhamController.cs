using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Models.ViewModels;

namespace ShopMVC.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _db;
        public SanPhamController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(
             int? idDanhMuc, int? idThuongHieu,
             decimal? giaMin, decimal? giaMax,
             string? tuKhoa, string? sapXep,
             int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            var activeFlashSale = await _db.Vouchers
                .Where(v => v.IsFlashSale && v.IsActive && DateTime.Now >= v.NgayBatDau && DateTime.Now <= v.NgayHetHan)
                .OrderByDescending(v => v.NgayBatDau)
                .FirstOrDefaultAsync();

            var flashSaleMap = activeFlashSale == null
                ? new Dictionary<int, VoucherSanPham>()
                : await _db.VoucherSanPhams
                    .Where(vp => vp.VoucherId == activeFlashSale.Id)
                    .ToDictionaryAsync(vp => vp.SanPhamId, vp => vp);

            var products = await _db.SanPhams
                .Include(p => p.ThuongHieu)
                .Include(p => p.DanhMuc)
                .Include(p => p.Anhs)
                .Where(p => p.TrangThai == TrangThaiHienThi.Hien
                        && p.IsActive
                        && (p.ThuongHieu == null || p.ThuongHieu.HienThi))
                .ToListAsync();

            decimal EffectivePrice(SanPham p)
            {
                if (flashSaleMap.TryGetValue(p.Id, out var fsItem) && fsItem.GiaGiam.HasValue)
                    return fsItem.GiaGiam.Value;
                return p.GiaKhuyenMai ?? p.Gia;
            }

            var filteredProducts = products.AsEnumerable();

            if (idDanhMuc.HasValue)
                filteredProducts = filteredProducts.Where(p => p.IdDanhMuc == idDanhMuc.Value);

            if (idThuongHieu.HasValue)
                filteredProducts = filteredProducts.Where(p => p.IdThuongHieu == idThuongHieu.Value);

            if (giaMin.HasValue)
                filteredProducts = filteredProducts.Where(p => EffectivePrice(p) >= giaMin.Value);

            if (giaMax.HasValue)
                filteredProducts = filteredProducts.Where(p => EffectivePrice(p) <= giaMax.Value);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim().ToLowerInvariant();
                filteredProducts = filteredProducts.Where(p =>
                    p.Ten.ToLowerInvariant().Contains(kw) ||
                    (p.MoTaNgan ?? string.Empty).ToLowerInvariant().Contains(kw));
            }

            sapXep = string.IsNullOrWhiteSpace(sapXep) ? "moi" : sapXep.ToLowerInvariant();

            var groupedProducts = filteredProducts
                .GroupBy(p => p.ParentId ?? p.Id)
                .Select(g =>
                {
                    var items = g.ToList();
                    var representative = sapXep switch
                    {
                        "gia-asc" => items.OrderBy(EffectivePrice).ThenByDescending(x => x.Id).First(),
                        "gia-desc" => items.OrderByDescending(EffectivePrice).ThenByDescending(x => x.Id).First(),
                        _ => items.OrderByDescending(x => x.NgayCapNhat).ThenByDescending(x => x.Id).First()
                    };

                    var sortValue = sapXep switch
                    {
                        "gia-asc" => EffectivePrice(representative),
                        "gia-desc" => EffectivePrice(representative),
                        _ => 0m
                    };

                    var sortDate = items.Max(x => x.NgayCapNhat == default ? x.NgayTao : x.NgayCapNhat);

                    return new
                    {
                        GroupId = g.Key,
                        Representative = representative,
                        SortValue = sortValue,
                        SortDate = sortDate
                    };
                });

            groupedProducts = sapXep switch
            {
                "gia-asc" => groupedProducts.OrderBy(x => x.SortValue).ThenByDescending(x => x.SortDate),
                "gia-desc" => groupedProducts.OrderByDescending(x => x.SortValue).ThenByDescending(x => x.SortDate),
                _ => groupedProducts.OrderByDescending(x => x.SortDate).ThenByDescending(x => x.Representative.Id)
            };

            var totalGroups = groupedProducts.Count();

            var reps = groupedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Representative)
                .ToList();

            // 8) Lấy map biến thể
            var parentIds = reps.Select(r => r.ParentId ?? r.Id).Distinct().ToList();
            var siblingRaw = await _db.SanPhams
                .Include(x => x.Anhs)
                .Where(x => x.IsActive
                          && x.TrangThai == TrangThaiHienThi.Hien
                          && parentIds.Contains(x.ParentId ?? x.Id))
                .Select(x => new
                {
                    GroupId = x.ParentId ?? x.Id,
                    x.Id,
                    x.Mau,
                    x.ThuocTinh2,
                    FirstImg = x.Anhs.OrderByDescending(a => a.LaAnhChinh).ThenBy(a => a.ThuTu).Select(a => a.Url).FirstOrDefault()
                })
                .ToListAsync();

            var siblingsMap = siblingRaw
                .GroupBy(x => x.GroupId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => new
                    {
                        v.Id,
                        v.Mau,
                        v.ThuocTinh2,
                        v.FirstImg
                    }).ToList()
                );

            ViewBag.FlashSaleMap = flashSaleMap;

            var vm = new SanPhamListVM
            {
                Items = reps,
                DanhMucs = await _db.DanhMucs.Where(x => x.HienThi).OrderBy(x => x.ThuTu).ToListAsync(),
                ThuongHieus = await _db.ThuongHieus.Where(x => x.HienThi).OrderBy(x => x.Ten).ToListAsync(),
                IdDanhMuc = idDanhMuc,
                IdThuongHieu = idThuongHieu,
                GiaMin = giaMin,
                GiaMax = giaMax,
                TuKhoa = tuKhoa,
                SapXep = sapXep,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalGroups
            };

            ViewBag.SiblingsMap = siblingsMap;

            return View(vm);
        }

        // ... (Các hàm ChiTiet, BienThe, RangeDayMonth giữ nguyên không đổi) ...
        static string RangeDayMonth(DateTime from, DateTime to)
        {
            string Th(int m) => "Th" + m;
            if (from.Month == to.Month) return $"{from.Day}–{to.Day} {Th(from.Month)}";
            return $"{from:dd/MM}–{to:dd/MM}";
        }

        public async Task<IActionResult> ChiTiet(int? id, string? slug, bool openChat = false)
        {
            var query = _db.SanPhams
                .Include(p => p.ThuongHieu)
                .Include(p => p.DanhMuc)
                .Include(p => p.Anhs)
                .AsQueryable();

            query = query.Where(p => p.TrangThai == TrangThaiHienThi.Hien
                                  && (p.ThuongHieu == null || p.ThuongHieu.HienThi)
                                  && p.IsActive);

            SanPham? sp = null;
            if (id.HasValue)
                sp = await query.FirstOrDefaultAsync(p => p.Id == id.Value);
            else if (!string.IsNullOrWhiteSpace(slug))
                sp = await query.FirstOrDefaultAsync(p => p.Ten.Replace(' ', '-').ToLower() == slug!.ToLower());

            if (sp == null) return NotFound();

            // Sắp xếp ảnh
            sp.Anhs = sp.Anhs
                .OrderByDescending(a => a.LaAnhChinh)
                .ThenBy(a => a.ThuTu)
                .ToList();

            // Lấy các biến thể (siblings)
            var parentId = sp.ParentId ?? sp.Id;
            var siblings = await _db.SanPhams
                .Include(x => x.Anhs)
                .Where(x => x.IsActive && x.TrangThai == TrangThaiHienThi.Hien
                          && (x.ParentId == parentId || x.Id == parentId))
                .OrderBy(x => x.Mau).ThenBy(x => x.ThuocTinh2)
                .ToListAsync();

            // Label thuộc tính 2 theo danh mục
            string? label2 = null;
            var slugDm = sp.DanhMuc?.Slug?.ToLower();
            if (slugDm == "thoi-trang") label2 = "Size";
            else if (slugDm == "dien-thoai") label2 = "Dung lượng";
            else if (slugDm == "laptop") label2 = "RAM/SSD";

            int leadMinDays = 2;
            int leadMaxDays = 4;
            var today = DateTime.Today;
            var etaFrom = today.AddDays(leadMinDays);
            var etaTo = today.AddDays(leadMaxDays);

            var vm = new ProductDetailsVM
            {
                Product = sp,
                Siblings = siblings,
                ShippingEtaText = RangeDayMonth(etaFrom, etaTo),
                ThuocTinh2Label = label2
            };

            // ================== THÊM ĐOẠN NÀY ==================
            // Lấy danh sách đánh giá cho sản phẩm này
            var danhGias = await _db.DanhGias
                .Where(d => d.IdSanPham == sp.Id)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            ViewBag.DanhGiasSP = danhGias;
            // ===================================================

            ViewBag.OpenChat = openChat;
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> BienThe(int id)
        {
            var sp = await _db.SanPhams
                .Include(x => x.Anhs)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.TrangThai == TrangThaiHienThi.Hien);
            if (sp == null) return NotFound();

            var imgs = sp.Anhs.OrderByDescending(a => a.LaAnhChinh).ThenBy(a => a.ThuTu).Select(a => a.Url).ToList();
            string GiaFmt(decimal v) => string.Format("{0:n0} đ", v);

            return Json(new
            {
                id = sp.Id,
                ten = sp.Ten,
                gia = sp.Gia,
                giaKhuyenMai = sp.GiaKhuyenMai,
                giaText = sp.GiaKhuyenMai.HasValue ? GiaFmt(sp.GiaKhuyenMai.Value) : GiaFmt(sp.Gia),
                giaGocText = sp.GiaKhuyenMai.HasValue ? GiaFmt(sp.Gia) : null,
                tonKho = sp.TonKho,
                moTaNgan = sp.MoTaNgan,
                images = imgs
            });
        }
    }
}
