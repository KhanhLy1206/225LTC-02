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
            // Retrieve account matching either Username or Customer Email
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.VaiTro)
                .Include(t => t.KhachHangs)
                .FirstOrDefaultAsync(t => 
                    (t.TenDangNhap.ToLower() == usernameOrEmail.ToLower() ||
                     t.KhachHangs.Any(kh => kh.Email != null && kh.Email.ToLower() == usernameOrEmail.ToLower())) &&
                    t.TrangThai);

            if (taiKhoan == null)
            {
                return null;
            }

            // Compare passwords (supports both plain text for pre-existing SQL seed data and SHA256 hashes for new registrants)
            string hashedPassword = HashPassword(password);
            if (taiKhoan.MatKhau != password && taiKhoan.MatKhau != hashedPassword)
            {
                return null;
            }

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
