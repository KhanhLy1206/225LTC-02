using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<VaiTro> VaiTros { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public DbSet<ChuBaiXe> ChuBaiXes { get; set; } = null!;
        public DbSet<DangKyBaiXe> DangKyBaiXes { get; set; } = null!;
        public DbSet<LoaiXe> LoaiXes { get; set; } = null!;
        public DbSet<Xe> Xes { get; set; } = null!;
        public DbSet<KhachHang_Xe> KhachHangXes { get; set; } = null!;
        public DbSet<BaiXe> BaiXes { get; set; } = null!;
        public DbSet<KhuVuc> KhuVucs { get; set; } = null!;
        public DbSet<ChoDauXe> ChoDauXes { get; set; } = null!;
        public DbSet<DatCho> DatChos { get; set; } = null!;
        public DbSet<DanhGiaBinhLuan> DanhGiaBinhLuans { get; set; } = null!;
        public DbSet<KhieuNai> KhieuNais { get; set; } = null!;
        public DbSet<TinhThanh> TinhThanhs { get; set; } = null!;
        public DbSet<QuanHuyen> QuanHuyens { get; set; } = null!;
        public DbSet<XaPhuong> XaPhuongs { get; set; } = null!;
        public DbSet<LogDieuKhienBarrier> LogDieuKhienBarriers { get; set; } = null!;
        public DbSet<BangGia> BangGias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for KhachHang_Xe
            modelBuilder.Entity<KhachHang_Xe>()
                .HasKey(kx => new { kx.IDKhachHang, kx.IDXe });

            // Configure unique index constraints to match database checks
            modelBuilder.Entity<VaiTro>()
                .HasIndex(v => v.TenVaiTro)
                .IsUnique();

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();
        }
    }
}
