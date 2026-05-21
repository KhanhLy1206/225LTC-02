USE master
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyBaiXe')
BEGIN
    ALTER DATABASE QuanLyBaiXe
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE

    DROP DATABASE QuanLyBaiXe
END
GO

CREATE DATABASE QuanLyBaiXe
GO

USE QuanLyBaiXe
GO

-- ============================================================
-- 1. VaiTro
-- ============================================================
CREATE TABLE VaiTro (
    ID          INT IDENTITY(1,1)   NOT NULL,
    TenVaiTro   NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_VaiTro PRIMARY KEY (ID),
    CONSTRAINT UQ_VaiTro_Ten UNIQUE (TenVaiTro)
);
GO

-- ============================================================
-- 2. TaiKhoan
-- ============================================================
CREATE TABLE TaiKhoan (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDVaiTro    INT                 NOT NULL,
    TenDangNhap VARCHAR(50)         NOT NULL,
    MatKhau     VARCHAR(255)        NOT NULL,
    AnhDaiDien  VARCHAR(255)        NULL,
    TrangThai   BIT                 NOT NULL DEFAULT 1,   -- 1 = hoạt động, 0 = khóa

    CONSTRAINT PK_TaiKhoan          PRIMARY KEY (ID),
    CONSTRAINT FK_TaiKhoan_VaiTro   FOREIGN KEY (IDVaiTro) REFERENCES VaiTro(ID),
    CONSTRAINT UQ_TaiKhoan_TenDN    UNIQUE (TenDangNhap),
    CONSTRAINT CK_TaiKhoan_TenDN    CHECK (LEN(TenDangNhap) >= 5),
    CONSTRAINT CK_TaiKhoan_MK       CHECK (LEN(MatKhau)     >= 6)
);
GO

-- ============================================================
-- 3. KhachHang
-- ============================================================
CREATE TABLE KhachHang (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan  INT                 NOT NULL,
    HoTen       NVARCHAR(100)       NOT NULL,
    SDT         VARCHAR(11)         NULL,
    CCCD        VARCHAR(20)         NULL,
    BangLaiXe   VARCHAR(20)         NULL,
    DiaChi      NVARCHAR(255)       NULL,
    LoaiKH      NVARCHAR(50)        NOT NULL DEFAULT N'Vãng lai',

    CONSTRAINT PK_KhachHang             PRIMARY KEY (ID),
    CONSTRAINT FK_KhachHang_TaiKhoan    FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT UQ_KhachHang_TaiKhoan    UNIQUE (IDTaiKhoan),       
    CONSTRAINT UQ_KhachHang_CCCD        UNIQUE (CCCD),
    CONSTRAINT UQ_KhachHang_BLX         UNIQUE (BangLaiXe),
    CONSTRAINT CK_KhachHang_SDT         CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_KhachHang_LoaiKH      CHECK (LoaiKH IN (N'Vãng lai', N'Thường xuyên', N'VIP'))
);
GO


-- ============================================================
-- 5. ChuBaiXe
-- ============================================================
CREATE TABLE ChuBaiXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan  INT                 NOT NULL,
    TenChuBai   NVARCHAR(100)       NOT NULL,
    SDT         VARCHAR(11)         NULL,
    Email       VARCHAR(100)        NULL,
    CCCD        VARCHAR(20)         NULL,
    DiaChi      NVARCHAR(255)       NULL,

    CONSTRAINT PK_ChuBaiXe              PRIMARY KEY (ID),
    CONSTRAINT FK_ChuBaiXe_TaiKhoan     FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT UQ_ChuBaiXe_TaiKhoan     UNIQUE (IDTaiKhoan),      -- 1 tài khoản = 1 chủ bãi
    CONSTRAINT CK_ChuBaiXe_SDT          CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_ChuBaiXe_Email        CHECK (Email LIKE '%@%.%')
);
GO

-- ============================================================
-- 6. LoaiXe
-- ============================================================
CREATE TABLE LoaiXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    TenLoaiXe   NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_LoaiXe        PRIMARY KEY (ID),
    CONSTRAINT UQ_LoaiXe_Ten    UNIQUE (TenLoaiXe)
);
GO

