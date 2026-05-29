using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [StringLength(11)]
        public string? SDT { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? CCCD { get; set; }

        [StringLength(20)]
        public string? BangLaiXe { get; set; }

        [StringLength(20)]
        public string? MaXa { get; set; }

        [StringLength(255)]
        public string? DiaChiChiTiet { get; set; }

        [ForeignKey("IDTaiKhoan")]
        public TaiKhoan TaiKhoan { get; set; } = null!;

        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }

        public virtual ICollection<DatCho> DatChos { get; set; } = new List<DatCho>();
        public virtual ICollection<KhieuNai> KhieuNais { get; set; } = new List<KhieuNai>();
    }
}
