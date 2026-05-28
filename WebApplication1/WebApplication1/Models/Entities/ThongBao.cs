using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>Tài khoản nhận thông báo</summary>
        [Required]
        public int IDTaiKhoan { get; set; }

        /// <summary>Tiêu đề ngắn</summary>
        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = null!;

        /// <summary>Nội dung chi tiết</summary>
        [Required]
        public string NoiDung { get; set; } = null!;

        /// <summary>Loại: DuyetBai, TuChoiBai, ...</summary>
        [StringLength(50)]
        public string LoaiThongBao { get; set; } = "info";

        public bool DaDoc { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        /// <summary>Link điều hướng khi click thông báo (tuỳ chọn)</summary>
        [StringLength(500)]
        public string? DuongDan { get; set; }

        // Navigation
        [ForeignKey("IDTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }
    }
}
