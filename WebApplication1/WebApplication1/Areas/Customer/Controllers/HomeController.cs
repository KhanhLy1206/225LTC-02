using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        // GET: /Customer/Home/Index (hoặc /Customer)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Customer/Home/Bookings
        public IActionResult Bookings()
        {
            return View();
        }

        // GET: /Customer/Home/Search
        public IActionResult Search()
        {
            return View();
        }

        // GET: /Customer/Home/Vehicles
        public IActionResult Vehicles()
        {
            return View();
        }

        // GET: /Customer/Home/Chat
        public IActionResult Chat()
        {
            return View();
        }

        // GET: /Customer/Home/Complaints
        public IActionResult Complaints()
        {
            return View();
        }

        // GET: /Customer/Home/Profile
        public IActionResult Profile()
        {
            return View();
        }
    }
}
