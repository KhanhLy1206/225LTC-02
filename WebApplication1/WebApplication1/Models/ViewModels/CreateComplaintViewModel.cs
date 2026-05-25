using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModels
{
    public class CreateComplaintViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn đơn đặt chỗ.")]
        public int IDDatCho { get; set; }

        [Required(ErrorMessage = "Tiêu đề khiếu nại không được để trống.")]
        [StringLength(150, ErrorMessage = "Tiêu đề không vượt quá 150 ký tự.")]
        public string TieuDe { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung chi tiết không được để trống.")]
        public string NoiDung { get; set; } = null!;
    }
}
