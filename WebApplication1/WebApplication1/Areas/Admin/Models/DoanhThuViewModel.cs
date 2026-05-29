using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Models
{
    public class DoanhThuViewModel
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongHoaHong { get; set; }
        public decimal TongChuBaiNhan { get; set; }
        public int TongGiaoDich { get; set; }
        public int GiaoDichDaThanhToan { get; set; }
        public int GiaoDichChuaThanhToan { get; set; }
        public int GiaoDichHoanTien { get; set; }
        public decimal DoanhThuThangTruoc { get; set; }
        public decimal PhanTramTangTruong =>
            DoanhThuThangTruoc > 0
                ? Math.Round((TongDoanhThu - DoanhThuThangTruoc) / DoanhThuThangTruoc * 100, 1)
                : 0;
        public Dictionary<int, decimal> DoanhThuTheoNgay { get; set; } = new();
        public Dictionary<int, int> LuotDoTheoNgay { get; set; } = new();
        public List<decimal> DoanhThuTheoThang { get; set; } = new();
        public List<TopBaiXeItem> TopBaiXe { get; set; } = new();
        public List<PhanTichLoaiXe> PhanTichLoaiXes { get; set; } = new();
        public List<HoaDon> HoaDons { get; set; } = new();
    }

    public class TopBaiXeItem
    {
        public int IDBaiXe { get; set; }
        public string TenBai { get; set; } = null!;
        public string TenChuBai { get; set; } = null!;
        public int SoLuot { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal HoaHong { get; set; }
        public int PhanTram { get; set; }
    }

    public class PhanTichLoaiXe
    {
        public string TenLoaiXe { get; set; } = null!;
        public int SoLuot { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
