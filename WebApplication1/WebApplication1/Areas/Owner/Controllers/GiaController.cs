using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    public class GiaController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
