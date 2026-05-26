using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("TinhThanh")]
    public class TinhThanh
    {
        [Key]
        [StringLength(20)]
        public string MaTinh { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TenTinh { get; set; } = null!;

        public virtual ICollection<QuanHuyen> QuanHuyens { get; set; } = new List<QuanHuyen>();
    }
}
