using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ShopMVC.Validations
{
    public class VietnamesePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            string phone = value.ToString() ?? string.Empty;

            if (Regex.IsMatch(phone, @"^0\d{9}$"))
                return ValidationResult.Success;

            return new ValidationResult("Số điện thoại không hợp lệ.");
        }
    }

    public class ValidEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            string email = value.ToString() ?? string.Empty;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email
                    ? ValidationResult.Success
                    : new ValidationResult("Email không hợp lệ.");
            }
            catch
            {
                return new ValidationResult("Email không hợp lệ.");
            }
        }
    }

    public class ValidPriceAttribute : ValidationAttribute
    {
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = decimal.MaxValue;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (decimal.TryParse(value.ToString(), out decimal price))
            {
                if (price >= MinPrice && price <= MaxPrice)
                    return ValidationResult.Success;

                return new ValidationResult($"Giá phải nằm trong khoảng {MinPrice} - {MaxPrice}.");
            }

            return new ValidationResult("Giá không hợp lệ.");
        }
    }
}
