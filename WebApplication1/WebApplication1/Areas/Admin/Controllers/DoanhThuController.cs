using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Areas.Admin.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoanhThuController : Controller
    {
        private readonly AppDbContext _db;
        public DoanhThuController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            thang ??= DateTime.Now.Month;
            nam   ??= DateTime.Now.Year;

            // ── Hóa đơn tháng hiện tại ──────────────────────────────────────
            var hoaDons = await _db.HoaDons
                .Include(h => h.DatCho).ThenInclude(d => d.KhachHang)
                .Include(h => h.DatCho).ThenInclude(d => d.ChoDauXe)
                    .ThenInclude(c => c.KhuVuc).ThenInclude(k => k.BaiXe)
                    .ThenInclude(b => b.ChuBaiXe)
                .Where(h => h.NgayTao.Month == thang && h.NgayTao.Year == nam)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            var daDanhToan = hoaDons.Where(h => h.TrangThai == "Đã thanh toán").ToList();

            // ── Tháng trước ─────────────────────────────────────────────────
            var thangTruoc = thang == 1 ? 12 : thang.Value - 1;
            var namTruoc   = thang == 1 ? nam.Value - 1 : nam.Value;
            var dtThangTruoc = await _db.HoaDons
                .Where(h => h.TrangThai == "Đã thanh toán"
                         && h.NgayTao.Month == thangTruoc
                         && h.NgayTao.Year  == namTruoc)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            // ── Doanh thu 12 tháng trong năm ────────────────────────────────
            var dtTheoThang = new List<decimal>();
            for (int m = 1; m <= 12; m++)
            {
                var dt = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán"
                             && h.NgayTao.Month == m
                             && h.NgayTao.Year  == nam)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0;
                dtTheoThang.Add(Math.Round(dt / 1000, 0));
            }

            // ── Lượt đỗ theo ngày ───────────────────────────────────────────
            var luotTheoNgay = await _db.DatChos
                .Where(d => d.NgayDat.Month == thang && d.NgayDat.Year == nam)
                .GroupBy(d => d.NgayDat.Day)
                .Select(g => new { Ngay = g.Key, SoLuot = g.Count() })
                .ToListAsync();

            // ── Top bãi xe ──────────────────────────────────────────────────
            var topRaw = daDanhToan
                .GroupBy(h => h.DatCho?.ChoDauXe?.KhuVuc?.BaiXe)
                .Where(g => g.Key != null)
                .Select(g => new TopBaiXeItem
                {
                    IDBaiXe   = g.Key!.ID,
                    TenBai    = g.Key.TenBai,
                    TenChuBai = g.Key.ChuBaiXe?.TenChuBai ?? "—",
                    SoLuot    = g.Count(),
                    DoanhThu  = g.Sum(h => h.TongTien),
                    HoaHong   = g.Sum(h => h.TienChietKhauAdmin)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(5)
                .ToList();

            var maxDT = topRaw.Any() ? topRaw.Max(x => x.DoanhThu) : 1;
            foreach (var t in topRaw)
                t.PhanTram = maxDT > 0 ? (int)(t.DoanhThu / maxDT * 100) : 0;

            // ── Phân tích loại xe ───────────────────────────────────────────
            var phanTichLoai = await _db.DatChos
                .Include(d => d.ChoDauXe).ThenInclude(c => c.KhuVuc).ThenInclude(k => k.LoaiXe)
                .Include(d => d.HoaDon)
                .Where(d => d.NgayDat.Month == thang && d.NgayDat.Year == nam
                         && d.HoaDon != null && d.HoaDon.TrangThai == "Đã thanh toán")
                .ToListAsync();

            var phanTich = phanTichLoai
                .GroupBy(d => d.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? "Khác")
                .Select(g => new PhanTichLoaiXe
                {
                    TenLoaiXe = g.Key,
                    SoLuot    = g.Count(),
                    DoanhThu  = g.Sum(d => d.HoaDon?.TongTien ?? 0)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            var vm = new DoanhThuViewModel
            {
                Thang                  = thang.Value,
                Nam                    = nam.Value,
                TongDoanhThu           = daDanhToan.Sum(h => h.TongTien),
                TongHoaHong            = daDanhToan.Sum(h => h.TienChietKhauAdmin),
                TongChuBaiNhan         = daDanhToan.Sum(h => h.TienChuBaiNhan),
                TongGiaoDich           = hoaDons.Count,
                GiaoDichDaThanhToan    = hoaDons.Count(h => h.TrangThai == "Đã thanh toán"),
                GiaoDichChuaThanhToan  = hoaDons.Count(h => h.TrangThai == "Chưa thanh toán"),
                GiaoDichHoanTien       = hoaDons.Count(h => h.TrangThai == "Đã hoàn tiền"),
                DoanhThuThangTruoc     = dtThangTruoc,
                DoanhThuTheoNgay       = daDanhToan
                    .GroupBy(h => h.NgayTao.Day)
                    .ToDictionary(g => g.Key, g => g.Sum(h => h.TongTien)),
                LuotDoTheoNgay         = luotTheoNgay.ToDictionary(x => x.Ngay, x => x.SoLuot),
                DoanhThuTheoThang      = dtTheoThang,
                TopBaiXe               = topRaw,
                PhanTichLoaiXes        = phanTich,
                HoaDons                = hoaDons
            };

            return View(vm);
        }
    }
}
