using System.ComponentModel.DataAnnotations;
using WebApplication1.Validation;

namespace WebApplication1.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tên đăng nhập phải từ 5 đến 50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới.")]
        [UniqueUsername] // Custom validation
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự.")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [VietnamesePhone] // Custom validation
        public string SDT { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự.")]
        public string Email { get; set; } = null!;
    }
}
