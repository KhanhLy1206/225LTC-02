using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDDatCho { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TienChietKhauAdmin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TienChuBaiNhan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        [ForeignKey("IDDatCho")]
        public virtual DatCho DatCho { get; set; } = null!;

        public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
    }
}
