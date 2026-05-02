using System.ComponentModel.DataAnnotations;

namespace ShopMVC.Areas.Admin.ViewModels
{
    public class ShopSettingsViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên shop.")]
        [StringLength(120, ErrorMessage = "Tên shop tối đa 120 ký tự.")]
        [Display(Name = "Tên shop")]
        public string ShopName { get; set; } = string.Empty;
    }
}
