using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Areas.Admin.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var now   = DateTime.Now;
            var today = DateTime.Today;

            var vm = new DashboardViewModel
            {
                // ── Stat cards ──────────────────────────────────
                TongKhachHang   = await _db.KhachHangs.CountAsync(),
                TongChuBai      = await _db.ChuBaiXes.CountAsync(),
                TongBaiHoatDong = await _db.BaiXes.CountAsync(b => b.TrangThai == "Hoạt động"),
                TongBaiTamDong  = await _db.BaiXes.CountAsync(b => b.TrangThai == "Tạm đóng"),
                TongBaiBaoTri   = await _db.BaiXes.CountAsync(b => b.TrangThai == "Bảo trì"),
                TongKhieuNaiCho = await _db.KhieuNais.CountAsync(k => k.TrangThai == "Chờ xử lý"),

                DoanhThuThang = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán"
                             && h.NgayTao.Month == now.Month
                             && h.NgayTao.Year  == now.Year)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0,

                HoaHongThang = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán"
                             && h.NgayTao.Month == now.Month
                             && h.NgayTao.Year  == now.Year)
                    .SumAsync(h => (decimal?)h.TienChietKhauAdmin) ?? 0,

                TongGiaoDich = await _db.HoaDons
                    .CountAsync(h => h.NgayTao.Month == now.Month && h.NgayTao.Year == now.Year),

                // ── Lượt đỗ hôm nay ─────────────────────────────
                LuotDoHomNay  = await _db.DatChos.CountAsync(d => d.NgayDat.Date == today),
                DangDoHienTai = await _db.DatChos.CountAsync(d => d.TrangThai == "Đang đỗ"),
                DaDatCho      = await _db.DatChos.CountAsync(d => d.TrangThai == "Đã đặt"),
                HoanThanh     = await _db.DatChos
                    .CountAsync(d => d.TrangThai == "Hoàn thành" && d.NgayDat.Date == today),

                // ── Chỗ đỗ ──────────────────────────────────────
                TongChoDo   = await _db.ChoDauXes.CountAsync(),
                ChoDangDung = await _db.ChoDauXes
                    .CountAsync(c => c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt"),

                // ── Đơn chờ duyệt ────────────────────────────────
                DonChoduyet = await _db.DangKyBaiXes
                    .Where(d => d.TrangThai == "Chờ duyệt")
                    .Include(d => d.XaPhuong)
                    .OrderByDescending(d => d.NgayGui)
                    .Take(3)
                    .ToListAsync()
            };

            // ── Biểu đồ 7 ngày gần nhất ─────────────────────────
            for (int i = 6; i >= 0; i--)
            {
                var ngay = today.AddDays(-i);
                vm.NhanNgay.Add(ngay.ToString("dd/MM"));

                var dt = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán" && h.NgayTao.Date == ngay)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0;
                vm.DoanhThuNgay.Add(Math.Round(dt / 1000, 0));

                var luot = await _db.DatChos.CountAsync(d => d.NgayDat.Date == ngay);
                vm.LuotDoNgay.Add(luot);
            }

            // ── Top 5 bãi xe doanh thu cao ───────────────────────
            var topRaw = await _db.HoaDons
                .Include(h => h.DatCho).ThenInclude(d => d.ChoDauXe)
                    .ThenInclude(c => c.KhuVuc).ThenInclude(k => k.BaiXe)
                    .ThenInclude(b => b.ChuBaiXe)
                .Where(h => h.TrangThai == "Đã thanh toán"
                         && h.NgayTao.Month == now.Month
                         && h.NgayTao.Year  == now.Year)
                .ToListAsync();

            vm.TopBaiXe = topRaw
                .GroupBy(h => h.DatCho?.ChoDauXe?.KhuVuc?.BaiXe)
                .Where(g => g.Key != null)
                .Select(g => new TopBaiItem
                {
                    TenBai    = g.Key!.TenBai,
                    TenChuBai = g.Key.ChuBaiXe?.TenChuBai ?? "—",
                    SoLuot    = g.Count(),
                    DoanhThu  = g.Sum(h => h.TongTien)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(5)
                .ToList();

            // Tính % so với top 1
            if (vm.TopBaiXe.Any())
            {
                var maxDT = vm.TopBaiXe.Max(x => x.DoanhThu);
                foreach (var t in vm.TopBaiXe)
                    t.PhanTramDay = maxDT > 0 ? (int)(t.DoanhThu / maxDT * 100) : 0;
            }

            // ── Tình trạng bãi đỗ xe ─────────────────────────────
            var baiList = await _db.BaiXes
                .Include(b => b.KhuVucs).ThenInclude(k => k.ChoDauXes)
                .OrderBy(b => b.TenBai)
                .Take(8)
                .ToListAsync();

            vm.DanhSachBai = baiList.Select(b => new BaiXeTinhTrang
            {
                TenBai    = b.TenBai,
                TrangThai = b.TrangThai,
                SucChua   = b.SucChua,
                DangDung  = b.KhuVucs
                    .SelectMany(k => k.ChoDauXes)
                    .Count(c => c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt")
            }).ToList();

            // ── Hoạt động gần đây ────────────────────────────────
            var datChos = await _db.DatChos
                .Include(d => d.KhachHang)
                .OrderByDescending(d => d.NgayDat)
                .Take(3)
                .ToListAsync();

            var khieuNais = await _db.KhieuNais
                .Include(k => k.KhachHang)
                .OrderByDescending(k => k.NgayGui)
                .Take(2)
                .ToListAsync();

            foreach (var dc in datChos)
                vm.HoatDongGanDay.Add(new HoatDongItem
                {
                    Icon     = "fa-car",
                    MauNen   = "#dbeafe",
                    MauChu   = "#2563eb",
                    TieuDe   = $"Đặt chỗ — {dc.BienSoXe}",
                    MoTa     = dc.KhachHang?.HoTen ?? "—",
                    ThoiGian = ThoiGianTuongDoi(dc.NgayDat)
                });

            foreach (var kn in khieuNais)
                vm.HoatDongGanDay.Add(new HoatDongItem
                {
                    Icon     = "fa-flag",
                    MauNen   = "#fee2e2",
                    MauChu   = "#dc2626",
                    TieuDe   = "Khiếu nại mới",
                    MoTa     = kn.KhachHang?.HoTen ?? "—",
                    ThoiGian = ThoiGianTuongDoi(kn.NgayGui)
                });

            vm.HoatDongGanDay = vm.HoatDongGanDay
                .OrderByDescending(h => h.ThoiGian).Take(6).ToList();

            return View(vm);
        }

        private static string ThoiGianTuongDoi(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1)  return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} giờ trước";
            return $"{(int)diff.TotalDays} ngày trước";
        }
    }
}
