using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ── DbSets ──────────────────────────────────────────────────────────
        public DbSet<TinhThanh> TinhThanhs { get; set; } = null!;
        public DbSet<QuanHuyen> QuanHuyens { get; set; } = null!;
        public DbSet<XaPhuong> XaPhuongs { get; set; } = null!;
        public DbSet<VaiTro> VaiTros { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public DbSet<ChuBaiXe> ChuBaiXes { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<BaiXe> BaiXes { get; set; } = null!;
        public DbSet<LoaiXe> LoaiXes { get; set; } = null!;
        public DbSet<Xe> Xes { get; set; } = null!;
        public DbSet<KhachHang_Xe> KhachHangXes { get; set; } = null!;
        public DbSet<KhuVuc> KhuVucs { get; set; } = null!;
        public DbSet<ChoDauXe> ChoDauXes { get; set; } = null!;
        public DbSet<BangGia> BangGias { get; set; } = null!;
        public DbSet<DatCho> DatChos { get; set; } = null!;
        public DbSet<LogDieuKhienBarrier> LogDieuKhienBarriers { get; set; } = null!;
        public DbSet<HoaDon> HoaDons { get; set; } = null!;
        public DbSet<ThanhToan> ThanhToans { get; set; } = null!;
        public DbSet<DanhGiaBinhLuan> DanhGiaBinhLuans { get; set; } = null!;
        public DbSet<PhienChat> PhienChats { get; set; } = null!;
        public DbSet<TinNhan> TinNhans { get; set; } = null!;
        public DbSet<KhieuNai> KhieuNais { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── TinhThanh ──────────────────────────────────────────────────
            modelBuilder.Entity<TinhThanh>().ToTable("TinhThanh").HasKey(t => t.MaTinh);

            // ── QuanHuyen ──────────────────────────────────────────────────
            modelBuilder.Entity<QuanHuyen>().ToTable("QuanHuyen").HasKey(q => q.MaHuyen);
            modelBuilder.Entity<QuanHuyen>()
                .HasOne(q => q.TinhThanh).WithMany(t => t.QuanHuyens)
                .HasForeignKey(q => q.MaTinh);

            // ── XaPhuong ───────────────────────────────────────────────────
            modelBuilder.Entity<XaPhuong>().ToTable("XaPhuong").HasKey(x => x.MaXa);
            modelBuilder.Entity<XaPhuong>()
                .HasOne(x => x.QuanHuyen).WithMany(q => q.XaPhuongs)
                .HasForeignKey(x => x.MaHuyen);

            // ── VaiTro ─────────────────────────────────────────────────────
            modelBuilder.Entity<VaiTro>().ToTable("VaiTro");
            modelBuilder.Entity<VaiTro>()
                .HasIndex(v => v.TenVaiTro).IsUnique();

            // ── TaiKhoan ───────────────────────────────────────────────────
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.VaiTro).WithMany(v => v.TaiKhoans)
                .HasForeignKey(t => t.IDVaiTro);

            // ── KhachHang ──────────────────────────────────────────────────
            modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan).WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.IDTaiKhoan);
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.XaPhuong).WithMany()
                .HasForeignKey(k => k.MaXa);

            // ── ChuBaiXe ───────────────────────────────────────────────────
            modelBuilder.Entity<ChuBaiXe>().ToTable("ChuBaiXe");
            modelBuilder.Entity<ChuBaiXe>()
                .HasOne(c => c.TaiKhoan).WithOne(t => t.ChuBaiXe)
                .HasForeignKey<ChuBaiXe>(c => c.IDTaiKhoan);
            modelBuilder.Entity<ChuBaiXe>()
                .HasOne(c => c.XaPhuong).WithMany()
                .HasForeignKey(c => c.MaXa);

            // ── Admin ──────────────────────────────────────────────────────
            modelBuilder.Entity<Admin>().ToTable("Admin");
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.TaiKhoan).WithOne(t => t.Admin)
                .HasForeignKey<Admin>(a => a.IDTaiKhoan);

            // ── BaiXe (gộp đăng ký, TrangThai: Chờ duyệt/Hoạt động/...) ──
            modelBuilder.Entity<BaiXe>().ToTable("BaiXe");
            modelBuilder.Entity<BaiXe>()
                .HasOne(b => b.ChuBaiXe).WithMany(c => c.BaiXes)
                .HasForeignKey(b => b.IDChuBai);
            modelBuilder.Entity<BaiXe>()
                .HasOne(b => b.XaPhuong).WithMany()
                .HasForeignKey(b => b.MaXa);
            modelBuilder.Entity<BaiXe>()
                .Property(b => b.PhanTramChietKhau).HasPrecision(5, 2);
            modelBuilder.Entity<BaiXe>()
                .Property(b => b.DienTich).HasPrecision(10, 2);

            // ── Xe ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Xe>().ToTable("Xe").HasKey(x => x.BienSoXe);
            modelBuilder.Entity<Xe>()
                .HasOne(x => x.LoaiXe).WithMany()
                .HasForeignKey(x => x.IDLoaiXe);

            // ── KhachHang_Xe ───────────────────────────────────────────────
            modelBuilder.Entity<KhachHang_Xe>().ToTable("KhachHang_Xe");
            modelBuilder.Entity<KhachHang_Xe>()
                .HasKey(kx => new { kx.IDKhachHang, kx.IDXe });
            modelBuilder.Entity<KhachHang_Xe>()
                .HasOne(kx => kx.KhachHang).WithMany()
                .HasForeignKey(kx => kx.IDKhachHang);
            modelBuilder.Entity<KhachHang_Xe>()
                .HasOne(kx => kx.Xe).WithMany()
                .HasForeignKey(kx => kx.IDXe).OnDelete(DeleteBehavior.Cascade);

            // ── KhuVuc ─────────────────────────────────────────────────────
            modelBuilder.Entity<KhuVuc>().ToTable("KhuVuc");
            modelBuilder.Entity<KhuVuc>()
                .HasOne(k => k.BaiXe).WithMany(b => b.KhuVucs)
                .HasForeignKey(k => k.IDBaiXe);
            modelBuilder.Entity<KhuVuc>()
                .HasOne(k => k.LoaiXe).WithMany(l => l.KhuVucs)
                .HasForeignKey(k => k.IDLoaiXe);

            // ── ChoDauXe ───────────────────────────────────────────────────
            modelBuilder.Entity<ChoDauXe>().ToTable("ChoDauXe");
            modelBuilder.Entity<ChoDauXe>()
                .HasOne(c => c.KhuVuc).WithMany(k => k.ChoDauXes)
                .HasForeignKey(c => c.IDKhuVuc);

            // ── BangGia ────────────────────────────────────────────────────
            modelBuilder.Entity<BangGia>()
                .HasOne(b => b.BaiXe).WithMany(bx => bx.BangGias)
                .HasForeignKey(b => b.IDBaiXe);
            modelBuilder.Entity<BangGia>()
                .HasOne(b => b.LoaiXe).WithMany(l => l.BangGias)
                .HasForeignKey(b => b.IDLoaiXe);
            modelBuilder.Entity<BangGia>().Property(b => b.GiaTheoGio).HasPrecision(18, 2);
            modelBuilder.Entity<BangGia>().Property(b => b.GiaQuaDem).HasPrecision(18, 2);
            modelBuilder.Entity<BangGia>().Property(b => b.GiaTheoThang).HasPrecision(18, 2);
            modelBuilder.Entity<BangGia>().Property(b => b.GiaDatCho).HasPrecision(18, 2);

            // ── DatCho ─────────────────────────────────────────────────────
            modelBuilder.Entity<DatCho>()
                .HasOne(d => d.KhachHang).WithMany(k => k.DatChos)
                .HasForeignKey(d => d.IDKhachHang);
            modelBuilder.Entity<DatCho>()
                .HasOne(d => d.ChoDauXe).WithMany(c => c.DatChos)
                .HasForeignKey(d => d.IDChoDau);
            modelBuilder.Entity<DatCho>().Property(d => d.TienCoc).HasPrecision(18, 2);

            // ── LogDieuKhienBarrier ────────────────────────────────────────
            modelBuilder.Entity<LogDieuKhienBarrier>()
                .Property(l => l.ThoiGianLệnh).HasColumnName("ThoiGianLệnh");
            modelBuilder.Entity<LogDieuKhienBarrier>()
                .HasOne(l => l.DatCho).WithMany(d => d.LogDieuKhienBarriers)
                .HasForeignKey(l => l.IDDatCho);
            modelBuilder.Entity<LogDieuKhienBarrier>()
                .HasOne(l => l.TaiKhoan).WithMany(t => t.LogDieuKhienBarriers)
                .HasForeignKey(l => l.IDTaiKhoan);

            // ── HoaDon ─────────────────────────────────────────────────────
            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.DatCho).WithOne(d => d.HoaDon)
                .HasForeignKey<HoaDon>(h => h.IDDatCho);
            modelBuilder.Entity<HoaDon>().Property(h => h.TongTien).HasPrecision(18, 2);
            modelBuilder.Entity<HoaDon>().Property(h => h.TienChietKhauAdmin).HasPrecision(18, 2);
            modelBuilder.Entity<HoaDon>().Property(h => h.TienChuBaiNhan).HasPrecision(18, 2);

            // ── ThanhToan ──────────────────────────────────────────────────
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.HoaDon).WithMany(h => h.ThanhToans)
                .HasForeignKey(t => t.IDHoaDon);
            modelBuilder.Entity<ThanhToan>().Property(t => t.SoTien).HasPrecision(18, 2);

            // ── DanhGiaBinhLuan ────────────────────────────────────────────
            modelBuilder.Entity<DanhGiaBinhLuan>()
                .HasOne(d => d.KhachHang).WithMany()
                .HasForeignKey(d => d.IDKhachHang);
            modelBuilder.Entity<DanhGiaBinhLuan>()
                .HasOne(d => d.BaiXe).WithMany(b => b.DanhGiaBinhLuans)
                .HasForeignKey(d => d.IDBaiXe);
            modelBuilder.Entity<DanhGiaBinhLuan>()
                .HasOne(d => d.DatCho).WithMany()
                .HasForeignKey(d => d.IDDatCho);

            // ── PhienChat ──────────────────────────────────────────────────
            modelBuilder.Entity<PhienChat>()
                .HasOne(p => p.KhachHang).WithMany()
                .HasForeignKey(p => p.IDKhachHang);
            modelBuilder.Entity<PhienChat>()
                .HasOne(p => p.ChuBaiXe).WithMany()
                .HasForeignKey(p => p.IDChuBai);

            // ── TinNhan ────────────────────────────────────────────────────
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.PhienChat).WithMany(p => p.TinNhans)
                .HasForeignKey(t => t.IDPhienChat).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.TaiKhoan).WithMany(tk => tk.TinNhans)
                .HasForeignKey(t => t.IDTaiKhoanGui);

            // ── KhieuNai ───────────────────────────────────────────────────
            modelBuilder.Entity<KhieuNai>()
                .HasOne(k => k.KhachHang).WithMany(kh => kh.KhieuNais)
                .HasForeignKey(k => k.IDKhachHang);
            modelBuilder.Entity<KhieuNai>()
                .HasOne(k => k.BaiXe).WithMany(b => b.KhieuNais)
                .HasForeignKey(k => k.IDBaiXe);
            modelBuilder.Entity<KhieuNai>()
                .HasOne(k => k.DatCho).WithMany()
                .HasForeignKey(k => k.IDDatCho);
            modelBuilder.Entity<KhieuNai>()
                .HasOne(k => k.Admin).WithMany(a => a.KhieuNais)
                .HasForeignKey(k => k.IDAdminXuLy);
        }
    }
}
