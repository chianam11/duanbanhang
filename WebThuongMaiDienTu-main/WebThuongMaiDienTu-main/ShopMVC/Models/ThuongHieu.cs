using System.ComponentModel.DataAnnotations;

namespace ShopMVC.Models
{
    public class ThuongHieu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên thương hiệu phải từ 2 đến 200 ký tự.")]
        public string Ten { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? MoTa { get; set; }

        [StringLength(500, ErrorMessage = "Logo tối đa 500 ký tự.")]
        public string? LogoUrl { get; set; }

        [StringLength(200, ErrorMessage = "Slug tối đa 200 ký tự.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ được chứa chữ thường không dấu, số và dấu gạch ngang.")]
        public string? Slug { get; set; }

        public bool HienThi { get; set; } = true;

        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
