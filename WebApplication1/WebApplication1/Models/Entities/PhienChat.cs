using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("PhienChat")]
    public class PhienChat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDKhachHang { get; set; }

        [Required]
        public int IDChuBai { get; set; }

        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [ForeignKey("IDKhachHang")]
        public virtual KhachHang KhachHang { get; set; } = null!;

        [ForeignKey("IDChuBai")]
        public virtual ChuBaiXe ChuBaiXe { get; set; } = null!;

        public virtual ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
    }
}
