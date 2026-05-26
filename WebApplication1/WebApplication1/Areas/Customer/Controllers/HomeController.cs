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
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _hubContext;

        public HomeController(AppDbContext context, IConfiguration configuration, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
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
        public async Task<IActionResult> GetLotDetails(int lotId, string bienSoXe, DateTime? startTime, DateTime? endTime)
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

            // Calculate active window (default to current time if not provided)
            var start = startTime ?? DateTime.Now;
            var end = endTime ?? DateTime.Now.AddHours(3);

            // Determine overlapping bookings for these spots during the selected window
            var overlappingBookings = await _context.DatChos
                .Where(dc => areaIds.Contains(dc.ChoDauXe!.IDKhuVuc) 
                    && (dc.TrangThai == "Đang đỗ" || dc.TrangThai == "Đã đặt" || dc.TrangThai == "Chờ thanh toán")
                    && dc.TgianBatDau < end 
                    && dc.TgianKetThuc > start)
                .ToListAsync();

            var result = areas.Select(kv => new
            {
                id = kv.ID,
                name = kv.TenKhuVuc,
                spots = spots.Where(s => s.IDKhuVuc == kv.ID).Select(s => new
                {
                    id = s.ID,
                    name = s.TenChoDau,
                    status = s.TrangThaiO == "Bảo trì" ? "Bảo trì" :
                             (overlappingBookings.Any(ab => ab.IDChoDau == s.ID && ab.TrangThai == "Đang đỗ") ? "Đang đỗ" :
                              (overlappingBookings.Any(ab => ab.IDChoDau == s.ID && (ab.TrangThai == "Đã đặt" || ab.TrangThai == "Chờ thanh toán")) ? "Đã đặt" : "Trống"))
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
                .ThenInclude(kv => kv.BaiXe)
                .FirstOrDefaultAsync(c => c.ID == spotId && c.TrangThaiO != "Bảo trì");

            if (spot == null)
            {
                return Json(new { success = false, message = "Chỗ đỗ bạn chọn không tồn tại hoặc đang bảo trì." });
            }

            // Check if there are any overlapping bookings for this spot during the selected range
            var hasOverlap = await _context.DatChos
                .AnyAsync(dc => dc.IDChoDau == spotId 
                    && (dc.TrangThai == "Đang đỗ" || dc.TrangThai == "Đã đặt" || dc.TrangThai == "Chờ thanh toán")
                    && dc.TgianBatDau < endTime 
                    && dc.TgianKetThuc > startTime);

            if (hasOverlap)
            {
                return Json(new { success = false, message = "Chỗ đỗ bạn chọn đã có người đặt trong khoảng thời gian này." });
            }

            var lot = spot.KhuVuc!.BaiXe!;
            double durationHours = (endTime - startTime).TotalHours;
            if (durationHours <= 0)
            {
                return Json(new { success = false, message = "Thời gian thuê tối thiểu là 1 giờ." });
            }

            // Fetch price
            var price = await _context.BangGias
                .Where(bg => bg.IDBaiXe == lot.ID && bg.IDLoaiXe == vehicle.IDLoaiXe)
                .Select(bg => bg.GiaTheoGio)
                .FirstOrDefaultAsync();
            decimal cost = Math.Round(price * (decimal)durationHours, 2);

            // Create booking in pending status ("Đã đặt" fits SQL constraints, payment is tracked via HoaDon/ThanhToan)
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

            // Update spot to booked (locks the spot during transaction)
            spot.TrangThaiO = "Đã đặt";

            _context.Add(booking);
            await _context.SaveChangesAsync();

            // Create Invoice in pending status ("Chưa thanh toán")
            decimal chietKhauAdmin = Math.Round(cost * (lot.PhanTramChietKhau / 100), 2);
            decimal tienChuBai = cost - chietKhauAdmin;

            var invoice = new HoaDon
            {
                IDDatCho = booking.ID,
                TongTien = cost,
                TienChietKhauAdmin = chietKhauAdmin,
                TienChuBaiNhan = tienChuBai,
                NgayTao = DateTime.Now,
                TrangThai = "Chưa thanh toán"
            };
            _context.Add(invoice);
            await _context.SaveChangesAsync();

            // Create Payment in pending status
            var payment = new ThanhToan
            {
                IDHoaDon = invoice.ID,
                PhuongThuc = "VNPAY",
                SoTien = cost,
                TrangThai = false, // Unpaid
                NgayThanhToan = DateTime.Now
            };
            _context.Add(payment);
            await _context.SaveChangesAsync();

            // Build VNPAY Payment Link
            var vnpay = new Services.VnPayLibrary();
            var tmnCode = _configuration["Vnpay:TmnCode"]?.Trim();
            var hashSecret = _configuration["Vnpay:HashSecret"]?.Trim();
            var baseUrl = _configuration["Vnpay:BaseUrl"]?.Trim();
            var returnUrl = _configuration["Vnpay:ReturnUrl"]?.Trim();

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl))
            {
                return Json(new { success = false, message = "Lỗi cấu hình cổng thanh toán VNPAY." });
            }

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(cost * 100)).ToString()); // Amount multiplied by 100
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            
            // Get client IP address securely and ensure IPv4 format
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddress == "::1" || string.IsNullOrEmpty(ipAddress) || ipAddress.Contains(":"))
            {
                ipAddress = "127.0.0.1";
            }
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            
            // Normalize OrderInfo to safe plain English characters to avoid signature mismatch
            string orderInfo = $"Thanh toan dat cho do xe {spot.TenChoDau} tai bai {lot.ID}";
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", payment.ID.ToString()); // Use Payment ID as VNPAY reference

            string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);

            System.Console.WriteLine("======================== DEBUG VNPAY REQUEST ========================");
            System.Console.WriteLine("Base URL: " + baseUrl);
            System.Console.WriteLine("TmnCode: " + tmnCode);
            System.Console.WriteLine("HashSecret: " + hashSecret);
            System.Console.WriteLine("Parameters to sign: " + string.Join("&", vnpay.GetRequestData().Select(kv => $"{kv.Key}={kv.Value}")));
            System.Console.WriteLine("Generated Redirect URL: " + paymentUrl);
            System.Console.WriteLine("=====================================================================");

            return Json(new { success = true, paymentUrl = paymentUrl });
        }

        // GET: /Customer/Home/VnpayReturn
        [HttpGet]
        public async Task<IActionResult> VnpayReturn()
        {
            var hashSecret = _configuration["Vnpay:HashSecret"];
            if (string.IsNullOrEmpty(hashSecret))
            {
                TempData["ErrorMessage"] = "Lỗi cấu hình khóa bí mật VNPAY.";
                return RedirectToAction("Bookings");
            }

            var vnpay = new Services.VnPayLibrary();
            foreach (var key in Request.Query.Keys)
            {
                var value = Request.Query[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    if (key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                    {
                        vnpay.AddResponseData(key, value);
                    }
                }
            }

            // Extract essential parameters
            string txnRefVal = vnpay.GetResponseData("vnp_TxnRef");
            if (string.IsNullOrEmpty(txnRefVal) || !int.TryParse(txnRefVal, out int paymentId))
            {
                TempData["ErrorMessage"] = "Mã tham chiếu thanh toán không hợp lệ.";
                return RedirectToAction("Bookings");
            }

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();

            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

            if (isValidSignature)
            {
                var payment = await _context.ThanhToans
                    .Include(t => t.HoaDon)
                    .ThenInclude(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                    .FirstOrDefaultAsync(t => t.ID == paymentId);

                if (payment != null)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        // Success!
                        payment.TrangThai = true;
                        payment.MaGiaoDich = vnp_TransactionNo;
                        payment.NgayThanhToan = DateTime.Now;

                        payment.HoaDon.TrangThai = "Đã thanh toán";
                        payment.HoaDon.DatCho.TrangThai = "Đã đặt";
                        payment.HoaDon.DatCho.ChoDauXe.TrangThaiO = "Đã đặt";

                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Thanh toán thành công hóa đơn #{payment.HoaDon.ID} qua VNPAY!";
                    }
                    else
                    {
                        // Failed or Cancelled - Release spot!
                        payment.TrangThai = false;
                        payment.HoaDon.TrangThai = "Chưa thanh toán";
                        payment.HoaDon.DatCho.TrangThai = "Đã hủy";
                        payment.HoaDon.DatCho.ChoDauXe.TrangThaiO = "Trống"; // Set spot back to vacant!

                        await _context.SaveChangesAsync();

                        TempData["ErrorMessage"] = $"Giao dịch VNPAY thất bại (Mã lỗi: {vnp_ResponseCode}). Chỗ đỗ đã được giải phóng.";
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Chữ ký bảo mật VNPAY không hợp lệ.";
            }

            return RedirectToAction("Bookings");
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

        // GET: /Customer/Home/Chat
        [HttpGet]
        public async Task<IActionResult> Chat(int? ownerId, int? activeChatId)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customerAccountId = customer.IDTaiKhoan;

            // If ownerId is provided, check or create a session
            if (ownerId.HasValue)
            {
                var existingSession = await _context.PhienChats
                    .FirstOrDefaultAsync(pc => pc.IDKhachHang == customer.ID && pc.IDChuBai == ownerId.Value);

                if (existingSession == null)
                {
                    var newSession = new PhienChat
                    {
                        IDKhachHang = customer.ID,
                        IDChuBai = ownerId.Value,
                        NgayBatDau = DateTime.Now
                    };
                    _context.PhienChats.Add(newSession);
                    await _context.SaveChangesAsync();
                    activeChatId = newSession.ID;
                }
                else
                {
                    activeChatId = existingSession.ID;
                }

                // Redirect to the chat view with the active session ID to prevent resubmission
                return RedirectToAction("Chat", new { activeChatId = activeChatId });
            }

            // Load all chat sessions for this customer
            var chatSessions = await _context.PhienChats
                .Include(pc => pc.ChuBaiXe)
                .Include(pc => pc.TinNhans)
                .Where(pc => pc.IDKhachHang == customer.ID)
                .ToListAsync();

            // Find active chat session
            PhienChat? activeChatSession = null;
            if (activeChatId.HasValue)
            {
                activeChatSession = chatSessions.FirstOrDefault(pc => pc.ID == activeChatId.Value);
            }
            if (activeChatSession == null)
            {
                activeChatSession = chatSessions.FirstOrDefault();
            }

            // Load messages for the active session
            List<TinNhan> messages = new List<TinNhan>();
            if (activeChatSession != null)
            {
                messages = await _context.TinNhans
                    .Where(m => m.IDPhienChat == activeChatSession.ID)
                    .OrderBy(m => m.NgayGui)
                    .ToListAsync();

                // Mark unread messages from owner as read
                var unreadMessages = messages.Where(m => m.IDTaiKhoanGui != customerAccountId && !m.DaDoc).ToList();
                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.DaDoc = true;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.ChatSessions = chatSessions;
            ViewBag.ActiveSession = activeChatSession;
            ViewBag.Messages = messages;
            ViewBag.UserAccountId = customerAccountId;

            return View();
        }

        // POST: /Customer/Home/SendChatMessage
        [HttpPost]
        public async Task<IActionResult> SendChatMessage(int sessionId, string content)
        {
            var customer = GetCurrentCustomer();
            if (customer == null || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });
            }

            var session = await _context.PhienChats.FirstOrDefaultAsync(pc => pc.ID == sessionId && pc.IDKhachHang == customer.ID);
            if (session == null)
            {
                return Json(new { success = false, message = "Phiên chat không tồn tại." });
            }

            var message = new TinNhan
            {
                IDPhienChat = sessionId,
                IDTaiKhoanGui = customer.IDTaiKhoan,
                NoiDung = content,
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _context.TinNhans.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast the message via SignalR to all users in this session group
            await _hubContext.Clients.Group($"Session_{sessionId}").SendAsync("ReceiveMessage", new
            {
                id = message.ID,
                senderId = message.IDTaiKhoanGui,
                content = message.NoiDung,
                time = message.NgayGui.ToString("HH:mm")
            });

            return Json(new { success = true, messageId = message.ID, time = message.NgayGui.ToString("HH:mm") });
        }

        // GET: /Customer/Home/GetChatMessages
        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int sessionId)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var session = await _context.PhienChats.FirstOrDefaultAsync(pc => pc.ID == sessionId && pc.IDKhachHang == customer.ID);
            if (session == null)
            {
                return Json(new { success = false, message = "Phiên chat không tồn tại." });
            }

            var messages = await _context.TinNhans
                .Where(m => m.IDPhienChat == sessionId)
                .OrderBy(m => m.NgayGui)
                .Select(m => new
                {
                    id = m.ID,
                    senderId = m.IDTaiKhoanGui,
                    content = m.NoiDung,
                    time = m.NgayGui.ToString("HH:mm"),
                    isMe = m.IDTaiKhoanGui == customer.IDTaiKhoan
                })
                .ToListAsync();

            // Mark unread messages from owner as read
            var unreadMessages = await _context.TinNhans
                .Where(m => m.IDPhienChat == sessionId && m.IDTaiKhoanGui != customer.IDTaiKhoan && !m.DaDoc)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.DaDoc = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, messages = messages });
        }
    }
}