-- ============================================================
-- 7. Xe
-- ============================================================
CREATE TABLE Xe (
    BienSoXe    VARCHAR(20)         NOT NULL,
    IDLoaiXe    INT                 NOT NULL,
    TenXe       NVARCHAR(100)       NULL,
    Hang        NVARCHAR(50)        NULL,
    MauSac      NVARCHAR(50)        NULL,
    HinhAnh     NVARCHAR(255)       NULL,

    CONSTRAINT PK_Xe            PRIMARY KEY (BienSoXe),
    CONSTRAINT FK_Xe_LoaiXe     FOREIGN KEY (IDLoaiXe) REFERENCES LoaiXe(ID)
);
GO

-- ============================================================
-- ADMIN
-- ============================================================
CREATE TABLE Admin (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan          INT                 NOT NULL,
    HoTen               NVARCHAR(100)       NOT NULL,
    SDT                 VARCHAR(11)         NULL,
    Email               VARCHAR(100)        NULL,

    -- % chiết khấu admin nhận trên mỗi hóa đơn
    PhanTramChietKhau   DECIMAL(5,2)        NOT NULL DEFAULT 20,

    CONSTRAINT PK_Admin PRIMARY KEY (ID),

    CONSTRAINT FK_Admin_TaiKhoan
        FOREIGN KEY (IDTaiKhoan)
        REFERENCES TaiKhoan(ID),

    CONSTRAINT UQ_Admin_TaiKhoan
        UNIQUE (IDTaiKhoan),

    CONSTRAINT CK_Admin_SDT
        CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),

    CONSTRAINT CK_Admin_Email
        CHECK (Email LIKE '%@%.%'),

    CONSTRAINT CK_Admin_ChietKhau
        CHECK (PhanTramChietKhau BETWEEN 0 AND 100)
);
GO
-- ============================================================
-- 8. KhachHang_Xe  
-- ============================================================
CREATE TABLE KhachHang_Xe (
    IDKhachHang INT             NOT NULL,
    IDXe        VARCHAR(20)     NOT NULL,
    LoaiSoHuu   NVARCHAR(50)    NOT NULL DEFAULT N'Cá nhân',

    CONSTRAINT PK_KhachHang_Xe          PRIMARY KEY (IDKhachHang, IDXe),
    CONSTRAINT FK_KhachHang_Xe_KH       FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_KhachHang_Xe_Xe       FOREIGN KEY (IDXe)        REFERENCES Xe(BienSoXe),
    CONSTRAINT CK_KhachHang_Xe_Loai     CHECK (LoaiSoHuu IN (N'Cá nhân', N'Doanh nghiệp'))
);
GO

-- ============================================================
-- 9. BaiXe
-- ============================================================
CREATE TABLE BaiXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDChuBai    INT                 NOT NULL,
    TenBai      NVARCHAR(100)       NOT NULL,
    ViTri       NVARCHAR(255)       NULL,
    SucChua     INT                 NOT NULL,
    TrangThai   NVARCHAR(50)        NOT NULL DEFAULT N'Hoạt động',
    HinhAnh     NVARCHAR(255)       NULL,

    CONSTRAINT PK_BaiXe             PRIMARY KEY (ID),
    CONSTRAINT FK_BaiXe_ChuBai      FOREIGN KEY (IDChuBai) REFERENCES ChuBaiXe(ID),
    CONSTRAINT CK_BaiXe_SucChua     CHECK (SucChua > 0),
    CONSTRAINT CK_BaiXe_TrangThai   CHECK (TrangThai IN (N'Hoạt động', N'Đóng cửa', N'Bảo trì', N'Tạm dừng'))
);
GO

-- ============================================================
-- 10. BangGia
-- ============================================================
CREATE TABLE BangGia (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDBaiXe         INT                 NOT NULL,
    IDLoaiXe        INT                 NOT NULL,
    TenBangGia      NVARCHAR(100)       NOT NULL,
    LoaiKhungGio    NVARCHAR(50)        NULL,
    TgianBatDau     TIME                NULL,
    TgianKetThuc    TIME                NULL,
    GiaTheoGio      DECIMAL(18,2)       NULL DEFAULT 0,
    GiaQuaDem       DECIMAL(18,2)       NULL DEFAULT 0,
    GiaTheoThang    DECIMAL(18,2)       NULL DEFAULT 0,
    GiaDatCho       DECIMAL(18,2)       NULL DEFAULT 0,   -- sửa lỗi DECIMAL(18,20)
    TrangThai       BIT                 NOT NULL DEFAULT 1,

    CONSTRAINT PK_BangGia           PRIMARY KEY (ID),
    CONSTRAINT FK_BangGia_BaiXe     FOREIGN KEY (IDBaiXe)   REFERENCES BaiXe(ID),   -- sửa: BaiDo -> BaiXe
    CONSTRAINT FK_BangGia_LoaiXe    FOREIGN KEY (IDLoaiXe)  REFERENCES LoaiXe(ID),
    CONSTRAINT CK_BangGia_Gia       CHECK (GiaTheoGio >= 0 AND GiaQuaDem >= 0
                                        AND GiaTheoThang >= 0 AND GiaDatCho >= 0)
);
GO

