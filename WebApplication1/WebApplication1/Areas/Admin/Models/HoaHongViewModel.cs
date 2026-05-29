namespace WebApplication1.Areas.Admin.Models
{
    public class HoaHongBaiXeItem
    {
        public int IDBaiXe { get; set; }
        public string TenBai { get; set; } = null!;
        public string TenChuBai { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public decimal PhanTramHienTai { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal HoaHongThang { get; set; }
        public int SoGiaoDichThang { get; set; }

        // Phản hồi từ Owner
        public string? TrangThaiPhanHoi { get; set; }   // "PhanHoiHoaHong_ChapNhan" | "PhanHoiHoaHong_TuChoi" | null
        public string? NoiDungPhanHoi { get; set; }
        public int? IDThongBaoPhanHoi { get; set; }

        public bool CoPhanHoiMoi => TrangThaiPhanHoi != null;
        public bool OwnerChapNhan => TrangThaiPhanHoi == "PhanHoiHoaHong_ChapNhan";
        public bool OwnerTuChoi   => TrangThaiPhanHoi == "PhanHoiHoaHong_TuChoi";
    }

    public class HoaHongViewModel
    {
        // Thống kê tổng quan
        public decimal TyLeMacDinh { get; set; }
        public int TongBaiApDungMacDinh { get; set; }
        public int TongBaiTungChinhnh { get; set; }
        public decimal TongHoaHongThang { get; set; }
        public decimal TongDoanhThuThang { get; set; }
        public int SoPhanHoiChuaDoc { get; set; }

        // Danh sách bãi xe
        public List<HoaHongBaiXeItem> DanhSachBai { get; set; } = new();

        // Filter
        public string? Search { get; set; }
        public string? LocTrangThai { get; set; }

        public decimal TyLeMacDinhHeThong { get; set; } = 10;
    }
}
