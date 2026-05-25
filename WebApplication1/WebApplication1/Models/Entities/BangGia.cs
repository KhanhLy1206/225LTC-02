using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("BangGia")]
    public class BangGia
    {
        [Key]
        public int ID { get; set; }

        public int IDBaiXe { get; set; }

        public int IDLoaiXe { get; set; }

        [Required]
        [StringLength(100)]
        public string TenBangGia { get; set; } = null!;

        public decimal GiaTheoGio { get; set; }

        public decimal GiaQuaDem { get; set; }

        public decimal GiaTheoThang { get; set; }

        public decimal GiaDatCho { get; set; }

        public bool TrangThai { get; set; }

        // Navigation properties
        [ForeignKey("IDBaiXe")]
        public virtual BaiXe? BaiXe { get; set; }

        [ForeignKey("IDLoaiXe")]
        public virtual LoaiXe? LoaiXe { get; set; }
    }
}
