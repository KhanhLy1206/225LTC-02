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

        [Required(ErrorMessage = "Diện tích bãi xe không được để trống.")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(10.00, 100000.00, ErrorMessage = "Diện tích phải từ 10 đến 100,000 m².")]
        [Display(Name = "Diện tích (m²)")]
        public decimal DienTich { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự.")]
        [Display(Name = "Hotline")]
        public string? SoDienThoai { get; set; }

        [StringLength(100, ErrorMessage = "Giờ hoạt động không được quá 100 ký tự.")]
        [Display(Name = "Giờ hoạt động")]
        public string? GioHoatDong { get; set; }

        [Required(ErrorMessage = "Phần trăm chiết khấu không được để trống.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.00, 100.00, ErrorMessage = "Phần trăm chiết khấu phải từ 0 đến 100.")]
        [Display(Name = "Chiết khấu (%)")]
        public decimal PhanTramChietKhau { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        [Required(ErrorMessage = "Hình ảnh bãi đỗ xe không được để trống.")]
        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; } = null!;

        [Required(ErrorMessage = "Giấy phép kinh doanh không được để trống.")]
        [StringLength(255)]
        [Display(Name = "Giấy phép kinh doanh")]
        public string GiayPhepKinhDoanh { get; set; } = null!;

        [Required(ErrorMessage = "Ngày gửi không được để trống.")]
        [Display(Name = "Ngày gửi")]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("IDChuBai")]
        public virtual ChuBaiXe? ChuBaiXe { get; set; }

        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }

        public virtual ICollection<KhuVuc> KhuVucs { get; set; } = new List<KhuVuc>();
        public virtual ICollection<BangGia> BangGias { get; set; } = new List<BangGia>();
        public virtual ICollection<KhieuNai> KhieuNais { get; set; } = new List<KhieuNai>();
        public virtual ICollection<DanhGiaBinhLuan> DanhGiaBinhLuans { get; set; } = new List<DanhGiaBinhLuan>();
    }
}
