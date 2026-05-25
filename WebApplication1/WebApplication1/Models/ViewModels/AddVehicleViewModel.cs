using System.ComponentModel.DataAnnotations;
using WebApplication1.Validation;

namespace WebApplication1.Models.ViewModels
{
    public class AddVehicleViewModel
    {
        [Required(ErrorMessage = "Biển số xe không được để trống.")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}(\.[0-9]{2})?$", ErrorMessage = "Biển số xe không đúng định dạng Việt Nam (VD: 51F-123.45 hoặc 51F1-123.45).")]
        [UniquePlateNumber(ErrorMessage = "Biển số xe này đã được đăng ký bởi phương tiện khác.")]
        public string BienSoXe { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại phương tiện.")]
        public int IDLoaiXe { get; set; }

        [Required(ErrorMessage = "Tên xe không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên xe không vượt quá 100 ký tự.")]
        public string TenXe { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Hãng sản xuất không vượt quá 50 ký tự.")]
        public string? Hang { get; set; }

        [StringLength(50, ErrorMessage = "Màu sắc không vượt quá 50 ký tự.")]
        public string? MauSac { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sở hữu.")]
        [StringLength(50)]
        public string LoaiSoHuu { get; set; } = "Cá nhân";
    }
}
