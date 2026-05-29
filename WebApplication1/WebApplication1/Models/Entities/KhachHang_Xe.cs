using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("KhachHang_Xe")]
    public class KhachHang_Xe
    {
        [Required]
        public int IDKhachHang { get; set; }

        [Required]
        [StringLength(20)]
        public string IDXe { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LoaiSoHuu { get; set; } = "Cá nhân";

        [ForeignKey("IDKhachHang")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("IDXe")]
        public Xe? Xe { get; set; }
    }
}
