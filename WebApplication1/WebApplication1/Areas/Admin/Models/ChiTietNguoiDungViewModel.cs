using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Models
{
    public class ChiTietNguoiDungViewModel
    {
        public KhachHang KhachHang { get; set; } = null!;
        public int TongDatCho { get; set; }
        public int DatChoHoanThanh { get; set; }
        public int DatChoHuy { get; set; }
        public decimal TongChiTieu { get; set; }
        public int SoLanHuy { get; set; }
        public int SoKhieuNai { get; set; }
        public List<string> CanhBaoHanhVi { get; set; } = new();
        public List<KhieuNai> KhieuNais { get; set; } = new();
        public List<DatCho> LichSuDatCho { get; set; } = new();
    }
}
