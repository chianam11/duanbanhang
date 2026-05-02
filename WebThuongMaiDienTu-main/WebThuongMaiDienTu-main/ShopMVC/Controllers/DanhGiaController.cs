using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Helpers;
using ShopMVC.Models;
using ShopMVC.Models.ViewModels;
using System.Security.Claims;

namespace ShopMVC.Controllers
{
    [Authorize]
    public class DanhGiaController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _hostEnv;

        public DanhGiaController(AppDbContext db, IWebHostEnvironment hostEnv)
        {
            _db = db;
            _hostEnv = hostEnv;
        }

        [HttpGet]
        public async Task<IActionResult> Tao(int idSanPham, int idDonHang)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chiTietDon = await _db.DonHangChiTiets
                .FirstOrDefaultAsync(ct => ct.IdDonHang == idDonHang
                    && ct.IdSanPham == idSanPham
                    && ct.DonHang != null
                    && ct.DonHang.UserId == userId);

            if (chiTietDon == null)
            {
                TempData["error"] = "Khong tim thay san pham trong don hang cua ban.";
                return RedirectToAction("CuaToi", "DonHang");
            }

            var daDanhGia = await _db.DanhGias.AnyAsync(d =>
                d.IdDonHang == idDonHang &&
                d.IdSanPham == idSanPham &&
                d.UserId == userId);

            if (daDanhGia)
            {
                TempData["error"] = "Ban da danh gia san pham nay cho don hang nay roi.";
                return RedirectToAction("ChiTiet", "DonHang", new { id = idDonHang });
            }

            var sanPham = await _db.SanPhams
                .Include(p => p.Anhs)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == idSanPham);

            if (sanPham == null)
            {
                return NotFound();
            }

            var vm = new DanhGiaVM
            {
                IdSanPham = idSanPham,
                IdDonHang = idDonHang,
                TenSanPham = sanPham.TenDayDu,
                AnhSanPham = sanPham.Anhs
                    .OrderByDescending(a => a.LaAnhChinh)
                    .ThenBy(a => a.ThuTu)
                    .Select(a => a.Url)
                    .FirstOrDefault()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tao(DanhGiaVM vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            vm.NoiDung = string.IsNullOrWhiteSpace(vm.NoiDung) ? null : vm.NoiDung.Trim();

            if (!FileUploadValidation.IsValidImage(vm.FileHinhAnh, out var imageError, maxSizeInMb: 5))
            {
                ModelState.AddModelError(nameof(vm.FileHinhAnh), imageError);
            }

            var chiTietDon = await _db.DonHangChiTiets
                .FirstOrDefaultAsync(ct => ct.IdDonHang == vm.IdDonHang
                    && ct.IdSanPham == vm.IdSanPham
                    && ct.DonHang != null
                    && ct.DonHang.UserId == userId);
            if (chiTietDon == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var sanPham = await _db.SanPhams
                    .Include(p => p.Anhs)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == vm.IdSanPham);

                if (sanPham != null)
                {
                    vm.TenSanPham = sanPham.TenDayDu;
                    vm.AnhSanPham = sanPham.Anhs
                        .OrderByDescending(a => a.LaAnhChinh)
                        .ThenBy(a => a.ThuTu)
                        .Select(a => a.Url)
                        .FirstOrDefault();
                }

                return View(vm);
            }

            var danhGia = new DanhGia
            {
                IdSanPham = vm.IdSanPham,
                IdDonHang = vm.IdDonHang,
                UserId = userId!,
                SoSao = vm.SoSao!.Value,
                NoiDung = vm.NoiDung,
                HienThiTen = vm.HienThiTen,
                TrangThai = TrangThaiDanhGia.ChoDuyet,
                NgayTao = DateTime.Now
            };

            if (vm.FileHinhAnh != null && vm.FileHinhAnh.Length > 0)
            {
                var uploadsDir = Path.Combine(_hostEnv.WebRootPath, "uploads", "reviews");
                Directory.CreateDirectory(uploadsDir);

                var safeName = Path.GetFileName(vm.FileHinhAnh.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{safeName}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.FileHinhAnh.CopyToAsync(fileStream);
                }

                danhGia.HinhAnh = "/uploads/reviews/" + uniqueFileName;
            }

            _db.DanhGias.Add(danhGia);
            await _db.SaveChangesAsync();

            TempData["success"] = "Gui danh gia thanh cong. Danh gia cua ban dang cho duyet.";
            return RedirectToAction("ChiTiet", "DonHang", new { id = vm.IdDonHang });
        }
    }
}