-- ============================================================
-- 11. KhuVuc
-- ============================================================
CREATE TABLE KhuVuc (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDBaiXe     INT                 NOT NULL,   -- sửa: IDBaiDo -> IDBaiXe
    TenKhuVuc   NVARCHAR(50)        NOT NULL,
    SucChua     INT                 NOT NULL,
    HinhAnh     NVARCHAR(255)       NULL,

    CONSTRAINT PK_KhuVuc            PRIMARY KEY (ID),
    CONSTRAINT FK_KhuVuc_BaiXe      FOREIGN KEY (IDBaiXe) REFERENCES BaiXe(ID),   -- sửa: BaiDo -> BaiXe
    CONSTRAINT CK_KhuVuc_SucChua    CHECK (SucChua > 0)
);
GO

-- ============================================================
-- 12. ChoDauXe
-- ============================================================
CREATE TABLE ChoDauXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDKhuVuc    INT                 NOT NULL,
    TenChoDau   NVARCHAR(20)        NOT NULL,
    KichThuoc   VARCHAR(50)         NULL,
    TrangThai   NVARCHAR(50)        NOT NULL DEFAULT N'Trống',

    CONSTRAINT PK_ChoDauXe              PRIMARY KEY (ID),
    CONSTRAINT FK_ChoDauXe_KhuVuc       FOREIGN KEY (IDKhuVuc) REFERENCES KhuVuc(ID),
    CONSTRAINT CK_ChoDauXe_TrangThai    CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang đỗ', N'Bảo trì'))
);
GO

-- ============================================================
-- 13. DatCho
-- ============================================================
CREATE TABLE DatCho (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDXe            VARCHAR(20)         NOT NULL,
    IDChoDau        INT                 NOT NULL,
    NgayDat         DATETIME            NOT NULL DEFAULT GETDATE(),
    TgianBatDau     DATETIME            NOT NULL,
    TgianKetThuc    DATETIME            NOT NULL,
    TienCoc         DECIMAL(18,2)       NULL DEFAULT 0,
    TrangThai       NVARCHAR(50)        NOT NULL DEFAULT N'Đã đặt',

    CONSTRAINT PK_DatCho                PRIMARY KEY (ID),
    CONSTRAINT FK_DatCho_KhachHang      FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_DatCho_Xe             FOREIGN KEY (IDXe)        REFERENCES Xe(BienSoXe),
    CONSTRAINT FK_DatCho_ChoDau         FOREIGN KEY (IDChoDau)    REFERENCES ChoDauXe(ID),
    CONSTRAINT CK_DatCho_ThoiGian       CHECK (TgianKetThuc > TgianBatDau),
    CONSTRAINT CK_DatCho_TienCoc        CHECK (TienCoc >= 0),
    CONSTRAINT CK_DatCho_TrangThai      CHECK (TrangThai IN (N'Đã đặt', N'Đã hủy', N'Hoàn thành', N'Quá hạn'))
);
GO

-- ============================================================
-- 14. Barrier
-- ============================================================
CREATE TABLE Barrier (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDBaiXe     INT                 NOT NULL,
    TenBarrier  NVARCHAR(100)       NOT NULL,
    LoaiBarrier NVARCHAR(50)        NULL,
    TrangThai   NVARCHAR(50)        NOT NULL DEFAULT N'Hoạt động',

    CONSTRAINT PK_Barrier           PRIMARY KEY (ID),
    CONSTRAINT FK_Barrier_BaiXe     FOREIGN KEY (IDBaiXe) REFERENCES BaiXe(ID),
    CONSTRAINT CK_Barrier_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Lỗi', N'Bảo trì'))
);
GO

