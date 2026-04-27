using System.ComponentModel.DataAnnotations;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.ViewModels
{
    public class VoucherCreateViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã voucher")]
        [Display(Name = "Mã voucher")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mã voucher phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Mã voucher chỉ được chứa chữ, số, gạch ngang và gạch dưới.")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình")]
        [Display(Name = "Tên/ghi chú voucher")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên voucher phải từ 3 đến 200 ký tự.")]
        public string Ten { get; set; } = "";

        [Display(Name = "% giảm")]
        [Range(0, 100, ErrorMessage = "% giảm từ 0 đến 100")]
        public double? PhanTramGiam { get; set; }

        [Display(Name = "Giảm trực tiếp (đ)")]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giảm trực tiếp phải lớn hơn hoặc bằng 0.")]
        public decimal? GiamTrucTiep { get; set; }

        [Display(Name = "Giảm tối đa (đ)")]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giảm tối đa phải lớn hơn hoặc bằng 0.")]
        public decimal? GiamToiDa { get; set; }

        [Required(ErrorMessage = "Chọn ngày bắt đầu")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Chọn ngày hết hạn")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime NgayHetHan { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Lượt dùng tối đa")]
        [Range(0, int.MaxValue, ErrorMessage = "Lượt dùng tối đa không hợp lệ.")]
        public int SoLanSuDungToiDa { get; set; } = 100;

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        public List<ThuongHieu> AvailableBrands { get; set; } = new();
        public List<DanhMuc> AvailableCategories { get; set; } = new();
        public List<int> SelectedBrandIds { get; set; } = new();
        public List<int> SelectedCategoryIds { get; set; } = new();

        [Display(Name = "Là chương trình Flash Sale")]
        public bool IsFlashSale { get; set; } = false;

        public byte[]? RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NgayHetHan < NgayBatDau)
            {
                yield return new ValidationResult(
                    "Ngày hết hạn phải lớn hơn hoặc bằng ngày bắt đầu.",
                    new[] { nameof(NgayHetHan) });
            }

            var hasPercentDiscount = PhanTramGiam.GetValueOrDefault() > 0;
            var hasFixedDiscount = GiamTrucTiep.GetValueOrDefault() > 0;
            if (!hasPercentDiscount && !hasFixedDiscount)
            {
                yield return new ValidationResult(
                    "Phải nhập giảm theo % hoặc giảm trực tiếp.",
                    new[] { nameof(PhanTramGiam), nameof(GiamTrucTiep) });
            }

            if (GiamToiDa.HasValue && GiamTrucTiep.HasValue && GiamToiDa.Value > 0 && GiamToiDa.Value < GiamTrucTiep.Value)
            {
                yield return new ValidationResult(
                    "Giảm tối đa không được nhỏ hơn mức giảm trực tiếp.",
                    new[] { nameof(GiamToiDa) });
            }
        }
    }
}
