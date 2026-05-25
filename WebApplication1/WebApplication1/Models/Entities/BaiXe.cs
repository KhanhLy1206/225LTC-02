using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("BaiXe")]
    public class BaiXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã chủ bãi là bắt buộc.")]
        public int IDChuBai { get; set; }

        public int? IDDangKy { get; set; }

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

        [Required(ErrorMessage = "Phần trăm chiết khấu không được để trống.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.00, 100.00, ErrorMessage = "Phần trăm chiết khấu phải từ 0 đến 100.")]
        [Display(Name = "Chiết khấu (%)")]
        public decimal PhanTramChietKhau { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }

        // Navigation properties
        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }
    }
}
