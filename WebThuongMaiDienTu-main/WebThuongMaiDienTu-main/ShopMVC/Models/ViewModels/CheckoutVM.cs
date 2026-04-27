using System.ComponentModel.DataAnnotations;
using ShopMVC.Validations;

namespace ShopMVC.Models.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 200 ký tự.")]
        public string HoTenNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Số điện thoại phải từ 10 đến 20 ký tự.")]
        [VietnamesePhone]
        public string DienThoaiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Địa chỉ phải từ 10 đến 500 ký tự.")]
        public string DiaChiNhan { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự.")]
        public string? GhiChu { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Phí vận chuyển không hợp lệ.")]
        public decimal PhiVanChuyen { get; set; } = 30000;

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Tiền giảm không hợp lệ.")]
        public decimal TienGiam { get; set; } = 0;

        public List<GioHangItem> Gio { get; set; } = new();

        public decimal TamTinh => Gio.Sum(x => x.ThanhTien);
        public decimal TongThanhToan => TamTinh + PhiVanChuyen - TienGiam;

        [StringLength(50, ErrorMessage = "Mã voucher tối đa 50 ký tự.")]
        public string? VoucherCode { get; set; }
    }
}
