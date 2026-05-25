using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("XaPhuong")]
    public class XaPhuong
    {
        [Key]
        [StringLength(20)]
        public string MaXa { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string MaHuyen { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TenXa { get; set; } = null!;

        [ForeignKey("MaHuyen")]
        public QuanHuyen? QuanHuyen { get; set; }
    }
}
