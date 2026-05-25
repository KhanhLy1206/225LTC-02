using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("DatCho")]
    public class DatCho
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDKhachHang { get; set; }

        [Required]
        public int IDChoDau { get; set; }

        [Required]
        [StringLength(20)]
        public string BienSoXe { get; set; } = null!;

        [Required]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required]
        public DateTime TgianBatDau { get; set; }

        [Required]
        public DateTime TgianKetThuc { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TienCoc { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Đã đặt";

        [ForeignKey("IDKhachHang")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("IDChoDau")]
        public ChoDauXe? ChoDauXe { get; set; }
    }
}
