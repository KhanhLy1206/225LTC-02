using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("DanhGiaBinhLuan")]
    public class DanhGiaBinhLuan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int IDKhachHang { get; set; }

        [Required]
        public int IDBaiXe { get; set; }

        public int? IDDatCho { get; set; }

        [Required]
        public int DiemDanhGia { get; set; }

        public string? NoiDungBinhLuan { get; set; }

        [Required]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("IDKhachHang")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("IDBaiXe")]
        public BaiXe? BaiXe { get; set; }

        [ForeignKey("IDDatCho")]
        public DatCho? DatCho { get; set; }
    }
}
