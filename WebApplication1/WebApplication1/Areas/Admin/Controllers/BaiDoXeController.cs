using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Areas.Admin.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BaiDoXeController : Controller
    {
        private readonly AppDbContext _db;
        public BaiDoXeController(AppDbContext db) => _db = db;

        // ── INDEX ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? search, string tab = "choduyet")
        {
            var now = DateTime.Now;

            // Query đơn chờ duyệt (BaiXe với TrangThai = 'Chờ duyệt')
            var donQuery = _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .Include(b => b.XaPhuong).ThenInclude(x => x!.QuanHuyen!).ThenInclude(q => q!.TinhThanh)
                .Where(b => b.TrangThai == "Chờ duyệt")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                donQuery = donQuery.Where(d => d.TenBai.Contains(search)
                    || (d.ChuBaiXe != null && d.ChuBaiXe.TenChuBai.Contains(search)));

            // Query bãi xe đã duyệt
            var baiQuery = _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .Include(b => b.XaPhuong).ThenInclude(x => x!.QuanHuyen!).ThenInclude(q => q!.TinhThanh)
                .Include(b => b.KhuVucs).ThenInclude(k => k.ChoDauXes)
                .Where(b => b.TrangThai != "Chờ duyệt")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                baiQuery = baiQuery.Where(b => b.TenBai.Contains(search)
                    || (b.ChuBaiXe != null && b.ChuBaiXe.TenChuBai.Contains(search)));

            var allBai = await baiQuery.OrderBy(b => b.TenBai).ToListAsync();

            // Lấy doanh thu tháng cho từng bãi
            var hoaDonThang = await _db.HoaDons
                .Include(h => h.DatCho).ThenInclude(d => d!.ChoDauXe!).ThenInclude(c => c!.KhuVuc)
                .Where(h => h.TrangThai == "Đã thanh toán"
                         && h.NgayTao.Month == now.Month
                         && h.NgayTao.Year == now.Year)
                .ToListAsync();

            var datChoThang = await _db.DatChos
                .Include(d => d.ChoDauXe).ThenInclude(c => c!.KhuVuc)
                .Where(d => d.NgayDat.Month == now.Month && d.NgayDat.Year == now.Year)
                .ToListAsync();

            var danhGias = await _db.DanhGiaBinhLuans.ToListAsync();

            Func<BaiXe, BaiXeStatItem> BuildStat = (BaiXe b) =>
            {
                var allCho = b.KhuVucs.SelectMany(k => k.ChoDauXes).ToList();
                var dangDung = allCho.Count(c => c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt");
                var dtThang = hoaDonThang
                    .Where(h => h.DatCho?.ChoDauXe?.KhuVuc?.IDBaiXe == b.ID)
                    .Sum(h => h.TongTien);
                var luotThang = datChoThang.Count(d => d.ChoDauXe?.KhuVuc?.IDBaiXe == b.ID);
                var dgBai = danhGias.Where(d => d.IDBaiXe == b.ID).ToList();
                return new BaiXeStatItem
                {
                    IDBaiXe       = b.ID,
                    TenBai        = b.TenBai,
                    TenChuBai     = b.ChuBaiXe?.TenChuBai ?? "—",
                    TrangThai     = b.TrangThai,
                    SucChua       = b.SucChua,
                    SoDangDung    = dangDung,
                    TongChoDo     = allCho.Count,
                    DoanhThuThang = dtThang,
                    TongLuotThang = luotThang,
                    DiemDanhGia   = dgBai.Any() ? dgBai.Average(d => d.DiemDanhGia) : 0,
                    SoDanhGia     = dgBai.Count,
                    BaiDangKhoa   = b.TrangThai != "Hoạt động"
                };
            };

            var vm = new BaiDoXeIndexViewModel
            {
                Search = search,
                Tab = tab,
                TongBai = allBai.Count,
                TongHoatDong = allBai.Count(b => b.TrangThai == "Hoạt động"),
                TongTamDong = allBai.Count(b => b.TrangThai == "Tạm đóng"),
                TongBaoTri = allBai.Count(b => b.TrangThai == "Bảo trì"),
                TongChoDuyet = await donQuery.CountAsync(),
                DonChoDuyet = await donQuery.OrderByDescending(d => d.NgayGui).ToListAsync(),
                BaiHoatDong = allBai.Where(b => b.TrangThai == "Hoạt động").Select(BuildStat).ToList(),
                BaiTamDong = allBai.Where(b => b.TrangThai == "Tạm đóng").Select(BuildStat).ToList(),
                BaiBaoTri = allBai.Where(b => b.TrangThai == "Bảo trì").Select(BuildStat).ToList(),
            };

            return View(vm);
        }

        // ── CHI TIẾT ─────────────────────────────────────────────────────────
        public async Task<IActionResult> ChiTiet(int id)
        {
            var baiXe = await _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .Include(b => b.XaPhuong).ThenInclude(x => x!.QuanHuyen!).ThenInclude(q => q!.TinhThanh)
                .Include(b => b.KhuVucs).ThenInclude(k => k.LoaiXe)
                .Include(b => b.KhuVucs).ThenInclude(k => k.ChoDauXes)
                .Include(b => b.BangGias).ThenInclude(bg => bg.LoaiXe)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (baiXe == null) return NotFound();

            var now = DateTime.Now;
            var today = DateTime.Today;

            var allDatCho = await _db.DatChos
                .Include(d => d.KhachHang)
                .Include(d => d.ChoDauXe).ThenInclude(c => c!.KhuVuc)
                .Include(d => d.HoaDon)
                .Where(d => d.ChoDauXe!.KhuVuc!.IDBaiXe == id)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            var allCho = baiXe.KhuVucs.SelectMany(k => k.ChoDauXes).ToList();
            var dangDung = allCho.Count(c => c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt");
            var hoaDonThang = allDatCho
                .Where(d => d.NgayDat.Month == now.Month && d.NgayDat.Year == now.Year && d.HoaDon != null)
                .Select(d => d.HoaDon!).ToList();

            var khuVucs = baiXe.KhuVucs.Select(k => new KhuVucChiTietItem
            {
                KhuVuc = k,
                SoTrong = k.ChoDauXes.Count(c => c.TrangThaiO == "Trống"),
                SoDaDat = k.ChoDauXes.Count(c => c.TrangThaiO == "Đã đặt"),
                SoDangDo = k.ChoDauXes.Count(c => c.TrangThaiO == "Đang đỗ"),
                SoBaoTri = k.ChoDauXes.Count(c => c.TrangThaiO == "Bảo trì"),
            }).ToList();

            var lichSu = allDatCho.Take(10).Select(d => new DatChoChiTietItem
            {
                ID = d.ID,
                BienSoXe = d.BienSoXe,
                TenKhachHang = d.KhachHang?.HoTen ?? "—",
                TenChoDau = d.ChoDauXe?.TenChoDau ?? "—",
                TenKhuVuc = d.ChoDauXe?.KhuVuc?.TenKhuVuc ?? "—",
                TgianBatDau = d.TgianBatDau,
                TgianKetThuc = d.TgianKetThuc,
                TrangThai = d.TrangThai,
                TongTien = d.HoaDon?.TongTien
            }).ToList();

            var nhanNgay = new List<string>();
            var dtNgay = new List<decimal>();
            var luotNgay = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var ngay = today.AddDays(-i);
                nhanNgay.Add(ngay.ToString("dd/MM"));
                var dt = allDatCho
                    .Where(d => d.NgayDat.Date == ngay && d.HoaDon?.TrangThai == "Đã thanh toán")
                    .Sum(d => d.HoaDon?.TongTien ?? 0);
                dtNgay.Add(Math.Round(dt / 1000, 0));
                luotNgay.Add(allDatCho.Count(d => d.NgayDat.Date == ngay));
            }

            var khieuNais = await _db.KhieuNais
                .Include(k => k.KhachHang)
                .Where(k => k.IDBaiXe == id)
                .OrderByDescending(k => k.NgayGui)
                .ToListAsync();

            var vm = new ChiTietBaiXeViewModel
            {
                BaiXe = baiXe,
                TongLuotDo = allDatCho.Count,
                LuotDoThang = allDatCho.Count(d => d.NgayDat.Month == now.Month && d.NgayDat.Year == now.Year),
                DoanhThuThang = hoaDonThang.Sum(h => h.TongTien),
                DoanhThuTong = allDatCho.Where(d => d.HoaDon?.TrangThai == "Đã thanh toán").Sum(d => d.HoaDon?.TongTien ?? 0),
                PhanTramLapDay = baiXe.SucChua > 0 ? (int)((double)dangDung / baiXe.SucChua * 100) : 0,
                TongKhieuNai = khieuNais.Count,
                KhieuNaiCho = khieuNais.Count(k => k.TrangThai == "Chờ xử lý"),
                KhuVucs = khuVucs,
                BangGias = baiXe.BangGias.ToList(),
                LichSuDatCho = lichSu,
                NhanNgay = nhanNgay,
                DoanhThuNgay = dtNgay,
                LuotDoNgay = luotNgay,
                TongHoanThanh = allDatCho.Count(d => d.TrangThai == "Hoàn thành"),
                TongHuy = allDatCho.Count(d => d.TrangThai == "Đã hủy"),
                TongDatCho = allDatCho.Count,
                KhieuNais = khieuNais,
            };

            return View(vm);
        }

        // ── DUYỆT ĐƠN ────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetDon(int id)
        {
            var baiXe = await _db.BaiXes.FindAsync(id);
            if (baiXe != null && baiXe.TrangThai == "Chờ duyệt")
            {
                baiXe.TrangThai = "Hoạt động";
                baiXe.GhiChu = null;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã duyệt bãi \"{baiXe.TenBai}\" thành công.";
            }
            return RedirectToAction(nameof(Index), new { tab = "choduyet" });
        }

        // ── TỪ CHỐI ĐƠN ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiDon(int id, string ghiChu)
        {
            var baiXe = await _db.BaiXes.FindAsync(id);
            if (baiXe != null && baiXe.TrangThai == "Chờ duyệt")
            {
                baiXe.TrangThai = "Từ chối";
                baiXe.GhiChu = ghiChu;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã từ chối đơn đăng ký bãi \"{baiXe.TenBai}\".";
            }
            return RedirectToAction(nameof(Index), new { tab = "choduyet" });
        }

        // ── ĐỔI TRẠNG THÁI BÃI ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id, string trangThai, string? lyDo)
        {
            var bai = await _db.BaiXes.FindAsync(id);
            if (bai != null)
            {
                var trangThaiCu = bai.TrangThai;
                bai.TrangThai = trangThai;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã đổi trạng thái bãi \"{bai.TenBai}\" từ {trangThaiCu} → {trangThai}.";
            }
            return RedirectToAction(nameof(Index), new { tab = "hoatdong" });
        }

        // ── KHÓA / MỞ BÃI ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaBai(int id, string lyDo, string? returnTab)
        {
            var bai = await _db.BaiXes.FindAsync(id);
            if (bai != null)
            {
                if (bai.TrangThai == "Hoạt động")
                {
                    bai.TrangThai = "Tạm đóng";
                    TempData["Success"] = $"Đã khóa bãi \"{bai.TenBai}\". Lý do: {lyDo}";
                }
                else
                {
                    bai.TrangThai = "Hoạt động";
                    TempData["Success"] = $"Đã mở khóa bãi \"{bai.TenBai}\" thành công.";
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { tab = returnTab ?? "hoatdong" });
        }
    }
}
