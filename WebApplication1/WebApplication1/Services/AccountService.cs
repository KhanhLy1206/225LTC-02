using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;
using WebApplication1.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterCustomerAsync(RegisterViewModel model)
        {
            // 1. Get or create 'Khách hàng' role
            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == "Khách hàng");
            if (role == null)
            {
                role = new VaiTro { TenVaiTro = "Khách hàng" };
                _context.VaiTros.Add(role);
                await _context.SaveChangesAsync();
            }

            // 2. Hash password with SHA256
            string hashedPassword = HashPassword(model.MatKhau);

            // 3. Create TaiKhoan record
            var taiKhoan = new TaiKhoan
            {
                IDVaiTro = role.ID,
                TenDangNhap = model.TenDangNhap,
                MatKhau = hashedPassword,
                TrangThai = true
            };

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // 4. Create KhachHang record linked to the TaiKhoan
            var khachHang = new KhachHang
            {
                IDTaiKhoan = taiKhoan.ID,
                HoTen = model.HoTen,
                SDT = model.SDT,
                Email = model.Email
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TaiKhoan?> ValidateLoginAsync(string usernameOrEmail, string password)
        {
            var input = usernameOrEmail.ToLower().Trim();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.VaiTro)
                .Include(t => t.KhachHang)
                .Include(t => t.ChuBaiXe)
                .Include(t => t.Admin)
                .FirstOrDefaultAsync(t =>
                    t.TrangThai &&
                    (t.TenDangNhap.ToLower() == input ||
                     (t.KhachHang != null && t.KhachHang.Email != null && t.KhachHang.Email.ToLower() == input) ||
                     (t.ChuBaiXe  != null && t.ChuBaiXe.Email  != null && t.ChuBaiXe.Email.ToLower()  == input)));

            if (taiKhoan == null) return null;

            // So sánh mật khẩu: hỗ trợ cả plain text (seed data) và SHA256 (đăng ký mới)
            var hashed = HashPassword(password);
            if (taiKhoan.MatKhau != password && taiKhoan.MatKhau != hashed)
                return null;

            return taiKhoan;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
