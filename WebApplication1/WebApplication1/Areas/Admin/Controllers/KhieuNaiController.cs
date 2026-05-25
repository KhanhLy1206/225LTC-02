using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhieuNaiController : Controller
    {
        private readonly AppDbContext _db;
        public KhieuNaiController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search, string? trangThai)
        {
            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            var query = _db.KhieuNais
                .Include(k => k.KhachHang)
                .Include(k => k.BaiXe)
                .Include(k => k.DatCho)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(k =>
                    k.TieuDe.Contains(search) ||
                    k.KhachHang.HoTen.Contains(search));

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(k => k.TrangThai == trangThai);

            ViewBag.SoChoXuLy    = await _db.KhieuNais.CountAsync(k => k.TrangThai == "Chờ xử lý");
            ViewBag.SoDangXuLy   = await _db.KhieuNais.CountAsync(k => k.TrangThai == "Đang xử lý");
            ViewBag.SoDaGiaiQuyet= await _db.KhieuNais.CountAsync(k => k.TrangThai == "Đã giải quyết");
            ViewBag.SoTuChoi     = await _db.KhieuNais.CountAsync(k => k.TrangThai == "Từ chối");

            ViewBag.KhieuNais = await query.OrderByDescending(k => k.NgayGui).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> XuLy(int id, string trangThai, string? ghiChu)
        {
            var kn = await _db.KhieuNais.FindAsync(id);
            if (kn != null)
            {
                kn.TrangThai  = trangThai;
                kn.GhiChuAdmin = ghiChu;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
