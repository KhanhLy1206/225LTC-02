using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("LoaiXe")]
    public class LoaiXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string TenLoaiXe { get; set; } = null!;

        public virtual ICollection<KhuVuc> KhuVucs { get; set; } = new List<KhuVuc>();
        public virtual ICollection<BangGia> BangGias { get; set; } = new List<BangGia>();
    }
}
