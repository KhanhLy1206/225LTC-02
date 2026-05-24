using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
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
            var vm = new DashboardViewModel
            {
                TongKhachHang    = await _db.KhachHangs.CountAsync(),
                TongChuBai       = await _db.ChuBaiXes.CountAsync(),
                TongBaiHoatDong  = await _db.BaiXes.CountAsync(b => b.TrangThai == "Hoạt động"),
                TongKhieuNaiCho  = await _db.KhieuNais.CountAsync(k => k.TrangThai == "Chờ xử lý"),

                DoanhThuThang    = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán"
                             && h.NgayTao.Month == DateTime.Now.Month
                             && h.NgayTao.Year  == DateTime.Now.Year)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0,

                HoaHongThang     = await _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán"
                             && h.NgayTao.Month == DateTime.Now.Month
                             && h.NgayTao.Year  == DateTime.Now.Year)
                    .SumAsync(h => (decimal?)h.TienChietKhauAdmin) ?? 0,

                LuotDoHomNay     = await _db.DatChos
                    .CountAsync(d => d.NgayDat.Date == DateTime.Today),

                DangDoHienTai    = await _db.DatChos
                    .CountAsync(d => d.TrangThai == "Đang đỗ"),

                DaDatCho         = await _db.DatChos
                    .CountAsync(d => d.TrangThai == "Đã đặt"),

                DonChoduyet      = await _db.DangKyBaiXes
                    .Where(d => d.TrangThai == "Chờ duyệt")
                    .Include(d => d.XaPhuong).ThenInclude(x => x.QuanHuyen).ThenInclude(q => q.TinhThanh)
                    .OrderByDescending(d => d.NgayGui)
                    .Take(5)
                    .ToListAsync(),

                HoatDongGanDay   = await _db.DatChos
                    .Include(d => d.KhachHang)
                    .OrderByDescending(d => d.NgayDat)
                    .Take(5)
                    .ToListAsync()
            };

            // Tổng chỗ đỗ
            vm.TongChoDo = await _db.ChoDauXes.CountAsync();
            vm.ChoDangDung = await _db.ChoDauXes
                .CountAsync(c => c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt");

            return View(vm);
        }
    }
}
