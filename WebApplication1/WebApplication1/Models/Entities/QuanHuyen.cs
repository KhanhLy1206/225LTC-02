using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("QuanHuyen")]
    public class QuanHuyen
    {
        [Key]
        [StringLength(20)]
        public string MaHuyen { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string MaTinh { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TenHuyen { get; set; } = null!;

        [ForeignKey("MaTinh")]
        public TinhThanh? TinhThanh { get; set; }

        public virtual ICollection<XaPhuong> XaPhuongs { get; set; } = new List<XaPhuong>();
    }
}
