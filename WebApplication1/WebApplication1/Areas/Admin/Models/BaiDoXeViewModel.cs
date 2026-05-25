using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Models
{
    public class BaiXeStatItem
    {
        public int IDBaiXe { get; set; }
        public string TenBai { get; set; } = null!;
        public string TenChuBai { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public int SucChua { get; set; }
        public int SoDangDung { get; set; }
        public int TongChoDo { get; set; }
        public int PhanTramLapDay => SucChua > 0 ? (int)((double)SoDangDung / SucChua * 100) : 0;
        public decimal DoanhThuThang { get; set; }
        public int TongLuotThang { get; set; }
        public double DiemDanhGia { get; set; }
        public int SoDanhGia { get; set; }
        public bool BaiDangKhoa { get; set; }
    }

    public class BaiDoXeIndexViewModel
    {
        public int TongBai { get; set; }
        public int TongHoatDong { get; set; }
        public int TongChoDuyet { get; set; }
        public int TongTamDong { get; set; }
        public int TongBaoTri { get; set; }
        public List<BaiXe> DonChoDuyet { get; set; } = new();
        public List<BaiXeStatItem> BaiHoatDong { get; set; } = new();
        public List<BaiXeStatItem> BaiTamDong { get; set; } = new();
        public List<BaiXeStatItem> BaiBaoTri { get; set; } = new();
        public string? Search { get; set; }
        public string Tab { get; set; } = "choduyet";
    }

    public class ChiTietBaiXeViewModel
    {
        public BaiXe BaiXe { get; set; } = null!;
        public int TongLuotDo { get; set; }
        public int LuotDoThang { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuTong { get; set; }
        public int PhanTramLapDay { get; set; }
        public int TongKhieuNai { get; set; }
        public int KhieuNaiCho { get; set; }
        public List<KhuVucChiTietItem> KhuVucs { get; set; } = new();
        public List<BangGia> BangGias { get; set; } = new();
        public List<DatChoChiTietItem> LichSuDatCho { get; set; } = new();
        public List<string> NhanNgay { get; set; } = new();
        public List<decimal> DoanhThuNgay { get; set; } = new();
        public List<int> LuotDoNgay { get; set; } = new();
        public int TongHoanThanh { get; set; }
        public int TongHuy { get; set; }
        public int TongDatCho { get; set; }
        public int PhanTramHoanThanh => TongDatCho > 0 ? (int)((double)TongHoanThanh / TongDatCho * 100) : 0;
        public int PhanTramHuy => TongDatCho > 0 ? (int)((double)TongHuy / TongDatCho * 100) : 0;
        public List<KhieuNai> KhieuNais { get; set; } = new();
    }

    public class KhuVucChiTietItem
    {
        public KhuVuc KhuVuc { get; set; } = null!;
        public int SoTrong { get; set; }
        public int SoDaDat { get; set; }
        public int SoDangDo { get; set; }
        public int SoBaoTri { get; set; }
        public int PhanTramDay => KhuVuc.SucChua > 0
            ? (int)((double)(SoDaDat + SoDangDo) / KhuVuc.SucChua * 100) : 0;
    }

    public class DatChoChiTietItem
    {
        public int ID { get; set; }
        public string BienSoXe { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string TenChoDau { get; set; } = null!;
        public string TenKhuVuc { get; set; } = null!;
        public DateTime TgianBatDau { get; set; }
        public DateTime TgianKetThuc { get; set; }
        public string TrangThai { get; set; } = null!;
        public decimal? TongTien { get; set; }
    }
}