-- ============================================================
-- 15. LogBarrier
-- ============================================================
CREATE TABLE LogBarrier (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDBarrier   INT                 NOT NULL,
    IDXe        VARCHAR(20)         NOT NULL,
    IDDatCho    INT                 NULL,       -- NULL nếu xe vào không có đặt trước
    ThoiGian    DATETIME            NOT NULL DEFAULT GETDATE(),
    HanhDong    NVARCHAR(50)        NOT NULL,   -- 'Vào' hoặc 'Ra'
    KetQua      NVARCHAR(50)        NULL,       -- 'Thành công', 'Từ chối'
    LyDoTuChoi  NVARCHAR(255)       NULL,
    AnhBienSo   VARCHAR(255)        NULL,

    CONSTRAINT PK_LogBarrier            PRIMARY KEY (ID),
    CONSTRAINT FK_LogBarrier_Barrier    FOREIGN KEY (IDBarrier) REFERENCES Barrier(ID),
    CONSTRAINT FK_LogBarrier_Xe         FOREIGN KEY (IDXe)      REFERENCES Xe(BienSoXe),
    CONSTRAINT FK_LogBarrier_DatCho     FOREIGN KEY (IDDatCho)  REFERENCES DatCho(ID),
    CONSTRAINT CK_LogBarrier_HanhDong   CHECK (HanhDong IN (N'Vào', N'Ra'))
);
GO

-- ============================================================
-- 16. LuotGuiXe
-- ============================================================
CREATE TABLE LuotGuiXe (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDDatCho        INT                 NOT NULL,
    IDXe            VARCHAR(20)         NOT NULL,
    IDChoDau        INT                 NOT NULL,
    IDLogVao        INT                 NOT NULL,
    IDLogRa         INT                 NULL,       -- NULL khi xe chưa ra
    ThoiGianVao     DATETIME            NOT NULL,
    ThoiGianRa      DATETIME            NULL,
    TongThoiGian    DECIMAL(10,2)       NULL,       -- Đổi FLOAT -> DECIMAL tránh lỗi làm tròn
    DonGia          DECIMAL(18,2)       NULL DEFAULT 0,
    TongTien        DECIMAL(18,2)       NULL DEFAULT 0,
    TrangThai       NVARCHAR(50)        NOT NULL DEFAULT N'Đang gửi',

    CONSTRAINT PK_LuotGuiXe             PRIMARY KEY (ID),
    CONSTRAINT FK_LuotGuiXe_DatCho      FOREIGN KEY (IDDatCho)  REFERENCES DatCho(ID),
    CONSTRAINT FK_LuotGuiXe_Xe          FOREIGN KEY (IDXe)      REFERENCES Xe(BienSoXe),
    CONSTRAINT FK_LuotGuiXe_ChoDau      FOREIGN KEY (IDChoDau)  REFERENCES ChoDauXe(ID),
    CONSTRAINT FK_LuotGuiXe_LogVao      FOREIGN KEY (IDLogVao)  REFERENCES LogBarrier(ID),
    CONSTRAINT FK_LuotGuiXe_LogRa       FOREIGN KEY (IDLogRa)   REFERENCES LogBarrier(ID),
    CONSTRAINT CK_LuotGuiXe_ThoiGian    CHECK (ThoiGianRa IS NULL OR ThoiGianRa > ThoiGianVao),
    CONSTRAINT CK_LuotGuiXe_TrangThai   CHECK (TrangThai IN (N'Đang gửi', N'Đã ra', N'Đã thanh toán'))
);
GO

-- ============================================================
-- 17. HoaDon
-- ============================================================
CREATE TABLE HoaDon (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang INT                 NOT NULL,
    IDLuotGui   INT                 NOT NULL,
    ThanhTien   DECIMAL(18,2)       NOT NULL DEFAULT 0,
    NgayTao     DATETIME            NOT NULL DEFAULT GETDATE(),
    LoaiHoaDon  NVARCHAR(50)        NULL,

    CONSTRAINT PK_HoaDon                PRIMARY KEY (ID),
    CONSTRAINT FK_HoaDon_KhachHang      FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_HoaDon_LuotGui        FOREIGN KEY (IDLuotGui)   REFERENCES LuotGuiXe(ID),
    CONSTRAINT UQ_HoaDon_LuotGui        UNIQUE (IDLuotGui),    
    CONSTRAINT CK_HoaDon_ThanhTien      CHECK (ThanhTien >= 0)
);
GO

