using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ThanhToan")]
    public class ThanhToan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDHoaDon { get; set; }

        [Required]
        [StringLength(50)]
        public string PhuongThuc { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? MaGiaoDich { get; set; }

        [ForeignKey("IDHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;
    }
}
