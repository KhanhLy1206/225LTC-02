using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ChoDauXe")]
    public class ChoDauXe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDKhuVuc { get; set; }

        [Required]
        [StringLength(20)]
        public string TenChoDau { get; set; } = null!;

        [StringLength(50)]
        public string? KichThuoc { get; set; }

        [StringLength(50)]
        public string? MaSoKhoa { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThaiKhoa { get; set; } = "Đóng";

        [Required]
        [StringLength(50)]
        public string TrangThaiO { get; set; } = "Trống";

        [ForeignKey("IDKhuVuc")]
        public KhuVuc? KhuVuc { get; set; }

        public virtual ICollection<DatCho> DatChos { get; set; } = new List<DatCho>();
    }
}
