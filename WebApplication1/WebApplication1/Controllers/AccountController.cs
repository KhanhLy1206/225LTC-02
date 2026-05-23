using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // GET: /Account/RegisterOwner
        public IActionResult RegisterOwner()
        {
            // Dự phòng cho trang đăng ký chủ bãi xe
            return View();
        }
    }
}
