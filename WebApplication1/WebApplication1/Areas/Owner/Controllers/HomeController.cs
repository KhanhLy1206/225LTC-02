using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class HomeController : Controller
    {
        // GET: /Owner/Home/Index hoặc /Owner
        public IActionResult Index()
        {
            return RedirectToAction("Index", "BaiXe");
        }
    }
}
