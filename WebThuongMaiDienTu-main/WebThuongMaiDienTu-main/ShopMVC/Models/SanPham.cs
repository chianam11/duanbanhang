using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models
{
    public enum TrangThaiHienThi { An = 0, Hien = 1 }

    [Index(nameof(ParentId))]
    [Index(nameof(IdDanhMuc))]
    [Index(nameof(IdThuongHieu))]
    public class SanPham : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "Tên sản phẩm phải từ 2 đến 250 ký tự.")]
        public string Ten { get; set; } = string.Empty;

        [StringLength(120, ErrorMessage = "Hậu tố hiển thị tối đa 120 ký tự.")]
        public string? DisplaySuffix { get; set; }

        [StringLength(400, ErrorMessage = "Mô tả ngắn tối đa 400 ký tự.")]
        public string? MoTaNgan { get; set; }

        [StringLength(4000, ErrorMessage = "Mô tả chi tiết tối đa 4000 ký tự.")]
        public string? MoTaChiTiet { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        [Precision(18, 2)]
        public decimal Gia { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0.")]
        [Precision(18, 2)]
        public decimal? GiaKhuyenMai { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải lớn hơn hoặc bằng 0.")]
        public int TonKho { get; set; }

        public bool LaNoiBat { get; set; } = false;
        public TrangThaiHienThi TrangThai { get; set; } = TrangThaiHienThi.Hien;
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }

        public int? ParentId { get; set; }
        public SanPham? Parent { get; set; }
        public ICollection<SanPham> Children { get; set; } = new List<SanPham>();

        [StringLength(60, ErrorMessage = "Màu tối đa 60 ký tự.")]
        public string? Mau { get; set; }

        [StringLength(60, ErrorMessage = "Thuộc tính 2 tối đa 60 ký tự.")]
        public string? ThuocTinh2 { get; set; }

        [StringLength(80, ErrorMessage = "SKU tối đa 80 ký tự.")]
        public string? SKU { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int IdDanhMuc { get; set; }

        [ForeignKey(nameof(IdDanhMuc))]
        public DanhMuc? DanhMuc { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thương hiệu.")]
        public int IdThuongHieu { get; set; }

        [ForeignKey(nameof(IdThuongHieu))]
        public ThuongHieu? ThuongHieu { get; set; }

        public ICollection<AnhSanPham> Anhs { get; set; } = new List<AnhSanPham>();
        public virtual ICollection<ChiTietSanPham> ChiTietSanPhams { get; set; } = new List<ChiTietSanPham>();

        [NotMapped]
        public VoucherSanPham? FlashSaleInfo { get; set; }

        [NotMapped]
        public string TenDayDu => string.IsNullOrWhiteSpace(DisplaySuffix) ? Ten : $"{Ten} {DisplaySuffix}";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (GiaKhuyenMai.HasValue && GiaKhuyenMai.Value > Gia)
            {
                yield return new ValidationResult(
                    "Giá khuyến mãi không được lớn hơn giá gốc.",
                    new[] { nameof(GiaKhuyenMai) });
            }

            if (ParentId.HasValue && string.IsNullOrWhiteSpace(DisplaySuffix))
            {
                yield return new ValidationResult(
                    "Biến thể nên có hậu tố hiển thị để phân biệt với sản phẩm cha.",
                    new[] { nameof(DisplaySuffix) });
            }
        }
    }
}
