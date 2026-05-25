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

        // Navigation property
        public ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}
