using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.DataContext;

namespace WebApplication1.Validation
{
    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var username = value as string;
            if (string.IsNullOrWhiteSpace(username))
            {
                return ValidationResult.Success;
            }

            // Using Dependency Injection inside custom validation attribute
            var dbContext = (AppDbContext?)validationContext.GetService(typeof(AppDbContext));
            if (dbContext == null)
            {
                return ValidationResult.Success; // Fallback if DbContext service is missing
            }

            var exists = dbContext.TaiKhoans.Any(t => t.TenDangNhap.ToLower() == username.ToLower());
            if (exists)
            {
                return new ValidationResult("Tên đăng nhập này đã tồn tại trên hệ thống.");
            }

            return ValidationResult.Success;
        }
    }
}
