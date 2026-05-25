using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Areas.Owner.Models
{
    public class BaiXeViewModel
    {
        [Required(ErrorMessage = "Tên bãi xe không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên bãi xe không được quá 100 ký tự.")]
        [Display(Name = "Tên bãi xe")]
        public string TenBai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn Phường / Xã.")]
        [StringLength(20)]
        [Display(Name = "Mã xã/phường")]
        public string MaXa { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống.")]
        [StringLength(255, ErrorMessage = "Địa chỉ chi tiết không được quá 255 ký tự.")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string DiaChiChiTiet { get; set; } = null!;

        [Required(ErrorMessage = "Sức chứa không được để trống.")]
        [Range(1, 10000, ErrorMessage = "Sức chứa phải từ 1 đến 10,000.")]
        [Display(Name = "Sức chứa")]
        public int SucChua { get; set; }

        [Display(Name = "Hình ảnh bãi xe")]
        public IFormFile? HinhAnhFile { get; set; }
    }
}
