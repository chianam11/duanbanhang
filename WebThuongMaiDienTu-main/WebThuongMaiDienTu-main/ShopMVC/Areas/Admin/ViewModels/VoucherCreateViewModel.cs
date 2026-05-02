using System.ComponentModel.DataAnnotations;
using ShopMVC.Models;

namespace ShopMVC.Areas.Admin.ViewModels
{
    public class VoucherCreateViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui long nhap ma voucher")]
        [Display(Name = "Ma voucher")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ma voucher phai tu 3 den 50 ky tu.")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Ma voucher chi duoc chua chu, so, gach ngang va gach duoi.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap ten chuong trinh")]
        [Display(Name = "Ten/ghi chu voucher")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Ten voucher phai tu 3 den 200 ky tu.")]
        public string Ten { get; set; } = string.Empty;

        [Display(Name = "% giam")]
        [Range(0, 100, ErrorMessage = "% giam tu 0 den 100")]
        public double? PhanTramGiam { get; set; }

        [Display(Name = "Giam truc tiep (d)")]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giam truc tiep phai lon hon hoac bang 0.")]
        public decimal? GiamTrucTiep { get; set; }

        [Display(Name = "Giam toi da (d)")]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giam toi da phai lon hon hoac bang 0.")]
        public decimal? GiamToiDa { get; set; }

        [Required(ErrorMessage = "Chon ngay bat dau")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngay bat dau")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Chon ngay het han")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngay het han")]
        public DateTime NgayHetHan { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Luot dung toi da")]
        [Range(0, int.MaxValue, ErrorMessage = "Luot dung toi da khong hop le.")]
        public int SoLanSuDungToiDa { get; set; } = 100;

        [Display(Name = "Kich hoat")]
        public bool IsActive { get; set; } = true;

        public List<ThuongHieu> AvailableBrands { get; set; } = new();
        public List<DanhMuc> AvailableCategories { get; set; } = new();
        public List<int> SelectedBrandIds { get; set; } = new();
        public List<int> SelectedCategoryIds { get; set; } = new();

        [Display(Name = "La chuong trinh Flash Sale")]
        public bool IsFlashSale { get; set; }

        public byte[]? RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NgayHetHan < NgayBatDau)
            {
                yield return new ValidationResult(
                    "Ngay het han phai lon hon hoac bang ngay bat dau.",
                    new[] { nameof(NgayHetHan) });
            }

            var hasPercentDiscount = PhanTramGiam.GetValueOrDefault() > 0;
            var hasFixedDiscount = GiamTrucTiep.GetValueOrDefault() > 0;
            if (!hasPercentDiscount && !hasFixedDiscount)
            {
                yield return new ValidationResult(
                    "Phai nhap giam theo % hoac giam truc tiep.",
                    new[] { nameof(PhanTramGiam), nameof(GiamTrucTiep) });
            }

            if (GiamToiDa.HasValue && GiamTrucTiep.HasValue && GiamToiDa.Value > 0 && GiamToiDa.Value < GiamTrucTiep.Value)
            {
                yield return new ValidationResult(
                    "Giam toi da khong duoc nho hon muc giam truc tiep.",
                    new[] { nameof(GiamToiDa) });
            }
        }
    }
}
