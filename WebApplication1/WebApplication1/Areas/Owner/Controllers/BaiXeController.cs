using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class BaiXeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BaiXeController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetCurrentOwnerId()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("AccountId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                return id;
            }
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var tk = _context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == username);
                if (tk != null) return tk.ID;
            }
            return 0;
        }

        private int GetChuBaiId(int accountId)
        {
            var chuBai = _context.ChuBaiXes.FirstOrDefault(c => c.IDTaiKhoan == accountId);
            return chuBai != null ? chuBai.ID : 0;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            int chuBaiId = GetChuBaiId(ownerId);

            // DEBUG LOGS
            Console.WriteLine($"[DEBUG] Logged in Owner ID (TaiKhoan): {ownerId}, mapped to ChuBaiXe ID: {chuBaiId}");


            var danhSachBaiXe = await _context.BaiXes
                .Include(b => b.XaPhuong)
                    .ThenInclude(x => x!.QuanHuyen)
                        .ThenInclude(q => q!.TinhThanh)
                .Where(b => b.IDChuBai == chuBaiId)
                .ToListAsync();

            // Tính toán KPIs
            ViewBag.TongSoBaiXe = danhSachBaiXe.Count;
            ViewBag.TongSucChua = danhSachBaiXe.Sum(b => b.SucChua);
            ViewBag.SoBaiHoatDong = danhSachBaiXe.Count(b => b.TrangThai == "Hoạt động");
            ViewBag.SoBaiChoDuyet = danhSachBaiXe.Count(b => b.TrangThai == "Chờ duyệt");
            ViewBag.OwnerId = ownerId; // Truyền xuống View để dễ kiểm tra

            // Tải danh sách tỉnh/thành phục vụ dropdown trong modal
            ViewBag.Provinces = await _context.TinhThanhs.ToListAsync();

            return View(danhSachBaiXe);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaiXe model, IFormFile? HinhAnhFile, IFormFile? GiayPhepKinhDoanhFile)
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            int chuBaiId = GetChuBaiId(ownerId);
            string? hinhAnhPath = null;
            string? giayPhepPath = null;

            // Xử lý tệp hình ảnh tải lên
            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(HinhAnhFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhFile.CopyToAsync(fileStream);
                    }
                    hinhAnhPath = "/uploads/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("HinhAnh", "Có lỗi xảy ra khi tải lên tệp ảnh: " + ex.Message);
                }
            }

            // Xử lý tệp giấy phép kinh doanh tải lên
            if (GiayPhepKinhDoanhFile != null && GiayPhepKinhDoanhFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(GiayPhepKinhDoanhFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await GiayPhepKinhDoanhFile.CopyToAsync(fileStream);
                    }
                    giayPhepPath = "/uploads/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("GiayPhepKinhDoanh", "Có lỗi xảy ra khi tải lên giấy phép kinh doanh: " + ex.Message);
                }
            }

            // Kiểm tra tải tệp bắt buộc
            if (HinhAnhFile == null || HinhAnhFile.Length == 0)
            {
                ModelState.AddModelError("HinhAnh", "Vui lòng tải lên hình ảnh bãi đỗ xe.");
            }
            if (GiayPhepKinhDoanhFile == null || GiayPhepKinhDoanhFile.Length == 0)
            {
                ModelState.AddModelError("GiayPhepKinhDoanh", "Vui lòng tải lên giấy phép kinh doanh.");
            }

            // Gán các thuộc tính ngầm định trực tiếp vào Model bãi xe
            model.IDChuBai = chuBaiId;
            model.TrangThai = "Chờ duyệt";
            model.PhanTramChietKhau = 10.00m;
            model.NgayGui = DateTime.Now;
            if (hinhAnhPath != null)
            {
                model.HinhAnh = hinhAnhPath;
            }
            if (giayPhepPath != null)
            {
                model.GiayPhepKinhDoanh = giayPhepPath;
            }

            // Loại bỏ các trường tự động tạo hoặc gán thủ công khỏi validation
            ModelState.Remove(nameof(model.IDChuBai));
            ModelState.Remove(nameof(model.TrangThai));
            ModelState.Remove(nameof(model.PhanTramChietKhau));
            ModelState.Remove(nameof(model.NgayGui));
            ModelState.Remove(nameof(model.HinhAnh));
            ModelState.Remove(nameof(model.GiayPhepKinhDoanh));

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Gửi hồ sơ đăng ký bãi xe thành công! Vui lòng đợi quản trị viên phê duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, tải lại dữ liệu để hiển thị lỗi
            var danhSachBaiXe = await _context.BaiXes
                .Include(b => b.XaPhuong)
                    .ThenInclude(x => x!.QuanHuyen)
                        .ThenInclude(q => q!.TinhThanh)
                .Where(b => b.IDChuBai == chuBaiId)
                .ToListAsync();

            ViewBag.TongSoBaiXe = danhSachBaiXe.Count;
            ViewBag.TongSucChua = danhSachBaiXe.Sum(b => b.SucChua);
            ViewBag.SoBaiHoatDong = danhSachBaiXe.Count(b => b.TrangThai == "Hoạt động");
            ViewBag.SoBaiChoDuyet = danhSachBaiXe.Count(b => b.TrangThai == "Chờ duyệt");

            ViewBag.Provinces = await _context.TinhThanhs.ToListAsync();
            TempData["Error"] = "Đăng ký không thành công. Vui lòng kiểm tra lại dữ liệu nhập.";

            return View("Index", danhSachBaiXe);
        }

        [HttpGet("GetDistricts")]
        public async Task<IActionResult> GetDistricts(string provinceId)
        {
            var districts = await _context.QuanHuyens
                .Where(q => q.MaTinh == provinceId)
                .Select(q => new { id = q.MaHuyen, name = q.TenHuyen })
                .ToListAsync();
            return Json(districts);
        }

        [HttpGet("GetWards")]
        public async Task<IActionResult> GetWards(string districtId)
        {
            var wards = await _context.XaPhuongs
                .Where(w => w.MaHuyen == districtId)
                .Select(w => new { id = w.MaXa, name = w.TenXa })
                .ToListAsync();
            return Json(wards);
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                int ownerId = GetCurrentOwnerId();
                if (ownerId == 0) return Json(new { success = false, message = "Chưa đăng nhập." });

                int chuBaiId = GetChuBaiId(ownerId);

                // Đồng bộ cấu trúc Database: thêm cột DuongDan vào bảng ThongBao nếu chưa có
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ThongBao') AND name = 'DuongDan') ALTER TABLE ThongBao ADD DuongDan NVARCHAR(500) NULL;");
                }
                catch (Exception) { }

                // Lấy tất cả thông báo liên quan đến bãi xe của chủ này
                var notifications = await _context.ThongBaos
                    .Where(t => t.IDTaiKhoan == ownerId)
                    .OrderByDescending(t => t.NgayTao)
                    .Take(30)
                    .ToListAsync();

                // Lấy danh sách bãi xe của chủ để map thông tin chi tiết
                var baiXeList = await _context.BaiXes
                    .Include(b => b.XaPhuong).ThenInclude(x => x!.QuanHuyen).ThenInclude(q => q!.TinhThanh)
                    .Where(b => b.IDChuBai == chuBaiId)
                    .ToListAsync();

                var data = notifications.Select(t => {
                    // Tìm bãi xe khớp theo tên trong nội dung thông báo
                    var bai = baiXeList.FirstOrDefault(b => t.NoiDung != null && b.TenBai != null && t.NoiDung.Contains(b.TenBai));
                    return new {
                        id           = t.ID,
                        tieuDe       = t.TieuDe,
                        tenBai       = bai?.TenBai ?? "—",
                        trangThai    = (t.LoaiThongBao ?? "") switch {
                            "DuyetBai"   => "Hoạt động",
                            "TuChoiBai"  => "Từ chối",
                            "KhoaBai"    => "Tạm đóng",
                            "MoKhoaBai"  => "Hoạt động",
                            "TamDongBai" => "Tạm đóng",
                            "BaoTriBai"  => "Bảo trì",
                            _            => "—"
                        },
                        loaiThongBao = t.LoaiThongBao,
                        ghiChu       = t.NoiDung,
                        ngayGui      = t.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        diaChi       = bai == null ? "" :
                                       bai.DiaChiChiTiet
                                       + (bai.XaPhuong != null ? ", " + bai.XaPhuong.TenXa : "")
                                       + (bai.XaPhuong?.QuanHuyen != null ? ", " + bai.XaPhuong.QuanHuyen.TenHuyen : "")
                                       + (bai.XaPhuong?.QuanHuyen?.TinhThanh != null ? ", " + bai.XaPhuong.QuanHuyen.TinhThanh.TenTinh : ""),
                        sucChua      = bai?.SucChua ?? 0,
                        dienTich     = bai?.DienTich ?? 0,
                        daDoc        = t.DaDoc,
                        duongDan     = t.DuongDan
                    };
                }).ToList();

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] GetNotifications failed: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("MarkNotificationRead")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            int ownerId = GetCurrentOwnerId();
            var tb = await _context.ThongBaos.FirstOrDefaultAsync(t => t.ID == id && t.IDTaiKhoan == ownerId);
            if (tb != null)
            {
                tb.DaDoc = true;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        [HttpPost("MarkAllNotificationsRead")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            int ownerId = GetCurrentOwnerId();
            var unread = await _context.ThongBaos
                .Where(t => t.IDTaiKhoan == ownerId && !t.DaDoc)
                .ToListAsync();
            unread.ForEach(t => t.DaDoc = true);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ── PHẢN HỒI ĐỀ XUẤT HOA HỒNG (Owner → Admin) ──────────────────────
        [HttpPost("PhanHoiHoaHong")]
        public async Task<IActionResult> PhanHoiHoaHong(int idThongBao, bool chapNhan, string? lyDo)
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0) return Json(new { success = false, message = "Chưa đăng nhập." });

            // Tìm thông báo đề xuất gốc
            var tbDeXuat = await _context.ThongBaos
                .FirstOrDefaultAsync(t => t.ID == idThongBao && t.IDTaiKhoan == ownerId
                                       && t.LoaiThongBao == "DeXuatHoaHong");

            if (tbDeXuat == null)
                return Json(new { success = false, message = "Không tìm thấy đề xuất." });

            // Đánh dấu đã đọc thông báo gốc
            tbDeXuat.DaDoc = true;

            // Parse IDBai và TyLeMoi từ NoiDung (định dạng: ..."|TyLeMoi=12|TyLeCu=10|IDBai=5")
            int idBai = 0;
            decimal tyLeMoi = 0;
            decimal tyLeCu = 0;
            var noiDung = tbDeXuat.NoiDung ?? "";
            foreach (var part in noiDung.Split('|'))
            {
                if (part.StartsWith("TyLeMoi="))
                    decimal.TryParse(part.Replace("TyLeMoi=", ""),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out tyLeMoi);
                if (part.StartsWith("TyLeCu="))
                    decimal.TryParse(part.Replace("TyLeCu=", ""),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out tyLeCu);
                if (part.StartsWith("IDBai="))
                    int.TryParse(part.Replace("IDBai=", ""), out idBai);
            }

            // Lấy tài khoản Admin để gửi thông báo ngược lại
            var adminTk = await _context.Admins
                .Include(a => a.TaiKhoan)
                .FirstOrDefaultAsync();

            var bai = await _context.BaiXes
                .Include(b => b.ChuBaiXe)
                .FirstOrDefaultAsync(b => b.ID == idBai);

            var tenBai = bai?.TenBai ?? $"Bãi #{idBai}";
            var tenChu = bai?.ChuBaiXe?.TenChuBai ?? "Chủ bãi";

            if (adminTk != null)
            {
                var loai = chapNhan ? "PhanHoiHoaHong_ChapNhan" : "PhanHoiHoaHong_TuChoi";
                var icon = chapNhan ? "✅" : "❌";
                var trangThaiText = chapNhan ? "Chấp nhận" : "Từ chối";

                _context.ThongBaos.Add(new ThongBao
                {
                    IDTaiKhoan   = adminTk.IDTaiKhoan,
                    TieuDe       = $"{icon} {tenChu} {trangThaiText.ToLower()} đề xuất hoa hồng {tyLeMoi}% — {tenBai}",
                    NoiDung      = chapNhan
                        ? $"Chủ bãi \"{tenChu}\" đã chấp nhận tỷ lệ hoa hồng mới {tyLeMoi}% cho bãi xe \"{tenBai}\"."
                        : $"Chủ bãi \"{tenChu}\" từ chối tỷ lệ hoa hồng {tyLeMoi}% cho bãi xe \"{tenBai}\"."
                          + (string.IsNullOrWhiteSpace(lyDo) ? "" : $"\nLý do: {lyDo}"),
                    LoaiThongBao = loai,
                    // Truyền thông tin để Admin áp dụng khi đọc
                    DuongDan     = $"/Admin/HoaHong?bai={idBai}&tylemoi={tyLeMoi.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                    NgayTao      = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = chapNhan
                    ? "Đã chấp nhận đề xuất. Admin sẽ được thông báo."
                    : "Đã gửi phản hồi từ chối đến Admin."
            });
        }

        // ── API: lấy danh sách đề xuất hoa hồng chưa phản hồi ───────────────
        [HttpGet("GetDeXuatHoaHong")]
        public async Task<IActionResult> GetDeXuatHoaHong()
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0) return Json(new { success = false });

            var deXuats = await _context.ThongBaos
                .Where(t => t.IDTaiKhoan == ownerId
                         && t.LoaiThongBao == "DeXuatHoaHong"
                         && !t.DaDoc)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            var data = deXuats.Select(t =>
            {
                decimal tyLeMoi = 0, tyLeCu = 0;
                int idBai = 0;
                foreach (var part in (t.NoiDung ?? "").Split('|'))
                {
                    if (part.StartsWith("TyLeMoi="))
                        decimal.TryParse(part.Replace("TyLeMoi=", ""),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out tyLeMoi);
                    if (part.StartsWith("TyLeCu="))
                        decimal.TryParse(part.Replace("TyLeCu=", ""),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out tyLeCu);
                    if (part.StartsWith("IDBai="))
                        int.TryParse(part.Replace("IDBai=", ""), out idBai);
                }
                // Lấy nội dung hiển thị (bỏ phần metadata sau dấu |)
                var noiDungHienThi = (t.NoiDung ?? "").Split('|')[0].Trim();
                return new
                {
                    id           = t.ID,
                    tieuDe       = t.TieuDe,
                    noiDung      = noiDungHienThi,
                    tyLeMoi,
                    tyLeCu,
                    idBai,
                    ngayTao      = t.NgayTao.ToString("dd/MM/yyyy HH:mm")
                };
            }).ToList();

            return Json(new { success = true, data, count = data.Count });
        }
    }
}
