using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDPhienChat { get; set; }

        [Required]
        public int IDTaiKhoanGui { get; set; }

        [Required]
        public string NoiDung { get; set; } = null!;

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public bool DaDoc { get; set; }

        [ForeignKey("IDPhienChat")]
        public virtual PhienChat PhienChat { get; set; } = null!;

        [ForeignKey("IDTaiKhoanGui")]
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
