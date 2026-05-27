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
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0) return Json(new { success = false, message = "Chưa đăng nhập." });

            int chuBaiId = GetChuBaiId(ownerId);
            if (chuBaiId == 0) return Json(new { success = false, message = "Không tìm thấy thông tin chủ bãi." });

            var notifications = await _context.BaiXes
                .Include(b => b.XaPhuong).ThenInclude(x => x!.QuanHuyen).ThenInclude(q => q!.TinhThanh)
                .Where(b => b.IDChuBai == chuBaiId && b.TrangThai != "Chờ duyệt")
                .OrderByDescending(b => b.NgayGui)
                .ToListAsync();

            var data = notifications.Select(b => new {
                id = b.ID,
                tenBai = b.TenBai,
                trangThai = b.TrangThai,
                ghiChu = string.IsNullOrEmpty(b.GhiChu) 
                    ? (b.TrangThai == "Hoạt động" ? "Hồ sơ hợp lệ. Bãi đỗ xe của bạn đã được Admin phê duyệt thành công và đã đi vào hoạt động trên hệ thống." : "")
                    : b.GhiChu,
                ngayGui = b.NgayGui.ToString("dd/MM/yyyy HH:mm"),
                diaChi = b.DiaChiChiTiet + (b.XaPhuong != null ? ", " + b.XaPhuong.TenXa + ", " + b.XaPhuong.QuanHuyen.TenHuyen + ", " + b.XaPhuong.QuanHuyen.TinhThanh.TenTinh : ""),
                sucChua = b.SucChua,
                dienTich = b.DienTich
            }).ToList();

            return Json(new { success = true, data = data });
        }
    }
}
