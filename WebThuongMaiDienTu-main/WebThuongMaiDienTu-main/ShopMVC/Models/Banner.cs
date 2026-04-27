using System.ComponentModel.DataAnnotations;

namespace ShopMVC.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên banner.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên banner phải từ 2 đến 200 ký tự.")]
        public string? TenBanner { get; set; } // Tên để quản lý cho dễ

        [StringLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự.")]
        public string? HinhAnh { get; set; }   // Lưu tên file ảnh (ví dụ: banner1.jpg)

        [Range(0, 100000, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 100000.")]
        public int ThuTu { get; set; }         // Để sắp xếp banner nào hiện trước

        public bool HienThi { get; set; }      // Ẩn/Hiện banner
    }
}
