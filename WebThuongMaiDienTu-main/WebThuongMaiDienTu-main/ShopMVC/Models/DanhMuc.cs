using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopMVC.Models
{
    public class DanhMuc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2 đến 200 ký tự.")]
        public string Ten { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Slug tối đa 200 ký tự.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ được chứa chữ thường không dấu, số và dấu gạch ngang.")]
        public string? Slug { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? MoTa { get; set; }

        public int? DanhMucChaId { get; set; }
        [ForeignKey(nameof(DanhMucChaId))]
        public DanhMuc? DanhMucCha { get; set; }

        [Range(0, 100000, ErrorMessage = "Thứ tự phải từ 0 đến 100000.")]
        public int ThuTu { get; set; } = 0;
        public bool HienThi { get; set; } = true;
        [StringLength(500, ErrorMessage = "Đường dẫn icon tối đa 500 ký tự.")]
        public string? IconUrl { get; set; }

        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
