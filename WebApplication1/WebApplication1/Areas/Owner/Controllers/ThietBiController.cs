using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class ThietBiController : Controller
    {
        private readonly AppDbContext _context;

        public ThietBiController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOwnerId()
        {
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

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Sync Database Schema: Make IDDatCho nullable and add IDChoDau in LogDieuKhienBarrier table
            try
            {
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE LogDieuKhienBarrier ALTER COLUMN IDDatCho INT NULL;");
            }
            catch (Exception) {}
            try
            {
                await _context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LogDieuKhienBarrier') AND name = 'IDChoDau') ALTER TABLE LogDieuKhienBarrier ADD IDChoDau INT NULL;");
            }
            catch (Exception) {}

            int chuBaiId = GetChuBaiId(ownerId);

            var lots = await _context.BaiXes
                .Where(b => b.IDChuBai == chuBaiId)
                .ToListAsync();

            ViewBag.BaiXes = lots;

            return View();
        }

        [HttpGet("GetSpotsForLot")]
        public async Task<IActionResult> GetSpotsForLot(int lotId)
        {
            var spots = await _context.ChoDauXes
                .Include(s => s.KhuVuc)
                    .ThenInclude(k => k!.LoaiXe)
                .Where(s => s.KhuVuc!.IDBaiXe == lotId)
                .Select(s => new {
                    id = s.ID,
                    area = s.KhuVuc!.TenKhuVuc,
                    name = s.TenChoDau,
                    type = s.KhuVuc!.LoaiXe != null ? 
                        (s.KhuVuc.LoaiXe.TenLoaiXe == "Xe bán tải" ? "xebantai" :
                         s.KhuVuc.LoaiXe.TenLoaiXe == "Xe tải" ? "xetai" :
                         s.KhuVuc.LoaiXe.TenLoaiXe == "Xe buýt" ? "xebuyt" :
                         s.KhuVuc.LoaiXe.TenLoaiXe == "Xe máy" ? "xemay" : "oto") : "oto",
                    lockCode = s.MaSoKhoa,
                    lockState = s.TrangThaiKhoa ?? "Đóng"
                })
                .ToListAsync();

            return Json(spots);
        }

        [HttpGet("GetBookingsForSpot")]
        public async Task<IActionResult> GetBookingsForSpot(int spotId)
        {
            var bookings = await _context.DatChos
                .Include(d => d.KhachHang)
                .Where(d => d.IDChoDau == spotId)
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new {
                    id = d.ID,
                    plate = d.BienSoXe,
                    customer = d.KhachHang != null ? d.KhachHang.HoTen : "Khách vãng lai",
                    time = d.TgianBatDau.ToString("yyyy-MM-dd HH:mm") + " - " + d.TgianKetThuc.ToString("HH:mm"),
                    status = d.TrangThai
                })
                .ToListAsync();

            return Json(bookings);
        }

        [HttpGet("GetControlLogs")]
        public async Task<IActionResult> GetControlLogs(int lotId)
        {
            var logs = await _context.LogDieuKhienBarriers
                .Include(l => l.DatCho)
                    .ThenInclude(d => d!.ChoDauXe)
                .Include(l => l.ChoDauXe)
                .Include(l => l.TaiKhoan)
                .Where(l => (l.IDChoDau != null && l.ChoDauXe!.KhuVuc!.IDBaiXe == lotId) || 
                            (l.IDDatCho != null && l.DatCho!.ChoDauXe!.KhuVuc!.IDBaiXe == lotId))
                .OrderByDescending(l => l.ThoiGianLệnh)
                .Select(l => new {
                    id = $"CMD-{l.ID}",
                    time = l.ThoiGianLệnh.ToString("yyyy-MM-dd HH:mm:ss"),
                    spotName = l.ChoDauXe != null ? l.ChoDauXe.TenChoDau : (l.DatCho != null && l.DatCho.ChoDauXe != null ? l.DatCho.ChoDauXe.TenChoDau : "N/A"),
                    lockCode = l.ChoDauXe != null ? l.ChoDauXe.MaSoKhoa : (l.DatCho != null && l.DatCho.ChoDauXe != null ? l.DatCho.ChoDauXe.MaSoKhoa : "N/A"),
                    operatorName = l.TaiKhoan != null ? (l.TaiKhoan.VaiTro!.TenVaiTro == "Chủ bãi xe" ? "Chủ bãi xe (Web)" : l.TaiKhoan.TenDangNhap) : "Hệ thống",
                    action = l.HanhDong,
                    result = l.KetQua,
                    note = l.GhiChu
                })
                .ToListAsync();

            return Json(logs);
        }

        [HttpPost("ToggleBarrier")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBarrier(int spotId, string action, int? bookingId = null, string? note = null)
        {
            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            string resultStatus = action == "Open" ? "Mở" : "Đóng";
            spot.TrangThaiKhoa = resultStatus;
            _context.Update(spot);

            int ownerId = GetCurrentOwnerId();
            if (ownerId != 0)
            {
                int? finalBookingId = bookingId;
                if (!finalBookingId.HasValue)
                {
                    var lastBooking = await _context.DatChos
                        .Where(d => d.IDChoDau == spotId)
                        .OrderByDescending(d => d.ID)
                        .FirstOrDefaultAsync();
                    if (lastBooking != null)
                    {
                        finalBookingId = lastBooking.ID;
                    }
                }

                var log = new LogDieuKhienBarrier
                {
                    IDDatCho = finalBookingId,
                    IDChoDau = spotId,
                    IDTaiKhoan = ownerId,
                    ThoiGianLệnh = DateTime.Now,
                    HanhDong = action == "Open" ? "Mở khóa" : "Khóa lại",
                    KetQua = "Thành công",
                    GhiChu = string.IsNullOrEmpty(note) ? $"Chủ bãi điều khiển IoT Barrier {spot.MaSoKhoa} sang trạng thái {resultStatus}" : note
                };
                _context.LogDieuKhienBarriers.Add(log);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, newLockState = resultStatus });
        }

        [HttpPost("ResetBarrier")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetBarrier(int spotId, int? bookingId = null, string? note = null)
        {
            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            string oldStatus = spot.TrangThaiKhoa ?? "Đóng";
            string newStatus = oldStatus == "Lỗi" ? "Đóng" : oldStatus;
            spot.TrangThaiKhoa = newStatus;
            _context.Update(spot);

            int ownerId = GetCurrentOwnerId();
            if (ownerId != 0)
            {
                int? finalBookingId = bookingId;
                if (!finalBookingId.HasValue)
                {
                    var lastBooking = await _context.DatChos
                        .Where(d => d.IDChoDau == spotId)
                        .OrderByDescending(d => d.ID)
                        .FirstOrDefaultAsync();
                    if (lastBooking != null)
                    {
                        finalBookingId = lastBooking.ID;
                    }
                }

                var log = new LogDieuKhienBarrier
                {
                    IDDatCho = finalBookingId,
                    IDChoDau = spotId,
                    IDTaiKhoan = ownerId,
                    ThoiGianLệnh = DateTime.Now,
                    HanhDong = "Reset",
                    KetQua = "Thành công",
                    GhiChu = string.IsNullOrEmpty(note) ? $"Chủ bãi khởi động lại phần cứng thiết bị {spot.MaSoKhoa}." : note
                };
                _context.LogDieuKhienBarriers.Add(log);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, newLockState = newStatus });
        }

        [HttpPost("AddBarrier")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBarrier(int spotId, string lockCode)
        {
            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            // Check duplicate lockCode
            var isDuplicate = await _context.ChoDauXes.AnyAsync(s => s.MaSoKhoa == lockCode && s.ID != spotId);
            if (isDuplicate) return Json(new { success = false, message = "Mã khóa đã tồn tại. Vui lòng nhập mã khác." });

            spot.MaSoKhoa = lockCode;
            spot.TrangThaiKhoa = "Đóng";
            _context.Update(spot);

            // Log
            var lastBooking = await _context.DatChos
                .Where(d => d.IDChoDau == spotId)
                .OrderByDescending(d => d.ID)
                .FirstOrDefaultAsync();

            int ownerId = GetCurrentOwnerId();
            if (lastBooking != null && ownerId != 0)
            {
                var log = new LogDieuKhienBarrier
                {
                    IDDatCho = lastBooking.ID,
                    IDTaiKhoan = ownerId,
                    ThoiGianLệnh = DateTime.Now,
                    HanhDong = "Thêm mới",
                    KetQua = "Thành công",
                    GhiChu = $"Đăng ký thiết bị IoT {lockCode} tại vị trí {spot.TenChoDau}"
                };
                _context.LogDieuKhienBarriers.Add(log);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
