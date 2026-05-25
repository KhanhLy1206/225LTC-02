using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Xe")]
    public class Xe
    {
        [Key]
        [StringLength(20)]
        public string BienSoXe { get; set; } = null!;

        [Required]
        public int IDLoaiXe { get; set; }

        [StringLength(100)]
        public string? TenXe { get; set; }

        [StringLength(50)]
        public string? Hang { get; set; }

        [StringLength(50)]
        public string? MauSac { get; set; }

        [ForeignKey("IDLoaiXe")]
        public LoaiXe? LoaiXe { get; set; }
    }
}
