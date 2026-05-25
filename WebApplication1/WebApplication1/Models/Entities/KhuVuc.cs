using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("KhuVuc")]
    public class KhuVuc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDBaiXe { get; set; }

        [Required]
        public int IDLoaiXe { get; set; }

        [Required]
        [StringLength(50)]
        public string TenKhuVuc { get; set; } = null!;

        [Required]
        public int SucChua { get; set; }

        [ForeignKey("IDBaiXe")]
        public BaiXe? BaiXe { get; set; }

        [ForeignKey("IDLoaiXe")]
        public LoaiXe? LoaiXe { get; set; }
    }
}
