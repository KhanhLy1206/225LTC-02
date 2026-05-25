using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("DangKyBaiXe")]
    public class DangKyBaiXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string TenChuBai { get; set; } = null!;

        [Required]
        [StringLength(11)]
        public string SDT { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string CCCD { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TenBai { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string MaXa { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string DiaChiChiTiet { get; set; } = null!;

        [Required]
        public int SucChua { get; set; }

        [Required]
        [StringLength(255)]
        public string HinhAnh { get; set; } = null!;

        [StringLength(255)]
        public string? GiayPhepKinhDoanh { get; set; }

        [Required]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ duyệt";

        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }
    }
}
