using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("LogDieuKhienBarrier")]
    public class LogDieuKhienBarrier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDDatCho { get; set; }

        [Required]
        public int IDTaiKhoan { get; set; }

        [Required]
        public DateTime ThoiGianLệnh { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string HanhDong { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string KetQua { get; set; } = null!;

        [StringLength(255)]
        public string? GhiChu { get; set; }

        [ForeignKey("IDDatCho")]
        public DatCho? DatCho { get; set; }

        [ForeignKey("IDTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}
