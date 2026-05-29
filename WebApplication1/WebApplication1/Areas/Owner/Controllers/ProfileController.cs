using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using WebApplication1.Models.ViewModels;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Chủ bãi xe")]
    [Route("Owner/[controller]")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

        private ChuBaiXe? GetCurrentOwner()
        {
            int accountId = GetCurrentAccountId();
            return _context.ChuBaiXes
                .Include(c => c.TaiKhoan)
                .FirstOrDefault(c => c.IDTaiKhoan == accountId);
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var owner = GetCurrentOwner();
            if (owner == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            ViewBag.Owner = owner;
            ViewBag.Provinces = await _context.TinhThanhs.ToListAsync();

            // Resolve full address path for default dropdown binding
            if (!string.IsNullOrEmpty(owner.MaXa))
            {
                var xa = await _context.XaPhuongs
                    .Include(x => x.QuanHuyen)
                    .FirstOrDefaultAsync(x => x.MaXa == owner.MaXa);
                if (xa != null)
                {
                    ViewBag.CurrentXa = xa.MaXa;
                    ViewBag.CurrentHuyen = xa.MaHuyen;
                    ViewBag.CurrentTinh = xa.QuanHuyen?.MaTinh;
                    
                    ViewBag.Districts = await _context.QuanHuyens.Where(q => q.MaTinh == xa.QuanHuyen!.MaTinh).ToListAsync();
                    ViewBag.Wards = await _context.XaPhuongs.Where(x => x.MaHuyen == xa.MaHuyen).ToListAsync();
                }
            }

            return View();
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(UpdateOwnerProfileViewModel model)
        {
            var owner = GetCurrentOwner();
            if (owner == null) return Json(new { success = false, message = "Không tìm thấy thông tin chủ bãi xe." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            owner.TenChuBai = model.TenChuBai;
            owner.SDT = model.SDT;
            owner.Email = model.Email;
            owner.CCCD = model.CCCD;
            owner.MaXa = model.MaXa;
            owner.DiaChiChiTiet = model.DiaChiChiTiet;

            _context.Update(owner);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var owner = GetCurrentOwner();
            if (owner == null) return Json(new { success = false, message = "Không tìm thấy thông tin chủ bãi xe." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var account = owner.TaiKhoan;
            if (account == null) return Json(new { success = false, message = "Không tìm thấy tài khoản liên kết." });

            if (account.MatKhau != model.OldPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });
            }

            account.MatKhau = model.NewPassword;
            _context.Update(account);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            var owner = GetCurrentOwner();
            if (owner == null) return Json(new { success = false, message = "Không tìm thấy thông tin chủ bãi xe." });

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return Json(new { success = false, message = "Không có tệp tin nào được gửi." });
            }

            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                var account = owner.TaiKhoan;
                if (account != null)
                {
                    account.AnhDaiDien = "/uploads/" + uniqueFileName;
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, avatarUrl = account.AnhDaiDien });
                }
                return Json(new { success = false, message = "Tài khoản không tìm thấy." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet("GetDistricts")]
        public async Task<IActionResult> GetDistricts(string provinceId)
        {
            var districts = await _context.QuanHuyens
                .Where(q => q.MaTinh == provinceId)
                .Select(q => new { id = q.MaHuyen, name = q.TenHuyen })
                .ToListAsync();
            return Json(districts);
        }

        [HttpGet("GetWards")]
        public async Task<IActionResult> GetWards(string districtId)
        {
            var wards = await _context.XaPhuongs
                .Where(w => w.MaHuyen == districtId)
                .Select(w => new { id = w.MaXa, name = w.TenXa })
                .ToListAsync();
            return Json(wards);
        }
    }
}
