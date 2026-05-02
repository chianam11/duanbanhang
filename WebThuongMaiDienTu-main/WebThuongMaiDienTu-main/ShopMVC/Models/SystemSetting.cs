using System.ComponentModel.DataAnnotations;

namespace ShopMVC.Models
{
    public class SystemSetting
    {
        public const string ShopNameKey = "ShopName";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SettingValue { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
