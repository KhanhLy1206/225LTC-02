using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TongKhachHang { get; set; }
        public int TongChuBai { get; set; }
        public int TongTaiKhoan => TongKhachHang + TongChuBai;
        public int TongBaiHoatDong { get; set; }
        public int TongBaiTamDong { get; set; }
        public int TongBaiBaoTri { get; set; }
        public int TongKhieuNaiCho { get; set; }
        public int TongGiaoDich { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal HoaHongThang { get; set; }
        public int LuotDoHomNay { get; set; }
        public int DangDoHienTai { get; set; }
        public int DaDatCho { get; set; }
        public int HoanThanh { get; set; }
        public int TongChoDo { get; set; }
        public int ChoDangDung { get; set; }
        public int PhanTramDay => TongChoDo > 0 ? (int)((double)ChoDangDung / TongChoDo * 100) : 0;
        public List<string> NhanNgay { get; set; } = new();
        public List<decimal> DoanhThuNgay { get; set; } = new();
        public List<int> LuotDoNgay { get; set; } = new();
        public List<TopBaiItem> TopBaiXe { get; set; } = new();
        public List<BaiXeTinhTrang> DanhSachBai { get; set; } = new();
        public List<HoatDongItem> HoatDongGanDay { get; set; } = new();
        public List<BaiXe> DonChoduyet { get; set; } = new();
    }

    public class TopBaiItem
    {
        public string TenBai { get; set; } = null!;
        public string TenChuBai { get; set; } = null!;
        public int SoLuot { get; set; }
        public decimal DoanhThu { get; set; }
        public int PhanTramDay { get; set; }
    }

    public class BaiXeTinhTrang
    {
        public string TenBai { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public int SucChua { get; set; }
        public int DangDung { get; set; }
        public int PhanTramDay => SucChua > 0 ? (int)((double)DangDung / SucChua * 100) : 0;
    }

    public class HoatDongItem
    {
        public string Icon { get; set; } = null!;
        public string MauNen { get; set; } = null!;
        public string MauChu { get; set; } = null!;
        public string TieuDe { get; set; } = null!;
        public string MoTa { get; set; } = null!;
        public string ThoiGian { get; set; } = null!;
    }
}
