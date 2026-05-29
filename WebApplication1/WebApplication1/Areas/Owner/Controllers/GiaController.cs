using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class GiaController : Controller
    {
        private readonly AppDbContext _context;

        public GiaController(AppDbContext context)
        {
            _context = context;
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

            var baiXes = await _context.BaiXes
                .Where(b => b.IDChuBai == chuBaiId && b.TrangThai != "Từ chối")
                .Select(b => new { b.ID, b.TenBai, b.TrangThai })
                .ToListAsync();

            ViewBag.BaiXes = baiXes;
            return View();
        }

        [HttpGet("GetAreas")]
        public async Task<IActionResult> GetAreas(int lotId)
        {
            var areas = await _context.KhuVucs
                .Include(k => k.LoaiXe)
                .Where(k => k.IDBaiXe == lotId)
                .Select(k => new { 
                    value = k.ID, 
                    label = k.TenKhuVuc,
                    type = k.IDLoaiXe,
                    typeName = k.LoaiXe != null ? k.LoaiXe.TenLoaiXe : "Khác"
                })
                .ToListAsync();
            return Json(areas);
        }

        [HttpGet("GetLoaiXes")]
        public async Task<IActionResult> GetLoaiXes()
        {
            var types = await _context.LoaiXes
                .Select(l => new { value = l.ID, label = l.TenLoaiXe })
                .ToListAsync();
            return Json(types);
        }

        [HttpGet("GetPricing")]
        public async Task<IActionResult> GetPricing(int lotId, int loaiXeId)
        {
            var pricing = await _context.BangGias
                .FirstOrDefaultAsync(b => b.IDBaiXe == lotId && b.IDLoaiXe == loaiXeId);
            
            if (pricing == null)
            {
                return Json(new { 
                    exists = false,
                    giaTheoGio = 0,
                    giaQuaDem = 0,
                    giaTheoThang = 0,
                    giaDatCho = 10,
                    trangThai = true
                });
            }

            return Json(new {
                exists = true,
                giaTheoGio = pricing.GiaTheoGio,
                giaQuaDem = pricing.GiaQuaDem,
                giaTheoThang = pricing.GiaTheoThang,
                giaDatCho = pricing.GiaDatCho,
                trangThai = pricing.TrangThai
            });
        }

        [HttpGet("GetPricingLogs")]
        public async Task<IActionResult> GetPricingLogs()
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId == 0) return Json(new List<object>());

            int chuBaiId = GetChuBaiId(ownerId);

            var list = await _context.BangGias
                .Include(b => b.BaiXe)
                .Include(b => b.LoaiXe)
                .Where(b => b.BaiXe!.IDChuBai == chuBaiId)
                .OrderByDescending(b => b.ID)
                .Select(b => new {
                    lotName = b.BaiXe!.TenBai,
                    vehicleLabel = b.LoaiXe != null ? b.LoaiXe.TenLoaiXe : "Khác",
                    giaTheoGio = b.GiaTheoGio,
                    giaQuaDem = b.GiaQuaDem,
                    giaTheoThang = b.GiaTheoThang,
                    giaDatCho = b.GiaDatCho,
                    trangThai = b.TrangThai
                })
                .ToListAsync();

            return Json(list);
        }

        [HttpPost("SavePricing")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePricing(int lotId, int loaiXeId, decimal giaTheoGio, decimal giaQuaDem, decimal giaTheoThang, decimal giaDatCho, bool trangThai)
        {
            var baiXe = await _context.BaiXes.FindAsync(lotId);
            if (baiXe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bãi xe." });
            }
            if (baiXe.TrangThai != "Hoạt động")
            {
                return Json(new { success = false, message = "Bãi xe chưa được duyệt hoặc đang ngưng hoạt động. Không thể cập nhật giá." });
            }

            var pricing = await _context.BangGias
                .FirstOrDefaultAsync(b => b.IDBaiXe == lotId && b.IDLoaiXe == loaiXeId);

            var isNew = false;
            if (pricing == null)
            {
                isNew = true;
                var loaiXe = await _context.LoaiXes.FindAsync(loaiXeId);
                var tenLoai = loaiXe?.TenLoaiXe ?? "Khác";
                var tenBai = baiXe.TenBai;

                pricing = new BangGia
                {
                    IDBaiXe = lotId,
                    IDLoaiXe = loaiXeId,
                    TenBangGia = $"Bảng giá {tenLoai} - {tenBai}",
                    GiaTheoGio = giaTheoGio,
                    GiaQuaDem = giaQuaDem,
                    GiaTheoThang = giaTheoThang,
                    GiaDatCho = giaDatCho,
                    TrangThai = trangThai
                };
                _context.BangGias.Add(pricing);
            }
            else
            {
                pricing.GiaTheoGio = giaTheoGio;
                pricing.GiaQuaDem = giaQuaDem;
                pricing.GiaTheoThang = giaTheoThang;
                pricing.GiaDatCho = giaDatCho;
                pricing.TrangThai = trangThai;
                _context.BangGias.Update(pricing);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isNew = isNew });
        }
    }
}
