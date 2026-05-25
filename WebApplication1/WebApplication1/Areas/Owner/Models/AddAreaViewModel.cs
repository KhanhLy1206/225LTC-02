using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Areas.Owner.Models
{
    public class AddAreaViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bãi đỗ xe.")]
        public int LotId { get; set; }

        [Required(ErrorMessage = "Tên khu vực không được để trống.")]
        [StringLength(5, ErrorMessage = "Tên khu vực không vượt quá 5 ký tự.")]
        public string AreaName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại xe.")]
        public string VehicleType { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng chỗ đỗ xe không được để trống.")]
        [Range(1, 100, ErrorMessage = "Số lượng chỗ đỗ xe phải từ 1 đến 100.")]
        public int SpotCount { get; set; }
    }
}
