using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("BaiXe")]
    public class BaiXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDChuBai { get; set; }

        public int? IDDangKy { get; set; }

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
        [Column(TypeName = "decimal(5,2)")]
        public decimal PhanTramChietKhau { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Hoạt động";

        [StringLength(255)]
        public string? HinhAnh { get; set; }
    }
}
