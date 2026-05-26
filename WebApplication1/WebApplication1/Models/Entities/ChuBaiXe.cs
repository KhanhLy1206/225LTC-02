using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ChuBaiXe")]
    public class ChuBaiXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string TenChuBai { get; set; } = null!;

        [Required]
        [StringLength(11)]
        public string SDT { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? CCCD { get; set; }

        [StringLength(20)]
        public string? MaXa { get; set; }

        [StringLength(255)]
        public string? DiaChiChiTiet { get; set; }

        // Navigation properties
        [ForeignKey("IDTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }

        public virtual ICollection<BaiXe> BaiXes { get; set; } = new List<BaiXe>();
    }
}
