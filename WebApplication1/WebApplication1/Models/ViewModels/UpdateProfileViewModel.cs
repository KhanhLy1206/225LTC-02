using System.ComponentModel.DataAnnotations;
using WebApplication1.Validation;

namespace WebApplication1.Models.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên không vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [VietnamesePhone(ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam.")]
        public string SDT { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số CCCD không được để trống.")]
        [RegularExpression(@"^[0-9]{9}$|^[0-9]{12}$", ErrorMessage = "Số CCCD phải có 9 hoặc 12 số.")]
        public string CCCD { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Số bằng lái không vượt quá 20 ký tự.")]
        public string? BangLaiXe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Xã/Phường.")]
        [StringLength(20)]
        public string MaXa { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống.")]
        [StringLength(255, ErrorMessage = "Địa chỉ chi tiết không vượt quá 255 ký tự.")]
        public string DiaChiChiTiet { get; set; } = null!;
    }
}
