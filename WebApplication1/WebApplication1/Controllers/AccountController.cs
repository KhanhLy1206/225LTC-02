using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models.ViewModels;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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
            {
                return RedirectUserBasedOnRole();
            }
            return View();
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