-- ============================================================
-- 18. ChiTietHoaDon
-- ============================================================
CREATE TABLE ChiTietHoaDon (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDHoaDon    INT                 NOT NULL,
    TenChiTiet  NVARCHAR(100)       NOT NULL,
    SL          INT                 NOT NULL DEFAULT 1,
    DonGia      DECIMAL(18,2)       NOT NULL,
    ThanhTien   AS (SL * DonGia),

    CONSTRAINT PK_ChiTietHoaDon         PRIMARY KEY (ID),
    CONSTRAINT FK_ChiTietHoaDon_HoaDon  FOREIGN KEY (IDHoaDon) REFERENCES HoaDon(ID),
    CONSTRAINT CK_ChiTiet_DonGia        CHECK (DonGia >= 0),
    CONSTRAINT CK_ChiTiet_SL            CHECK (SL > 0)
);
GO

-- ============================================================
-- 19. ThanhToan
-- ============================================================
CREATE TABLE ThanhToan (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDHoaDon        INT                 NOT NULL,
    PhuongThuc      NVARCHAR(50)        NOT NULL,
    SoTien          DECIMAL(18,2)       NOT NULL,
    TrangThai       BIT                 NOT NULL DEFAULT 0,   -- 0 = chờ, 1 = thành công
    NgayThanhToan   DATETIME            NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_ThanhToan             PRIMARY KEY (ID),
    CONSTRAINT FK_ThanhToan_HoaDon      FOREIGN KEY (IDHoaDon) REFERENCES HoaDon(ID),
    CONSTRAINT CK_ThanhToan_PhuongThuc  CHECK (PhuongThuc IN (N'Tiền mặt', N'Thẻ', N'QR Code', N'Chuyển khoản')),
    CONSTRAINT CK_ThanhToan_SoTien      CHECK (SoTien >= 0)
);
GO

-- ============================================================
-- 20. DanhGia
-- ============================================================
CREATE TABLE DanhGia (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDHoaDon        INT                 NOT NULL,
    NoiDung         NVARCHAR(MAX)       NULL,
    DiemDanhGia     INT                 NOT NULL,
    NgayDanhGia     DATETIME            NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_DanhGia               PRIMARY KEY (ID),
    CONSTRAINT FK_DanhGia_KhachHang     FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_DanhGia_HoaDon        FOREIGN KEY (IDHoaDon)    REFERENCES HoaDon(ID),
    CONSTRAINT UQ_DanhGia_HoaDon        UNIQUE (IDKhachHang, IDHoaDon),  -- mỗi KH chỉ đánh giá 1 lần / hóa đơn
    CONSTRAINT CK_DanhGia_Diem          CHECK (DiemDanhGia BETWEEN 1 AND 5)
);
GO

-- ============================================================
-- 21. KhieuNai
-- ============================================================
CREATE TABLE KhieuNai (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang INT                 NOT NULL,
    IDHoaDon    INT                 NOT NULL,
    NoiDung     NVARCHAR(MAX)       NOT NULL,
    TrangThai   NVARCHAR(50)        NOT NULL DEFAULT N'Chờ xử lý',
    NgayKhieuNai DATETIME           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_KhieuNai              PRIMARY KEY (ID),
    CONSTRAINT FK_KhieuNai_KhachHang    FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_KhieuNai_HoaDon       FOREIGN KEY (IDHoaDon)    REFERENCES HoaDon(ID),
    CONSTRAINT CK_KhieuNai_TrangThai    CHECK (TrangThai IN (N'Chờ xử lý', N'Đang xử lý', N'Đã giải quyết', N'Từ chối'))
);
GO
-- ============================================================
-- INSERT VAI TRÒ
-- ============================================================
INSERT INTO VaiTro(TenVaiTro)
VALUES
(N'Admin'),
(N'Nhân viên'),
(N'Khách hàng'),
(N'Chủ bãi xe')
GO

-- ============================================================
-- INSERT LOẠI XE
-- ============================================================
INSERT INTO LoaiXe(TenLoaiXe)
VALUES
(N'Xe máy'),
(N'Ô tô'),
(N'Xe tải'),
(N'Xe đạp điện')
GO

