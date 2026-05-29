using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Areas.Admin.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaHongController : Controller
    {
        private readonly AppDbContext _db;
        public HoaHongController(AppDbContext db) => _db = db;

        // ── INDEX ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? search, string? locTrangThai)
        {
            var now = DateTime.Now;

            var baiQuery = _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .Where(b => b.TrangThai != "Chờ duyệt" && b.TrangThai != "Từ chối")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                baiQuery = baiQuery.Where(b =>
                    b.TenBai.Contains(search) ||
                    (b.ChuBaiXe != null && b.ChuBaiXe.TenChuBai.Contains(search)) ||
                    (b.ChuBaiXe != null && b.ChuBaiXe.Email.Contains(search)));

            if (!string.IsNullOrWhiteSpace(locTrangThai))
                baiQuery = baiQuery.Where(b => b.TrangThai == locTrangThai);

            var baiList = await baiQuery.OrderBy(b => b.TenBai).ToListAsync();

            // Hóa đơn tháng hiện tại
            var hoaDonThang = await _db.HoaDons
                .Include(h => h.DatCho).ThenInclude(d => d.ChoDauXe)
                    .ThenInclude(c => c!.KhuVuc)
                .Where(h => h.TrangThai == "Đã thanh toán"
                         && h.NgayTao.Month == now.Month
                         && h.NgayTao.Year == now.Year)
                .ToListAsync();

            // Phản hồi chưa đọc từ Owner gửi về Admin
            var phanHoiChuaDoc = await _db.ThongBaos
                .Where(t => (t.LoaiThongBao == "PhanHoiHoaHong_ChapNhan"
                          || t.LoaiThongBao == "PhanHoiHoaHong_TuChoi")
                         && !t.DaDoc)
                .ToListAsync();

            var danhSach = baiList.Select(b =>
            {
                var hdBai = hoaDonThang
                    .Where(h => h.DatCho?.ChoDauXe?.KhuVuc?.IDBaiXe == b.ID)
                    .ToList();

                // Tìm phản hồi mới nhất của bãi này
                var phanHoiBai = phanHoiChuaDoc
                    .Where(t => t.DuongDan != null && t.DuongDan.Contains($"bai={b.ID}"))
                    .OrderByDescending(t => t.NgayTao)
                    .FirstOrDefault();

                return new HoaHongBaiXeItem
                {
                    IDBaiXe          = b.ID,
                    TenBai           = b.TenBai,
                    TenChuBai        = b.ChuBaiXe?.TenChuBai ?? "—",
                    Email            = b.ChuBaiXe?.Email ?? "—",
                    TrangThai        = b.TrangThai,
                    PhanTramHienTai  = b.PhanTramChietKhau,
                    DoanhThuThang    = hdBai.Sum(h => h.TongTien),
                    HoaHongThang     = hdBai.Sum(h => h.TienChietKhauAdmin),
                    SoGiaoDichThang  = hdBai.Count,
                    TrangThaiPhanHoi = phanHoiBai?.LoaiThongBao,
                    NoiDungPhanHoi   = phanHoiBai?.NoiDung,
                    IDThongBaoPhanHoi = phanHoiBai?.ID
                };
            }).ToList();

            var tyLeMacDinh = baiList.Any()
                ? Math.Round(baiList.Average(b => b.PhanTramChietKhau), 2)
                : 10m;

            var vm = new HoaHongViewModel
            {
                Search               = search,
                LocTrangThai         = locTrangThai,
                TyLeMacDinh          = tyLeMacDinh,
                TyLeMacDinhHeThong   = tyLeMacDinh,
                TongBaiApDungMacDinh = baiList.Count(b => b.PhanTramChietKhau == tyLeMacDinh),
                TongBaiTungChinhnh   = baiList.Count(b => b.PhanTramChietKhau != tyLeMacDinh),
                TongHoaHongThang     = danhSach.Sum(x => x.HoaHongThang),
                TongDoanhThuThang    = danhSach.Sum(x => x.DoanhThuThang),
                SoPhanHoiChuaDoc     = phanHoiChuaDoc.Count,
                DanhSachBai          = danhSach
            };

            return View(vm);
        }

        // ── CẬP NHẬT TỶ LỆ MỘT BÃI (gửi thông báo chờ Owner phản hồi) ─────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTyLe(int id, decimal phanTram, string? lyDo)
        {
            if (phanTram < 0 || phanTram > 100)
            {
                TempData["Error"] = "Tỷ lệ hoa hồng phải từ 0% đến 100%.";
                return RedirectToAction(nameof(Index));
            }

            var bai = await _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (bai == null)
            {
                TempData["Error"] = "Không tìm thấy bãi xe.";
                return RedirectToAction(nameof(Index));
            }

            var tyLeCu = bai.PhanTramChietKhau;

            // KHÔNG cập nhật ngay — chờ Owner xác nhận
            // Lưu tỷ lệ đề xuất vào NoiDung thông báo để Owner biết
            if (bai.ChuBaiXe != null)
            {
                _db.ThongBaos.Add(new ThongBao
                {
                    IDTaiKhoan   = bai.ChuBaiXe.IDTaiKhoan,
                    TieuDe       = $"💰 Đề xuất thay đổi tỷ lệ hoa hồng: {tyLeCu}% → {phanTram}%",
                    NoiDung      = $"Admin đề xuất thay đổi tỷ lệ chiết khấu bãi xe \"{bai.TenBai}\" từ {tyLeCu}% → {phanTram}%."
                                 + (string.IsNullOrWhiteSpace(lyDo) ? "" : $"\nLý do: {lyDo}")
                                 + $"\n\nVui lòng xác nhận chấp nhận hoặc phản hồi lại Admin."
                                 + $"|TyLeMoi={phanTram}|TyLeCu={tyLeCu}|IDBai={id}",
                    LoaiThongBao = "DeXuatHoaHong",
                    DuongDan     = $"/Owner/BaiXe?phanhoihoahong=1&bai={id}",
                    NgayTao      = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã gửi đề xuất tỷ lệ {phanTram}% đến chủ bãi \"{bai.TenBai}\". Đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        // ── ÁP DỤNG TỶ LỆ ĐỒNG LOẠT ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApDungDongLoat(decimal phanTram, string? locTrangThai, string? lyDo)
        {
            if (phanTram < 0 || phanTram > 100)
            {
                TempData["Error"] = "Tỷ lệ hoa hồng phải từ 0% đến 100%.";
                return RedirectToAction(nameof(Index));
            }

            var baiQuery = _db.BaiXes
                .Include(b => b.ChuBaiXe)
                .Where(b => b.TrangThai != "Chờ duyệt" && b.TrangThai != "Từ chối");

            if (!string.IsNullOrWhiteSpace(locTrangThai))
                baiQuery = baiQuery.Where(b => b.TrangThai == locTrangThai);

            var danhSach = await baiQuery.ToListAsync();
            int soLuong = 0;

            foreach (var bai in danhSach)
            {
                if (bai.PhanTramChietKhau == phanTram) continue;

                var tyLeCu = bai.PhanTramChietKhau;
                soLuong++;

                if (bai.ChuBaiXe != null)
                {
                    _db.ThongBaos.Add(new ThongBao
                    {
                        IDTaiKhoan   = bai.ChuBaiXe.IDTaiKhoan,
                        TieuDe       = $"💰 Đề xuất thay đổi tỷ lệ hoa hồng: {tyLeCu}% → {phanTram}%",
                        NoiDung      = $"Admin đề xuất thay đổi tỷ lệ chiết khấu bãi xe \"{bai.TenBai}\" từ {tyLeCu}% → {phanTram}%."
                                     + (string.IsNullOrWhiteSpace(lyDo) ? "" : $"\nLý do: {lyDo}")
                                     + $"\n\nVui lòng xác nhận chấp nhận hoặc phản hồi lại Admin."
                                     + $"|TyLeMoi={phanTram}|TyLeCu={tyLeCu}|IDBai={bai.ID}",
                        LoaiThongBao = "DeXuatHoaHong",
                        DuongDan     = $"/Owner/BaiXe?phanhoihoahong=1&bai={bai.ID}",
                        NgayTao      = DateTime.Now
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = soLuong > 0
                ? $"Đã gửi đề xuất tỷ lệ {phanTram}% đến {soLuong} chủ bãi. Đang chờ xác nhận."
                : "Không có bãi nào cần cập nhật.";

            return RedirectToAction(nameof(Index));
        }

        // ── ADMIN ĐỌC PHẢN HỒI (đánh dấu đã đọc + áp dụng nếu chấp nhận) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocPhanHoi(int idThongBao)
        {
            var tb = await _db.ThongBaos.FindAsync(idThongBao);
            if (tb == null) return RedirectToAction(nameof(Index));

            tb.DaDoc = true;

            // Nếu Owner chấp nhận → áp dụng tỷ lệ mới vào DB
            if (tb.LoaiThongBao == "PhanHoiHoaHong_ChapNhan" && tb.DuongDan != null)
            {
                // Parse IDBai và TyLeMoi từ DuongDan: "/Admin/HoaHong?bai=5&tylemoi=12"
                var uri = new Uri("http://x" + tb.DuongDan);
                var qs  = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (int.TryParse(qs["bai"], out int idBai)
                 && decimal.TryParse(qs["tylemoi"], System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture, out decimal tyLeMoi))
                {
                    var bai = await _db.BaiXes.Include(b => b.ChuBaiXe).FirstOrDefaultAsync(b => b.ID == idBai);
                    if (bai != null)
                    {
                        var tyLeCu = bai.PhanTramChietKhau;
                        bai.PhanTramChietKhau = tyLeMoi;

                        // Thông báo xác nhận lại cho Owner
                        if (bai.ChuBaiXe != null)
                        {
                            _db.ThongBaos.Add(new ThongBao
                            {
                                IDTaiKhoan   = bai.ChuBaiXe.IDTaiKhoan,
                                TieuDe       = $"✅ Tỷ lệ hoa hồng đã được áp dụng: {tyLeMoi}%",
                                NoiDung      = $"Admin đã ghi nhận phản hồi của bạn. Tỷ lệ hoa hồng bãi xe \"{bai.TenBai}\" chính thức là {tyLeMoi}% kể từ hôm nay.",
                                LoaiThongBao = "HoaHongApDung",
                                DuongDan     = "/Owner/ThongKe",
                                NgayTao      = DateTime.Now
                            });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã ghi nhận phản hồi.";
            return RedirectToAction(nameof(Index));
        }

        // ── API: đếm phản hồi chưa đọc (cho badge sidebar) ─────────────────
        [HttpGet]
        public async Task<IActionResult> SoPhanHoiChuaDoc()
        {
            var count = await _db.ThongBaos
                .CountAsync(t => (t.LoaiThongBao == "PhanHoiHoaHong_ChapNhan"
                               || t.LoaiThongBao == "PhanHoiHoaHong_TuChoi")
                              && !t.DaDoc);
            return Json(new { count });
        }
    }
}
