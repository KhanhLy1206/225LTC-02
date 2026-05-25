using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Areas.Admin.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NguoiDungController : Controller
    {
        private readonly AppDbContext _db;
        public NguoiDungController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search, string? loai, string tab = "khachhang")
        {
            ViewBag.Tab    = tab;
            ViewBag.Search = search;
            ViewBag.Loai   = loai;

            // Thống kê tổng quan
            ViewBag.TongTaiKhoan      = await _db.TaiKhoans.CountAsync();
            ViewBag.TongHoatDong      = await _db.TaiKhoans.CountAsync(t => t.TrangThai);
            ViewBag.TongKhoa          = await _db.TaiKhoans.CountAsync(t => !t.TrangThai);
            ViewBag.TongKhachHang     = await _db.KhachHangs.CountAsync();
            ViewBag.TongChuBai        = await _db.ChuBaiXes.CountAsync();

            var khQuery = _db.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.XaPhuong)
                .Include(k => k.DatChos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                khQuery = khQuery.Where(k =>
                    k.HoTen.Contains(search) ||
                    k.TaiKhoan.TenDangNhap.Contains(search) ||
                    (k.Email != null && k.Email.Contains(search)) ||
                    (k.SDT   != null && k.SDT.Contains(search)));

            if (!string.IsNullOrEmpty(loai))
                khQuery = khQuery.Where(k => k.LoaiKH == loai);

            var cbQuery = _db.ChuBaiXes
                .Include(c => c.TaiKhoan)
                .Include(c => c.BaiXes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search) && tab == "owner")
                cbQuery = cbQuery.Where(c =>
                    c.TenChuBai.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.SDT.Contains(search));

            ViewBag.KhachHangs = await khQuery.OrderByDescending(k => k.ID).ToListAsync();
            ViewBag.ChuBais    = await cbQuery.OrderByDescending(c => c.ID).ToListAsync();

            return View();
        }

        // Chi tiết tài khoản khách hàng
        public async Task<IActionResult> ChiTiet(int id)
        {
            var kh = await _db.KhachHangs
                .Include(k => k.TaiKhoan).ThenInclude(t => t.VaiTro)
                .Include(k => k.XaPhuong).ThenInclude(x => x.QuanHuyen).ThenInclude(q => q.TinhThanh)
                .Include(k => k.DatChos).ThenInclude(d => d.ChoDauXe)
                    .ThenInclude(c => c.KhuVuc).ThenInclude(kv => kv.BaiXe)
                .Include(k => k.KhieuNais).ThenInclude(kn => kn.BaiXe)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kh == null) return NotFound();

            var vm = new ChiTietNguoiDungViewModel
            {
                KhachHang       = kh,
                TongDatCho      = kh.DatChos.Count,
                DatChoHoanThanh = kh.DatChos.Count(d => d.TrangThai == "Hoàn thành"),
                DatChoHuy       = kh.DatChos.Count(d => d.TrangThai == "Đã hủy"),
                TongChiTieu     = await _db.HoaDons
                    .Where(h => h.DatCho.IDKhachHang == id && h.TrangThai == "Đã thanh toán")
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0,
                KhieuNais       = kh.KhieuNais.OrderByDescending(k => k.NgayGui).ToList(),
                LichSuDatCho    = kh.DatChos.OrderByDescending(d => d.NgayDat).Take(10).ToList(),

                // Hành vi bất thường: hủy nhiều, khiếu nại nhiều
                SoLanHuy        = kh.DatChos.Count(d => d.TrangThai == "Đã hủy"),
                SoKhieuNai      = kh.KhieuNais.Count,
                CanhBaoHanhVi   = new List<string>()
            };

            if (vm.SoLanHuy >= 3)
                vm.CanhBaoHanhVi.Add($"Hủy đặt chỗ {vm.SoLanHuy} lần — có thể spam đặt chỗ");
            if (vm.SoKhieuNai >= 2)
                vm.CanhBaoHanhVi.Add($"Gửi {vm.SoKhieuNai} khiếu nại — cần theo dõi");
            if (kh.DatChos.Count(d => d.TrangThai == "Quá hạn") >= 2)
                vm.CanhBaoHanhVi.Add("Quá hạn đặt chỗ nhiều lần — không đến đúng giờ");

            return View(vm);
        }

        // Khóa / Mở tài khoản
        [HttpPost]
        public async Task<IActionResult> KhoaTaiKhoan(int idTaiKhoan, string? lyDo, string? returnUrl)
        {
            var tk = await _db.TaiKhoans.FindAsync(idTaiKhoan);
            if (tk != null)
            {
                tk.TrangThai = !tk.TrangThai;
                // Lưu lý do vào TempData để hiển thị
                if (!tk.TrangThai && !string.IsNullOrEmpty(lyDo))
                    TempData["ThongBao"] = $"Đã khóa tài khoản. Lý do: {lyDo}";
                else
                    TempData["ThongBao"] = "Đã mở khóa tài khoản thành công.";

                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}
