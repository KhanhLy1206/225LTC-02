using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDVaiTro { get; set; }

        [Required]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; } = null!;

        [StringLength(255)]
        public string? AnhDaiDien { get; set; }

        [Required]
        public bool TrangThai { get; set; } = true;

        [ForeignKey("IDVaiTro")]
        public VaiTro VaiTro { get; set; } = null!;

        public virtual KhachHang? KhachHang { get; set; }
        public virtual ChuBaiXe? ChuBaiXe { get; set; }
        public virtual Admin? Admin { get; set; }
        public virtual ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
        public virtual ICollection<LogDieuKhienBarrier> LogDieuKhienBarriers { get; set; } = new List<LogDieuKhienBarrier>();
    }
}
