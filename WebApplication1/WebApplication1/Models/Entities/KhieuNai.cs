using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("KhieuNai")]
    public class KhieuNai
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDKhachHang { get; set; }

        [Required]
        public int IDBaiXe { get; set; }

        public int? IDDatCho { get; set; }

        [Required]
        [StringLength(150)]
        public string TieuDe { get; set; } = null!;

        [Required]
        public string NoiDung { get; set; } = null!;

        [Required]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xử lý";

        public int? IDAdminXuLy { get; set; }

        public string? GhiChuAdmin { get; set; }

        [ForeignKey("IDKhachHang")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("IDBaiXe")]
        public BaiXe? BaiXe { get; set; }

        [ForeignKey("IDDatCho")]
        public DatCho? DatCho { get; set; }
    }
}
