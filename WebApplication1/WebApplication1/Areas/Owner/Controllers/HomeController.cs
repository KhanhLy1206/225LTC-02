using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class HomeController : Controller
    {
        // GET: /Owner/Home/Index hoặc /Owner
        public IActionResult Index()
        {
            return RedirectToAction("Index", "BaiXe");
        }
    }
}
