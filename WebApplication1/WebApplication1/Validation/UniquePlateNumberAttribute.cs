using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.DataContext;

namespace WebApplication1.Validation
{
    public class UniquePlateNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var plateNumber = value.ToString();
            var context = (AppDbContext?)validationContext.GetService(typeof(AppDbContext));

            if (context != null)
            {
                var exists = context.Xes.Any(x => x.BienSoXe == plateNumber);
                if (exists)
                {
                    return new ValidationResult("Biển số xe này đã tồn tại trong hệ thống.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
