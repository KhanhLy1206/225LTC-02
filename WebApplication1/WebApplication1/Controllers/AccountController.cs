using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using WebApplication1.Models.ViewModels;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly OtpService _otp;

        public AccountController(IAccountService accountService, AppDbContext db,
                                  IEmailService email, OtpService otp)
        {
            _accountService = accountService;
            _db    = db;
            _email = email;
            _otp   = otp;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserBasedOnRole();
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await _accountService.ValidateLoginAsync(model.TenDangNhapHoacEmail, model.MatKhau);
            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập/Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // Create claims list
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro.TenVaiTro),
                new Claim("UserId", taiKhoan.ID.ToString())
            };

            // Add customer's full name if available
            var khachHang = taiKhoan.KhachHang;
            if (khachHang != null)
            {
                claims.Add(new Claim("FullName", khachHang.HoTen));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect according to user role
            if (taiKhoan.VaiTro.TenVaiTro == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (taiKhoan.VaiTro.TenVaiTro == "Chủ bãi xe")
            {
                return RedirectToAction("Index", "Home", new { area = "Owner" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserBasedOnRole();
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.RegisterCustomerAsync(model);
            if (result)
            {
                TempData["RegisterSuccess"] = "Đăng ký tài khoản khách hàng thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký.");
            return View(model);
        }

        // GET: /Account/RegisterOwner
        [HttpGet]
        public IActionResult RegisterOwner()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectUserBasedOnRole();

            ViewBag.TinhThanhs = _db.TinhThanhs.OrderBy(t => t.TenTinh).ToList();
            return View();
        }

        // POST: /Account/RegisterOwner — lưu đơn đăng ký bãi xe, chưa tạo tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterOwner(
            string TenBaiXe, string Email, string SoDienThoai,
            string MaXa, string DiaChiChiTiet,
            int SucChua, decimal DienTich,
            IFormFile? GiayPhepKinhDoanh, IFormFile? HinhAnh)
        {
            // Kiểm tra email đã tồn tại chưa
            var emailExists = await _db.ChuBaiXes.AnyAsync(c => c.Email == Email)
                           || await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == Email);
            if (emailExists)
            {
                TempData["Error"] = "Email này đã được đăng ký. Vui lòng dùng email khác.";
                ViewBag.TinhThanhs = _db.TinhThanhs.OrderBy(t => t.TenTinh).ToList();
                return View();
            }

            // Lưu file hình ảnh
            string hinhAnhPath = "/images/default-parking.jpg";
            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                var ext  = Path.GetExtension(HinhAnh.FileName);
                var name = $"bai_{Guid.NewGuid():N}{ext}";
                var dir  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "baiXe");
                Directory.CreateDirectory(dir);
                using var fs = System.IO.File.Create(Path.Combine(dir, name));
                await HinhAnh.CopyToAsync(fs);
                hinhAnhPath = $"/uploads/baiXe/{name}";
            }

            // Lưu giấy phép
            string gpPath = "";
            if (GiayPhepKinhDoanh != null && GiayPhepKinhDoanh.Length > 0)
            {
                var ext  = Path.GetExtension(GiayPhepKinhDoanh.FileName);
                var name = $"gp_{Guid.NewGuid():N}{ext}";
                var dir  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "giayPhep");
                Directory.CreateDirectory(dir);
                using var fs = System.IO.File.Create(Path.Combine(dir, name));
                await GiayPhepKinhDoanh.CopyToAsync(fs);
                gpPath = $"/uploads/giayPhep/{name}";
            }

            // Tạo tài khoản tạm (chưa có ChuBaiXe) — dùng placeholder IDChuBai = 0
            // Thực tế: tạo ChuBaiXe tạm hoặc lưu thông tin vào BaiXe trực tiếp
            // Vì SQL mới gộp đăng ký vào BaiXe, cần tạo ChuBaiXe tạm trước
            var vaiTro = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "Chủ bãi xe");
            if (vaiTro == null)
            {
                vaiTro = new VaiTro { TenVaiTro = "Chủ bãi xe" };
                _db.VaiTros.Add(vaiTro);
                await _db.SaveChangesAsync();
            }

            // Tạo TaiKhoan tạm (chưa kích hoạt — TrangThai = false)
            var taiKhoan = new TaiKhoan
            {
                IDVaiTro    = vaiTro.ID,
                TenDangNhap = Email,
                MatKhau     = "PENDING", // sẽ được set khi admin duyệt
                TrangThai   = false      // chưa kích hoạt
            };
            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            // Tạo ChuBaiXe
            var chuBai = new ChuBaiXe
            {
                IDTaiKhoan    = taiKhoan.ID,
                TenChuBai     = Email.Split('@')[0], // tạm dùng phần trước @ làm tên
                SDT           = SoDienThoai,
                Email         = Email,
                MaXa          = MaXa,
                DiaChiChiTiet = DiaChiChiTiet
            };
            _db.ChuBaiXes.Add(chuBai);
            await _db.SaveChangesAsync();

            // Tạo BaiXe với TrangThai = "Chờ duyệt"
            var baiXe = new BaiXe
            {
                IDChuBai          = chuBai.ID,
                TenBai            = TenBaiXe,
                MaXa              = MaXa,
                DiaChiChiTiet     = DiaChiChiTiet,
                SucChua           = SucChua > 0 ? SucChua : 50,
                DienTich          = DienTich > 0 ? DienTich : 500,
                SoDienThoai       = SoDienThoai,
                PhanTramChietKhau = 10,
                TrangThai         = "Chờ duyệt",
                HinhAnh           = hinhAnhPath,
                GiayPhepKinhDoanh = gpPath,
                NgayGui           = DateTime.Now
            };
            _db.BaiXes.Add(baiXe);
            await _db.SaveChangesAsync();

            TempData["RegisterOwnerSuccess"] = $"Đơn đăng ký bãi xe \"{TenBaiXe}\" đã được gửi thành công! Chúng tôi sẽ xem xét và gửi thông tin tài khoản về email {Email} trong vòng 1-3 ngày làm việc.";
            return RedirectToAction("Login");
        }

        // ── OTP: Gửi mã xác thực email ──────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> GuiOtp([FromBody] GuiOtpRequest req)
        {
            if (string.IsNullOrEmpty(req.Email) || !req.Email.Contains('@'))
                return Json(new { success = false, message = "Email không hợp lệ." });

            // Kiểm tra email đã tồn tại chưa
            var exists = await _db.KhachHangs.AnyAsync(k => k.Email == req.Email)
                      || await _db.ChuBaiXes.AnyAsync(c => c.Email == req.Email);
            if (exists)
                return Json(new { success = false, message = "Email này đã được đăng ký." });

            var otp = _otp.TaoOtp(req.Email);
            var (subject, html) = EmailTemplates.GuiOtp(req.Email.Split('@')[0], otp, req.Loai ?? "tài khoản");

            try
            {
                await _email.GuiEmailAsync(req.Email, req.Email.Split('@')[0], subject, html);
                return Json(new { success = true, message = $"Mã OTP đã gửi về {req.Email}. Kiểm tra hộp thư (kể cả Spam)." });
            }
            catch
            {
                // Nếu chưa cấu hình email, vẫn cho đăng ký (dev mode)
                return Json(new { success = true, message = $"[DEV] OTP: {otp} (email chưa cấu hình)" });
            }
        }

        // ── OTP: Xác thực mã ─────────────────────────────────────────────────
        [HttpPost]
        public IActionResult XacThucOtp([FromBody] XacThucOtpRequest req)
        {
            if (_otp.XacThucOtp(req.Email, req.Otp))
                return Json(new { success = true });
            return Json(new { success = false, message = "Mã OTP không đúng hoặc đã hết hạn." });
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectUserBasedOnRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            if (User.IsInRole("Chủ bãi xe"))
            {
                return RedirectToAction("Index", "Home", new { area = "Owner" });
            }
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}

// ── Request models cho OTP ──────────────────────────────────────────────────
public class GuiOtpRequest
{
    public string? Email { get; set; }
    public string? Loai  { get; set; }
}

public class XacThucOtpRequest
{
    public string Email { get; set; } = "";
    public string Otp   { get; set; } = "";
}
