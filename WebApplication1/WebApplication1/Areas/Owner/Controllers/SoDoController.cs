using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Areas.Owner.Models;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using WebApplication1.Extensions;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class SoDoController : Controller
    {
        private readonly AppDbContext _context;

        public SoDoController(AppDbContext context)
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

            int chuBaiId = GetChuBaiId(ownerId);

            // Auto-release expired bookings first
            await _context.AutoReleaseExpiredBookingsAsync();

            var lots = await _context.BaiXes
                .Where(b => b.IDChuBai == chuBaiId && b.TrangThai != "Từ chối")
                .ToListAsync();

            var lotIds = lots.Select(l => l.ID).ToList();

            var spots = await _context.ChoDauXes
                .Include(s => s.KhuVuc)
                    .ThenInclude(k => k!.LoaiXe)
                .Where(s => lotIds.Contains(s.KhuVuc!.IDBaiXe))
                .ToListAsync();

            var bookings = await _context.DatChos
                .Include(d => d.KhachHang)
                .Where(d => lotIds.Contains(d.ChoDauXe!.KhuVuc!.IDBaiXe) && (d.TrangThai == "Đang đỗ" || d.TrangThai == "Đã đặt"))
                .ToListAsync();

            var spotsData = lots.ToDictionary(
                lot => lot.ID.ToString(),
                lot => spots
                    .Where(s => s.KhuVuc!.IDBaiXe == lot.ID)
                    .Select(s => {
                        var activeBooking = bookings.FirstOrDefault(b => b.IDChoDau == s.ID);
                        return new {
                            id = s.ID,
                            area = s.KhuVuc!.TenKhuVuc,
                            name = s.TenChoDau,
                            type = s.KhuVuc.LoaiXe?.TenLoaiXe switch
                            {
                                "Xe bán tải" => "xebantai",
                                "Xe tải" => "xetai",
                                "Xe buýt" => "xebuyt",
                                "Xe máy" => "xemay",
                                _ => "oto"
                            },
                            size = s.KichThuoc ?? "5x2.5m",
                            lockCode = s.MaSoKhoa ?? $"LOCK-{s.ID}",
                            lockState = s.TrangThaiKhoa,
                            occupancyState = s.TrangThaiO,
                            plate = activeBooking?.BienSoXe ?? "",
                            bookingCode = activeBooking != null ? $"#DC{activeBooking.ID}" : "",
                            customerName = activeBooking?.KhachHang?.HoTen ?? "",
                            customerPhone = activeBooking?.KhachHang?.SDT ?? "",
                            rentalTime = activeBooking != null ? $"{activeBooking.TgianBatDau:HH:mm} - {activeBooking.TgianKetThuc:HH:mm} ({activeBooking.NgayDat:dd/MM/yyyy})" : ""
                        };
                    })
                    .ToList()
            );

            ViewBag.SpotsJson = System.Text.Json.JsonSerializer.Serialize(spotsData);
            ViewBag.BaiXes = lots;

            return View();
        }

        [HttpPost("ToggleLock")]
        public async Task<IActionResult> ToggleLock(int spotId)
        {
            await _context.AutoReleaseExpiredBookingsAsync();

            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            spot.TrangThaiKhoa = spot.TrangThaiKhoa == "Mở" ? "Đóng" : "Mở";
            _context.Update(spot);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newLockState = spot.TrangThaiKhoa });
        }

        [HttpPost("ReportError")]
        public async Task<IActionResult> ReportError(int spotId)
        {
            await _context.AutoReleaseExpiredBookingsAsync();

            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            spot.TrangThaiKhoa = "Lỗi";
            spot.TrangThaiO = "Bảo trì";
            _context.Update(spot);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("ToggleMaintenance")]
        public async Task<IActionResult> ToggleMaintenance(int spotId)
        {
            await _context.AutoReleaseExpiredBookingsAsync();

            var spot = await _context.ChoDauXes.FindAsync(spotId);
            if (spot == null) return Json(new { success = false, message = "Không tìm thấy chỗ đỗ." });

            if (spot.TrangThaiO == "Bảo trì")
            {
                spot.TrangThaiO = "Trống";
                spot.TrangThaiKhoa = "Đóng";
            }
            else
            {
                spot.TrangThaiO = "Bảo trì";
                spot.TrangThaiKhoa = "Lỗi";
            }
            _context.Update(spot);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newOccupancyState = spot.TrangThaiO, newLockState = spot.TrangThaiKhoa });
        }

        private string GetLotNameInitials(string lotName)
        {
            if (string.IsNullOrWhiteSpace(lotName)) return "XX";
            var words = lotName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "XX";
            if (words.Length == 1)
            {
                var word = words[0];
                return word.Length >= 2 ? word.Substring(0, 2).ToUpper() : word.ToUpper() + "X";
            }
            var lastWord = words[words.Length - 1];
            var secondLastWord = words[words.Length - 2];
            char firstChar = string.IsNullOrEmpty(secondLastWord) ? 'X' : secondLastWord[0];
            char secondChar = string.IsNullOrEmpty(lastWord) ? 'X' : lastWord[0];
            string initials = $"{firstChar}{secondChar}".ToUpper();
            return initials.Replace("Đ", "D");
        }

        [HttpPost("AddArea")]
        public async Task<IActionResult> AddArea(AddAreaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            // Check parking lot exists and is active
            var baiXe = await _context.BaiXes.FindAsync(model.LotId);
            if (baiXe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bãi xe." });
            }
            if (baiXe.TrangThai != "Hoạt động")
            {
                return Json(new { success = false, message = $"Bãi xe đang ở trạng thái '{baiXe.TrangThai}'. Chỉ có thể thêm khu vực khi bãi xe đang hoạt động." });
            }

            // Check if the new spots would exceed the parking lot's total capacity
            var currentSpotsCount = await _context.ChoDauXes
                .CountAsync(c => c.KhuVuc!.IDBaiXe == model.LotId);
            if (currentSpotsCount + model.SpotCount > baiXe.SucChua)
            {
                return Json(new { 
                    success = false, 
                    message = $"Không thể thêm chỗ đỗ. Tổng số chỗ đỗ hiện tại ({currentSpotsCount}) cộng thêm số chỗ đỗ mới ({model.SpotCount}) sẽ vượt quá sức chứa tối đa của bãi xe ({baiXe.SucChua} chỗ)." 
                });
            }

            // Check if area already exists for this lot
            var existingKhuVuc = await _context.KhuVucs
                .Include(k => k.ChoDauXes)
                .Include(k => k.LoaiXe)
                .FirstOrDefaultAsync(k => k.IDBaiXe == model.LotId && k.TenKhuVuc.ToLower() == model.AreaName.ToLower());

            // Find appropriate LoaiXe
            string loaiXeName = model.VehicleType switch
            {
                "xebantai" => "Xe bán tải",
                "xetai" => "Xe tải",
                "xebuyt" => "Xe buýt",
                "xemay" => "Xe máy",
                _ => "Ô tô"
            };
            var loaiXe = await _context.LoaiXes.FirstOrDefaultAsync(l => l.TenLoaiXe == loaiXeName)
                         ?? await _context.LoaiXes.FirstOrDefaultAsync(l => l.TenLoaiXe.Contains(loaiXeName))
                         ?? await _context.LoaiXes.FirstOrDefaultAsync();

            if (loaiXe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy loại xe hợp lệ trong CSDL." });
            }

            KhuVuc targetKhuVuc;
            int startNumber = 1;

            if (existingKhuVuc != null)
            {
                // Verify vehicle type matches
                if (existingKhuVuc.IDLoaiXe != loaiXe.ID)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Khu vực '{model.AreaName}' đã tồn tại nhưng dành cho loại xe khác ({existingKhuVuc.LoaiXe?.TenLoaiXe ?? "Khác"}). Vui lòng đặt tên khu vực khác." 
                    });
                }

                // If it matches, we append spots to the existing area
                targetKhuVuc = existingKhuVuc;
                int currentSpotCount = existingKhuVuc.ChoDauXes.Count;
                startNumber = currentSpotCount + 1;
                
                // Update capacity
                targetKhuVuc.SucChua += model.SpotCount;
                _context.KhuVucs.Update(targetKhuVuc);
            }
            else
            {
                // Create new KhuVuc
                targetKhuVuc = new KhuVuc
                {
                    IDBaiXe = model.LotId,
                    IDLoaiXe = loaiXe.ID,
                    TenKhuVuc = model.AreaName,
                    SucChua = model.SpotCount
                };
                _context.KhuVucs.Add(targetKhuVuc);
            }

            await _context.SaveChangesAsync();

            // Create ChoDauXe records
            string lotPrefix = GetLotNameInitials(baiXe.TenBai);
            string size = "5x2.5m";
            if (model.VehicleType == "xebantai") size = "5.5x2.6m";
            else if (model.VehicleType == "xebuyt") size = "12x3.0m";
            else if (model.VehicleType == "xetai") size = "8x2.8m";

            int endNumber = startNumber + model.SpotCount - 1;
            for (int i = startNumber; i <= endNumber; i++)
            {
                string spotNum = i < 10 ? $"0{i}" : $"{i}";
                var newSpot = new ChoDauXe
                {
                    IDKhuVuc = targetKhuVuc.ID,
                    TenChoDau = $"{model.AreaName}-{spotNum}",
                    KichThuoc = size,
                    MaSoKhoa = $"LOCK-{lotPrefix}-{model.AreaName}{spotNum}",
                    TrangThaiKhoa = "Đóng",
                    TrangThaiO = "Trống"
                };
                _context.ChoDauXes.Add(newSpot);
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