-- ============================================================
-- INSERT TÀI KHOẢN
-- ============================================================
INSERT INTO TaiKhoan
(
    IDVaiTro,
    TenDangNhap,
    MatKhau
)
VALUES
(
    (SELECT ID FROM VaiTro WHERE TenVaiTro = N'Admin'),
    'admin01',
    '123456'
),
(
    (SELECT ID FROM VaiTro WHERE TenVaiTro = N'Nhân viên'),
    'nhanvien01',
    '123456'
),
(
    (SELECT ID FROM VaiTro WHERE TenVaiTro = N'Khách hàng'),
    'khachhang01',
    '123456'
),
(
    (SELECT ID FROM VaiTro WHERE TenVaiTro = N'Khách hàng'),
    'khachhang02',
    '123456'
),
(
    (SELECT ID FROM VaiTro WHERE TenVaiTro = N'Chủ bãi xe'),
    'chubaixe01',
    '123456'
)
GO

-- ============================================================
-- INSERT KHÁCH HÀNG
-- ============================================================
INSERT INTO KhachHang
(
    IDTaiKhoan,
    HoTen,
    SDT,
    CCCD,
    BangLaiXe,
    DiaChi,
    LoaiKH
)
VALUES
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang01'),
    N'Nguyễn Văn A',
    '0912345678',
    '123456789001',
    'BLX001',
    N'Gia Lai',
    N'VIP'
),
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang02'),
    N'Trần Thị B',
    '0987654321',
    '123456789002',
    'BLX002',
    N'Đà Nẵng',
    N'Thường xuyên'
)
GO


-- ============================================================
-- INSERT CHỦ BÃI XE
-- ============================================================
INSERT INTO ChuBaiXe
(
    IDTaiKhoan,
    TenChuBai,
    SDT,
    Email,
    CCCD,
    DiaChi
)
VALUES
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'chubaixe01'),
    N'Lê Văn Chủ',
    '0909999999',
    'chubaixe@gmail.com',
    '999999999999',
    N'Gia Lai'
)
GO

-- ============================================================
-- INSERT XE
-- ============================================================
INSERT INTO Xe
(
    BienSoXe,
    IDLoaiXe,
    TenXe,
    Hang,
    MauSac
)
VALUES
(
    '81A-12345',
    (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Ô tô'),
    N'Vios',
    N'Toyota',
    N'Trắng'
),
(
    '81B1-67890',
    (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Xe máy'),
    N'Vision',
    N'Honda',
    N'Đỏ'
)
GO

-- ============================================================
-- INSERT KHACHHANG_XE
-- ============================================================
INSERT INTO KhachHang_Xe
(
    IDKhachHang,
    IDXe,
    LoaiSoHuu
)
VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    '81A-12345',
    N'Cá nhân'
),
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789002'),
    '81B1-67890',
    N'Cá nhân'
)
GO

-- ============================================================
-- INSERT BÃI XE
-- ============================================================
INSERT INTO BaiXe
(
    IDChuBai,
    TenBai,
    ViTri,
    SucChua,
    TrangThai
)
VALUES
(
    (SELECT ID FROM ChuBaiXe WHERE CCCD = '999999999999'),
    N'Bãi xe trung tâm',
    N'An Khê - Gia Lai',
    100,
    N'Hoạt động'
)
GO

-- ============================================================
-- INSERT BẢNG GIÁ
-- ============================================================
INSERT INTO BangGia
(
    IDBaiXe,
    IDLoaiXe,
    TenBangGia,
    LoaiKhungGio,
    TgianBatDau,
    TgianKetThuc,
    GiaTheoGio,
    GiaQuaDem,
    GiaTheoThang,
    GiaDatCho
)
VALUES
(
    (SELECT ID FROM BaiXe WHERE TenBai = N'Bãi xe trung tâm'),
    (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Xe máy'),
    N'Giá xe máy',
    N'Ban ngày',
    '06:00',
    '22:00',
    5000,
    20000,
    100000,
    3000
),
(
    (SELECT ID FROM BaiXe WHERE TenBai = N'Bãi xe trung tâm'),
    (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Ô tô'),
    N'Giá ô tô',
    N'Ban ngày',
    '06:00',
    '22:00',
    20000,
    50000,
    500000,
    10000
)
GO