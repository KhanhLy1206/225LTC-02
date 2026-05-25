using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using WebApplication1.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentAccountId()
        {
            var accountIdClaim = User.FindFirst("AccountId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int id))
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

        private KhachHang? GetCurrentCustomer()
        {
            int accountId = GetCurrentAccountId();
            return _context.KhachHangs.FirstOrDefault(kh => kh.IDTaiKhoan == accountId);
        }

        // GET: /Customer/Home/Index
        public IActionResult Index()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            ViewBag.Customer = customer;
            ViewBag.WalletBalance = "150,000đ"; // Default static balance since wallet table is not in schema
            ViewBag.TotalBookings = _context.DatChos.Count(dc => dc.IDKhachHang == customer.ID);
            ViewBag.TotalVehicles = _context.KhachHangXes.Count(kx => kx.IDKhachHang == customer.ID);
            ViewBag.TotalComplaints = _context.KhieuNais.Count(kn => kn.IDKhachHang == customer.ID);

            // Active Bookings: Đã đặt (in time range) or Đang đỗ
            var now = DateTime.Now;
            var activeBookings = _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(cd => cd!.KhuVuc)
                        .ThenInclude(kv => kv!.BaiXe)
                .Where(dc => dc.IDKhachHang == customer.ID && (dc.TrangThai == "Đang đỗ" || (dc.TrangThai == "Đã đặt" && dc.TgianBatDau <= now && dc.TgianKetThuc >= now)))
                .OrderBy(dc => dc.TgianBatDau)
                .ToList();
            ViewBag.ActiveBookings = activeBookings;

            // Active Vehicles (registered by the customer)
            var activeVehicles = _context.KhachHangXes
                .Include(kx => kx.Xe)
                    .ThenInclude(x => x!.LoaiXe)
                .Where(kx => kx.IDKhachHang == customer.ID)
                .Select(kx => kx.Xe)
                .ToList();
            ViewBag.ActiveVehicles = activeVehicles;

            // Recent Bookings History
            var recentBookings = _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(cd => cd!.KhuVuc)
                        .ThenInclude(kv => kv!.BaiXe)
                .Where(dc => dc.IDKhachHang == customer.ID)
                .OrderByDescending(dc => dc.NgayDat)
                .Take(5)
                .ToList();
            ViewBag.RecentBookings = recentBookings;

            return View();
        }

        // GET: /Customer/Home/Bookings
        public IActionResult Bookings()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var allBookings = _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(cd => cd!.KhuVuc)
                        .ThenInclude(kv => kv!.BaiXe)
                .Where(dc => dc.IDKhachHang == customer.ID)
                .OrderByDescending(dc => dc.NgayDat)
                .ToList();

            var nowBookings = DateTime.Now;
            ViewBag.ActiveBookings = allBookings.Where(dc => dc.TrangThai == "Đang đỗ" || (dc.TrangThai == "Đã đặt" && dc.TgianBatDau <= nowBookings && dc.TgianKetThuc >= nowBookings)).ToList();
            ViewBag.UpcomingBookings = allBookings.Where(dc => dc.TrangThai == "Đã đặt" && dc.TgianBatDau > nowBookings).ToList();
            ViewBag.HistoryBookings = allBookings.Where(dc => dc.TrangThai == "Hoàn thành" || dc.TrangThai == "Đã hủy" || dc.TrangThai == "Quá hạn" || dc.TrangThai == "Đã hoàn thành" || (dc.TrangThai == "Đã đặt" && dc.TgianKetThuc < nowBookings)).ToList();

            return View();
        }

        // POST: /Customer/Home/ControlBarrier
        [HttpPost]
        public async Task<IActionResult> ControlBarrier(int bookingId, string action)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            var booking = await _context.DatChos
                .Include(dc => dc.ChoDauXe)
                .FirstOrDefaultAsync(dc => dc.ID == bookingId && dc.IDKhachHang == customer.ID);

            if (booking == null || booking.ChoDauXe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn đặt chỗ." });
            }

            string resultStatus = action == "Open" ? "Mở" : "Đóng";
            booking.ChoDauXe.TrangThaiKhoa = resultStatus;
            
            // Log control barrier
            var log = new LogDieuKhienBarrier
            {
                IDDatCho = booking.ID,
                IDTaiKhoan = GetCurrentAccountId(),
                ThoiGianLệnh = DateTime.Now,
                HanhDong = action == "Open" ? "Mở khóa" : "Khóa lại",
                KetQua = "Thành công",
                GhiChu = $"Khách hàng điều khiển IoT Barrier {booking.ChoDauXe.MaSoKhoa}"
            };

            _context.Add(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, currentStatus = resultStatus });
        }

        // POST: /Customer/Home/SubmitReview
        [HttpPost]
        public async Task<IActionResult> SubmitReview(int bookingId, int rating, string comment)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            var booking = await _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(cd => cd!.KhuVuc)
                .FirstOrDefaultAsync(dc => dc.ID == bookingId && dc.IDKhachHang == customer.ID);

            if (booking == null || booking.ChoDauXe == null || booking.ChoDauXe.KhuVuc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn đặt chỗ." });
            }

            // Check if already reviewed
            var exists = await _context.DanhGiaBinhLuans.AnyAsync(r => r.IDDatCho == bookingId);
            if (exists)
            {
                return Json(new { success = false, message = "Đơn đặt chỗ này đã được đánh giá trước đó." });
            }

            var review = new DanhGiaBinhLuan
            {
                IDKhachHang = customer.ID,
                IDBaiXe = booking.ChoDauXe.KhuVuc.IDBaiXe,
                IDDatCho = bookingId,
                DiemDanhGia = rating,
                NoiDungBinhLuan = comment,
                NgayTao = DateTime.Now
            };

            _context.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /Customer/Home/Search
        public IActionResult Search()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Get parking lots
            var parkingLots = _context.BaiXes
                .Where(b => b.TrangThai == "Hoạt động")
                .ToList();

            // Load pricing details and spots left for each parking lot
            var lotDetails = parkingLots.Select(lot => {
                var carPrice = _context.BangGias.Where(bg => bg.IDBaiXe == lot.ID && bg.LoaiXe!.TenLoaiXe == "Ô tô").Select(bg => bg.GiaTheoGio).FirstOrDefault();
                var motoPrice = _context.BangGias.Where(bg => bg.IDBaiXe == lot.ID && bg.LoaiXe!.TenLoaiXe == "Xe máy").Select(bg => bg.GiaTheoGio).FirstOrDefault();
                var spotsLeft = _context.ChoDauXes.Count(c => c.KhuVuc!.IDBaiXe == lot.ID && c.TrangThaiO == "Trống");
                return new {
                    Lot = lot,
                    CarPrice = carPrice,
                    MotoPrice = motoPrice,
                    SpotsLeft = spotsLeft
                };
            }).ToList();

            ViewBag.ParkingLots = lotDetails;

            // Load registered vehicles for dropdown
            var vehicles = _context.KhachHangXes
                .Include(kx => kx.Xe)
                .Where(kx => kx.IDKhachHang == customer.ID)
                .Select(kx => kx.Xe)
                .ToList();
            ViewBag.Vehicles = vehicles;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas(int lotId, string bienSoXe)
        {
            var vehicle = await _context.Xes.FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
            if (vehicle == null) return Json(new List<object>());

            var areas = await _context.KhuVucs
                .Where(kv => kv.IDBaiXe == lotId && kv.IDLoaiXe == vehicle.IDLoaiXe)
                .Select(kv => new { id = kv.ID, name = kv.TenKhuVuc })
                .ToListAsync();

            return Json(areas);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpots(int areaId)
        {
            var spots = await _context.ChoDauXes
                .Where(c => c.IDKhuVuc == areaId && c.TrangThaiO == "Trống")
                .Select(c => new { id = c.ID, name = c.TenChoDau })
                .ToListAsync();

            return Json(spots);
        }

        [HttpGet]
        public async Task<IActionResult> GetLotDetails(int lotId, string bienSoXe)
        {
            var vehicle = await _context.Xes.Include(x => x.LoaiXe).FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
            if (vehicle == null) return Json(new { success = false, message = "Không tìm thấy phương tiện." });

            var areas = await _context.KhuVucs
                .Where(kv => kv.IDBaiXe == lotId && kv.IDLoaiXe == vehicle.IDLoaiXe)
                .ToListAsync();

            var areaIds = areas.Select(a => a.ID).ToList();
            var spots = await _context.ChoDauXes
                .Where(s => areaIds.Contains(s.IDKhuVuc))
                .ToListAsync();

            var result = areas.Select(kv => new
            {
                id = kv.ID,
                name = kv.TenKhuVuc,
                spots = spots.Where(s => s.IDKhuVuc == kv.ID).Select(s => new
                {
                    id = s.ID,
                    name = s.TenChoDau,
                    status = s.TrangThaiO // "Trống", "Đang đỗ", "Đã đặt", "Bảo trì"
                }).ToList()
            }).ToList();

            return Json(new { success = true, areas = result });
        }

        // POST: /Customer/Home/BookSpot
        [HttpPost]
        public async Task<IActionResult> BookSpot(int spotId, string bienSoXe, DateTime startTime, DateTime endTime)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (endTime <= startTime)
            {
                return Json(new { success = false, message = "Thời gian kết thúc phải sau thời gian bắt đầu." });
            }

            var vehicle = await _context.Xes.Include(x => x.LoaiXe).FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
            if (vehicle == null) return Json(new { success = false, message = "Không tìm thấy phương tiện đăng ký." });

            // Find specific selected spot
            var spot = await _context.ChoDauXes
                .Include(c => c.KhuVuc)
                .FirstOrDefaultAsync(c => c.ID == spotId && c.TrangThaiO == "Trống");

            if (spot == null)
            {
                return Json(new { success = false, message = "Chỗ đỗ bạn chọn không khả dụng hoặc đã bị người khác đặt." });
            }

            int lotId = spot.KhuVuc!.IDBaiXe;
            double durationHours = (endTime - startTime).TotalHours;
            if (durationHours <= 0)
            {
                return Json(new { success = false, message = "Thời gian thuê tối thiểu là 1 giờ." });
            }

            // Fetch price
            var price = await _context.BangGias
                .Where(bg => bg.IDBaiXe == lotId && bg.IDLoaiXe == vehicle.IDLoaiXe)
                .Select(bg => bg.GiaTheoGio)
                .FirstOrDefaultAsync();
            decimal cost = price * (decimal)durationHours;

            // Create booking
            var booking = new DatCho
            {
                IDKhachHang = customer.ID,
                IDChoDau = spot.ID,
                BienSoXe = bienSoXe,
                NgayDat = DateTime.Now,
                TgianBatDau = startTime,
                TgianKetThuc = endTime,
                TienCoc = cost,
                TrangThai = "Đã đặt"
            };

            // Update spot to booked
            spot.TrangThaiO = "Đã đặt";

            _context.Add(booking);
            await _context.SaveChangesAsync();

            return Json(new { success = true, bookingId = booking.ID, cost = cost.ToString("N0") + "đ" });
        }

        // GET: /Customer/Home/Vehicles
        public IActionResult Vehicles()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var vehicles = _context.KhachHangXes
                .Include(kx => kx.Xe)
                    .ThenInclude(x => x!.LoaiXe)
                .Where(kx => kx.IDKhachHang == customer.ID)
                .ToList();

            ViewBag.CustomerVehicles = vehicles;
            ViewBag.VehicleTypes = _context.LoaiXes.ToList();

            return View();
        }

        // POST: /Customer/Home/AddVehicle
        [HttpPost]
        public async Task<IActionResult> AddVehicle(AddVehicleViewModel model)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            // Save Vehicle
            var vehicle = new Xe
            {
                BienSoXe = model.BienSoXe.ToUpper(),
                IDLoaiXe = model.IDLoaiXe,
                TenXe = model.TenXe,
                Hang = model.Hang,
                MauSac = model.MauSac
            };

            // Save Relationship
            var relation = new KhachHang_Xe
            {
                IDKhachHang = customer.ID,
                IDXe = vehicle.BienSoXe,
                LoaiSoHuu = model.LoaiSoHuu
            };

            _context.Add(vehicle);
            _context.Add(relation);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: /Customer/Home/DeleteVehicle
        [HttpPost]
        public async Task<IActionResult> DeleteVehicle(string bienSoXe)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            var relation = await _context.KhachHangXes.FirstOrDefaultAsync(kx => kx.IDKhachHang == customer.ID && kx.IDXe == bienSoXe);
            if (relation != null)
            {
                _context.KhachHangXes.Remove(relation);
                
                // Also delete vehicle if no other customer owns it
                var otherOwners = await _context.KhachHangXes.AnyAsync(kx => kx.IDXe == bienSoXe && kx.IDKhachHang != customer.ID);
                if (!otherOwners)
                {
                    var vehicle = await _context.Xes.FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
                    if (vehicle != null) _context.Xes.Remove(vehicle);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không tìm thấy phương tiện." });
        }

        // GET: /Customer/Home/Chat
        public IActionResult Chat()
        {
            return View();
        }

        // GET: /Customer/Home/Complaints
        public IActionResult Complaints()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var complaints = _context.KhieuNais
                .Include(kn => kn.BaiXe)
                .Where(kn => kn.IDKhachHang == customer.ID)
                .OrderByDescending(kn => kn.NgayGui)
                .ToList();
            ViewBag.Complaints = complaints;

            // Load bookings for dropdown
            var bookings = _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(c => c!.KhuVuc)
                        .ThenInclude(kv => kv!.BaiXe)
                .Where(dc => dc.IDKhachHang == customer.ID)
                .OrderByDescending(dc => dc.NgayDat)
                .ToList();
            ViewBag.Bookings = bookings;

            return View();
        }

        // POST: /Customer/Home/SubmitComplaint
        [HttpPost]
        public async Task<IActionResult> SubmitComplaint(CreateComplaintViewModel model)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var booking = await _context.DatChos
                .Include(dc => dc.ChoDauXe)
                    .ThenInclude(cd => cd!.KhuVuc)
                .FirstOrDefaultAsync(dc => dc.ID == model.IDDatCho && dc.IDKhachHang == customer.ID);

            if (booking == null || booking.ChoDauXe == null || booking.ChoDauXe.KhuVuc == null)
            {
                return Json(new { success = false, message = "Đơn đặt chỗ không hợp lệ." });
            }

            var complaint = new KhieuNai
            {
                IDKhachHang = customer.ID,
                IDBaiXe = booking.ChoDauXe.KhuVuc.IDBaiXe,
                IDDatCho = model.IDDatCho,
                TieuDe = model.TieuDe,
                NoiDung = model.NoiDung,
                NgayGui = DateTime.Now,
                TrangThai = "Chờ xử lý"
            };

            _context.Add(complaint);
            await _context.SaveChangesAsync();

            return Json(new { success = true, complaintId = "KN-" + complaint.ID });
        }

        // GET: /Customer/Home/Profile
        public IActionResult Profile()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            ViewBag.Customer = customer;
            ViewBag.Provinces = _context.TinhThanhs.ToList();

            // Resolve full address path for default dropdown binding
            if (!string.IsNullOrEmpty(customer.MaXa))
            {
                var xa = _context.XaPhuongs
                    .Include(x => x.QuanHuyen)
                    .FirstOrDefault(x => x.MaXa == customer.MaXa);
                if (xa != null)
                {
                    ViewBag.CurrentXa = xa.MaXa;
                    ViewBag.CurrentHuyen = xa.MaHuyen;
                    ViewBag.CurrentTinh = xa.QuanHuyen?.MaTinh;
                    
                    ViewBag.Districts = _context.QuanHuyens.Where(q => q.MaTinh == xa.QuanHuyen!.MaTinh).ToList();
                    ViewBag.Wards = _context.XaPhuongs.Where(x => x.MaHuyen == xa.MaHuyen).ToList();
                }
            }

            return View();
        }

        // POST: /Customer/Home/UpdateProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            customer.HoTen = model.HoTen;
            customer.SDT = model.SDT;
            customer.Email = model.Email;
            customer.CCCD = model.CCCD;
            customer.BangLaiXe = model.BangLaiXe;
            customer.MaXa = model.MaXa;
            customer.DiaChiChiTiet = model.DiaChiChiTiet;

            _context.Update(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: /Customer/Home/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.ID == customer.IDTaiKhoan);
            if (account == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            if (account.MatKhau != model.OldPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });
            }

            account.MatKhau = model.NewPassword;
            _context.Update(account);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Cascading geographical dropdown APIs
        [HttpGet]
        public IActionResult GetDistricts(string provinceId)
        {
            var districts = _context.QuanHuyens
                .Where(q => q.MaTinh == provinceId)
                .Select(q => new { id = q.MaHuyen, name = q.TenHuyen })
                .ToList();
            return Json(districts);
        }

        [HttpGet]
        public IActionResult GetWards(string districtId)
        {
            var wards = _context.XaPhuongs
                .Where(w => w.MaHuyen == districtId)
                .Select(w => new { id = w.MaXa, name = w.TenXa })
                .ToList();
            return Json(wards);
        }
    }
}
