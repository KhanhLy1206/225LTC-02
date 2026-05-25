using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication1.Validation
{
    public class VietnamesePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var phone = value as string;
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ValidationResult.Success;
            }

            // Regex checking Vietnamese phone format: 10 digits, starts with 03, 05, 07, 08, or 09
            var regex = new Regex(@"^(0[35789])[0-9]{8}$");
            if (!regex.IsMatch(phone))
            {
                return new ValidationResult("Số điện thoại không đúng định dạng Việt Nam (phải gồm 10 chữ số và bắt đầu bằng 03, 05, 07, 08 hoặc 09).");
            }

            return ValidationResult.Success;
        }
    }
}
