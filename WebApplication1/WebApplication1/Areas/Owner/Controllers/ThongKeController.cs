using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Generic;
using WebApplication1.Models.DataContext;
using System.Linq;
using WebApplication1.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using WebApplication1.Extensions;


namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            int ownerAccountId = GetCurrentOwnerId();
            if (ownerAccountId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            int chuBaiId = GetChuBaiId(ownerAccountId);

            // Auto-release expired bookings first
            await _context.AutoReleaseExpiredBookingsAsync();

            var baiXes = await _context.BaiXes
                .Where(b => b.IDChuBai == chuBaiId)
                .ToListAsync();

            ViewBag.BaiXes = baiXes;
            return View();
        }

        [HttpGet("GetThongKe")]
        public async Task<IActionResult> GetThongKe(int? baiXeId, string range = "30days")
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var taiKhoanId))
            {
                return Unauthorized();
            }

            // Auto-release expired bookings first
            await _context.AutoReleaseExpiredBookingsAsync();

            var chuBai = await _context.ChuBaiXes
                .Include(c => c.BaiXes)
                .FirstOrDefaultAsync(c => c.IDTaiKhoan == taiKhoanId);

            if (chuBai == null)
            {
                return Forbid();
            }

            var baiXeIds = chuBai.BaiXes.Select(b => b.ID).ToList();
            if (baiXeId.HasValue)
            {
                if (!baiXeIds.Contains(baiXeId.Value)) return Forbid();
                baiXeIds = new List<int> { baiXeId.Value };
            }

            var today = DateTime.Today;
            DateTime start;
            switch ((range ?? "30days").ToLower())
            {
                case "today": start = today; break;
                case "7days": start = today.AddDays(-6); break;
                case "30days": start = today.AddDays(-29); break;
                case "year": start = new DateTime(today.Year, 1, 1); break;
                default: start = today.AddDays(-29); break;
            }

            // Load invoices with related navigations (DatCho -> ChoDauXe -> KhuVuc -> BaiXe/LoaiXe)
            var hoaDons = await _context.HoaDons
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.BaiXe)
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.LoaiXe)
                .Include(h => h.ThanhToans)
                .Where(h => h.NgayTao >= start
                            && (h.ThanhToans.Any(t => t.TrangThai) || h.TrangThai == "Đã thanh toán")
                            && (h.DatCho != null && h.DatCho.ChoDauXe != null && h.DatCho.ChoDauXe.KhuVuc != null && baiXeIds.Contains(h.DatCho.ChoDauXe.KhuVuc.IDBaiXe)))
                .ToListAsync();

            // Map license plates to Xe with LoaiXe (DatCho doesn't have Xe nav property)
            var plateNumbers = hoaDons
                .Select(h => h.DatCho?.BienSoXe)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            var xeMap = new Dictionary<string, string>(); // plate -> TenLoai
            if (plateNumbers.Any())
            {
                var xes = await _context.Xes
                    .Include(x => x.LoaiXe)
                    .Where(x => plateNumbers.Contains(x.BienSoXe))
                    .ToListAsync();

                foreach (var xe in xes)
                {
                    xeMap[xe.BienSoXe] = xe.LoaiXe?.TenLoaiXe ?? string.Empty;
                }
            }

            var tongDoanhThu = hoaDons.Sum(x => x.TienChuBaiNhan);
            var luotDoXe = hoaDons.Count;
            var doanhThuTB = luotDoXe > 0 ? tongDoanhThu / luotDoXe : 0;

            var totalSpotsCount = await _context.ChoDauXes
                .CountAsync(c => baiXeIds.Contains(c.KhuVuc!.IDBaiXe));
            var occupiedSpotsCount = await _context.ChoDauXes
                .CountAsync(c => baiXeIds.Contains(c.KhuVuc!.IDBaiXe) && (c.TrangThaiO == "Đang đỗ" || c.TrangThaiO == "Đã đặt"));
            var occupancyRate = totalSpotsCount > 0 ? (double)occupiedSpotsCount / totalSpotsCount * 100 : 0;

            // Transactions payload
            var giaoDich = hoaDons
                .OrderByDescending(x => x.NgayTao)
                .Take(50)
                .Select(x => {
                    var bien = x.DatCho?.BienSoXe;
                    var typeName = string.Empty;
                    if (!string.IsNullOrEmpty(bien) && xeMap.ContainsKey(bien))
                    {
                        typeName = xeMap[bien];
                    }
                    else
                    {
                        typeName = x.DatCho?.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? string.Empty;
                    }

                    var typeKey = "oto";
                    if (typeName.Contains("bán tải", StringComparison.OrdinalIgnoreCase)) typeKey = "xebantai";
                    else if (typeName.Contains("buýt", StringComparison.OrdinalIgnoreCase) || typeName.Contains("bus", StringComparison.OrdinalIgnoreCase)) typeKey = "xebuyt";
                    else if (typeName.Contains("tải", StringComparison.OrdinalIgnoreCase)) typeKey = "xetai";
                    else if (typeName.Contains("máy", StringComparison.OrdinalIgnoreCase)) typeKey = "xemay";
                    else if (typeName.Contains("đạp", StringComparison.OrdinalIgnoreCase)) typeKey = "xedapdien";

                    return new
                    {
                        id = x.ID,
                        bienSo = bien,
                        lotName = x.DatCho?.ChoDauXe?.KhuVuc?.BaiXe?.TenBai ?? string.Empty,
                        spotName = x.DatCho?.ChoDauXe?.TenChoDau ?? string.Empty,
                        type = typeKey,
                        tongTien = x.TongTien,
                        thoiGian = x.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        trangThai = x.TrangThai
                    };
                })
                .ToList();

            // Chart aggregation
            List<string> chartLabels = new();
            List<decimal> chartData = new();

            if ((range ?? "30days").ToLower() == "today")
            {
                // hourly
                for (int h = 0; h < 24; h++)
                {
                    var hourStart = today.AddHours(h);
                    var sum = hoaDons.Where(x => x.NgayTao.Date == today && x.NgayTao.Hour == h).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add(h.ToString("D2") + ":00");
                    chartData.Add(sum);
                }
            }
            else if ((range ?? "30days").ToLower() == "7days")
            {
                for (int i = 0; i < 7; i++)
                {
                    var d = start.AddDays(i);
                    var sum = hoaDons.Where(x => x.NgayTao.Date == d.Date).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add(d.ToString("ddd", new CultureInfo("vi-VN")));
                    chartData.Add(sum);
                }
            }
            else if ((range ?? "30days").ToLower() == "30days")
            {
                // 4 weekly buckets
                for (int w = 0; w < 4; w++)
                {
                    var weekStart = start.AddDays(w * 7);
                    var weekEnd = weekStart.AddDays(7).AddTicks(-1);
                    var sum = hoaDons.Where(x => x.NgayTao >= weekStart && x.NgayTao <= weekEnd).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add($"Tuần {w + 1}");
                    chartData.Add(sum);
                }
            }
            else // year
            {
                for (int m = 1; m <= 12; m++)
                {
                    var monthStart = new DateTime(today.Year, m, 1);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                    var sum = hoaDons.Where(x => x.NgayTao >= monthStart && x.NgayTao <= monthEnd).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add("Tháng " + m);
                    chartData.Add(sum);
                }
            }

            // Vehicle breakdown by LoaiXe
            var breakdown = hoaDons
                .Select(h => {
                    var bien = h.DatCho?.BienSoXe;
                    var t = "Khác";
                    if (!string.IsNullOrEmpty(bien) && xeMap.ContainsKey(bien)) t = xeMap[bien];
                    else t = h.DatCho?.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? "Khác";
                    return new { type = t, value = h.TienChuBaiNhan };
                })
                .GroupBy(x => x.type)
                .Select(g => new { type = g.Key ?? "Khác", value = g.Sum(x => x.value) })
                .OrderByDescending(x => x.value)
                .ToList();

            var doughnutLabels = breakdown.Select(b => b.type).ToList();
            var doughnutData = breakdown.Select(b => (decimal)b.value).ToList();

            return Json(new
            {
                tongDoanhThu,
                luotDoXe,
                doanhThuTB,
                giaoDich,
                chartLabels,
                chartData,
                doughnutLabels,
                doughnutData,
                hieuSuat = Math.Round(occupancyRate, 0)
            });
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

        [HttpGet("GetInvoiceDetails")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0) return Json(new { success = false, message = "Chưa đăng nhập." });

            int chuBaiId = GetChuBaiId(ownerId);
            if (chuBaiId == 0) return Json(new { success = false, message = "Không tìm thấy thông tin chủ bãi." });

            // Load invoice with related data
            var invoice = await _context.HoaDons
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.BaiXe)
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.LoaiXe)
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.KhachHang)
                .Include(h => h.ThanhToans)
                .FirstOrDefaultAsync(h => h.ID == id);

            if (invoice == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

            // Security Check: Verify if this spot belongs to one of the owner's parking lots
            var spot = invoice.DatCho?.ChoDauXe;
            if (spot == null || spot.KhuVuc?.BaiXe?.IDChuBai != chuBaiId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xem hóa đơn này." });
            }

            var payment = invoice.ThanhToans.OrderByDescending(p => p.ID).FirstOrDefault();
            var paymentMethod = payment?.PhuongThuc ?? "VNPAY";

            // Map license plate to vehicle model type name
            string vehicleTypeName = string.Empty;
            var bien = invoice.DatCho?.BienSoXe;
            if (!string.IsNullOrEmpty(bien))
            {
                var xe = await _context.Xes
                    .Include(x => x.LoaiXe)
                    .FirstOrDefaultAsync(x => x.BienSoXe == bien);
                if (xe != null)
                {
                    vehicleTypeName = xe.LoaiXe?.TenLoaiXe ?? string.Empty;
                }
            }
            if (string.IsNullOrEmpty(vehicleTypeName))
            {
                vehicleTypeName = invoice.DatCho?.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? "Ô tô con";
            }

            return Json(new
            {
                success = true,
                invoiceId = invoice.ID,
                bookingId = invoice.IDDatCho,
                customerName = invoice.DatCho?.KhachHang?.HoTen ?? "Khách vãng lai",
                customerPhone = invoice.DatCho?.KhachHang?.SDT ?? "Chưa có",
                customerEmail = invoice.DatCho?.KhachHang?.Email ?? "Chưa có",
                lotName = spot.KhuVuc?.BaiXe?.TenBai ?? "Bãi xe",
                spotName = spot.TenChoDau ?? "Vị trí",
                areaName = spot.KhuVuc?.TenKhuVuc ?? "Khu",
                licensePlate = bien ?? "Chưa rõ",
                vehicleType = vehicleTypeName,
                startTime = invoice.DatCho?.TgianBatDau.ToString("dd/MM/yyyy HH:mm"),
                endTime = invoice.DatCho?.TgianKetThuc.ToString("dd/MM/yyyy HH:mm"),
                createdTime = invoice.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                totalAmount = invoice.TongTien,
                feeAdmin = invoice.TienChietKhauAdmin,
                ownerAmount = invoice.TienChuBaiNhan,
                paymentStatus = invoice.TrangThai,
                paymentMethod = paymentMethod
            });
        }
    }
}
