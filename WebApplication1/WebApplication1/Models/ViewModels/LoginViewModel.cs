using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc Email không được để trống.")]
        public string TenDangNhapHoacEmail { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
