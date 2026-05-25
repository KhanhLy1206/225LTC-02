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
-- 1. TinhThanh (Provinces/Cities)
-- ============================================================
CREATE TABLE TinhThanh (
    MaTinh      VARCHAR(20)         NOT NULL,
    TenTinh     NVARCHAR(100)       NOT NULL,

    CONSTRAINT PK_TinhThanh PRIMARY KEY (MaTinh)
);
GO

-- ============================================================
-- 2. QuanHuyen (Districts)
-- ============================================================
CREATE TABLE QuanHuyen (
    MaHuyen     VARCHAR(20)         NOT NULL,
    MaTinh      VARCHAR(20)         NOT NULL,
    TenHuyen    NVARCHAR(100)       NOT NULL,

    CONSTRAINT PK_QuanHuyen         PRIMARY KEY (MaHuyen),
    CONSTRAINT FK_QuanHuyen_Tinh    FOREIGN KEY (MaTinh) REFERENCES TinhThanh(MaTinh)
);
GO

-- ============================================================
-- 3. XaPhuong (Wards/Communes)
-- ============================================================
CREATE TABLE XaPhuong (
    MaXa        VARCHAR(20)         NOT NULL,
    MaHuyen     VARCHAR(20)         NOT NULL,
    TenXa       NVARCHAR(100)       NOT NULL,

    CONSTRAINT PK_XaPhuong          PRIMARY KEY (MaXa),
    CONSTRAINT FK_XaPhuong_Huyen    FOREIGN KEY (MaHuyen) REFERENCES QuanHuyen(MaHuyen)
);
GO

-- ============================================================
-- 4. VaiTro (System Roles)
-- ============================================================
CREATE TABLE VaiTro (
    ID          INT IDENTITY(1,1)   NOT NULL,
    TenVaiTro   NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_VaiTro PRIMARY KEY (ID),
    CONSTRAINT UQ_VaiTro_Ten UNIQUE (TenVaiTro)
);
GO

-- ============================================================
-- 5. TaiKhoan (Users Credentials)
-- ============================================================
CREATE TABLE TaiKhoan (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDVaiTro    INT                 NOT NULL,
    TenDangNhap VARCHAR(50)         NOT NULL,
    MatKhau     VARCHAR(255)        NOT NULL, -- Lưu trữ mật khẩu (mã hóa)
    AnhDaiDien  VARCHAR(255)        NULL,
    TrangThai   BIT                 NOT NULL DEFAULT 1, -- 1 = Hoạt động, 0 = Khóa

    CONSTRAINT PK_TaiKhoan          PRIMARY KEY (ID),
    CONSTRAINT FK_TaiKhoan_VaiTro   FOREIGN KEY (IDVaiTro) REFERENCES VaiTro(ID),
    CONSTRAINT UQ_TaiKhoan_TenDN    UNIQUE (TenDangNhap),
    CONSTRAINT CK_TaiKhoan_TenDN    CHECK (LEN(TenDangNhap) >= 5),
    CONSTRAINT CK_TaiKhoan_MK       CHECK (LEN(MatKhau)     >= 6)
);
GO

-- ============================================================
-- 6. KhachHang (Customers Profile)
-- ============================================================
CREATE TABLE KhachHang (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan  INT                 NOT NULL,
    HoTen       NVARCHAR(100)       NOT NULL,
    SDT         VARCHAR(11)         NULL,
    Email       VARCHAR(100)        NULL,
    CCCD        VARCHAR(20)         NULL,
    BangLaiXe   VARCHAR(20)         NULL,
    MaXa        VARCHAR(20)         NULL,
    DiaChiChiTiet NVARCHAR(255)     NULL,

    CONSTRAINT PK_KhachHang             PRIMARY KEY (ID),
    CONSTRAINT FK_KhachHang_TaiKhoan    FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT FK_KhachHang_Xa          FOREIGN KEY (MaXa) REFERENCES XaPhuong(MaXa),
    CONSTRAINT UQ_KhachHang_TaiKhoan    UNIQUE (IDTaiKhoan),       
    CONSTRAINT UQ_KhachHang_CCCD        UNIQUE (CCCD),
    CONSTRAINT UQ_KhachHang_BLX         UNIQUE (BangLaiXe),
    CONSTRAINT UQ_KhachHang_Email       UNIQUE (Email),
    CONSTRAINT CK_KhachHang_SDT         CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_KhachHang_Email       CHECK (Email LIKE '%@%.%')
);
GO

-- ============================================================
-- 7. ChuBaiXe (Parking Owners Profile)
-- ============================================================
CREATE TABLE ChuBaiXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan  INT                 NOT NULL,
    TenChuBai   NVARCHAR(100)       NOT NULL,
    SDT         VARCHAR(11)         NOT NULL,
    Email       VARCHAR(100)        NOT NULL,
    CCCD        VARCHAR(20)         NULL,
    MaXa        VARCHAR(20)         NULL,
    DiaChiChiTiet NVARCHAR(255)     NULL,

    CONSTRAINT PK_ChuBaiXe              PRIMARY KEY (ID),
    CONSTRAINT FK_ChuBaiXe_TaiKhoan     FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT FK_ChuBaiXe_Xa           FOREIGN KEY (MaXa) REFERENCES XaPhuong(MaXa),
    CONSTRAINT UQ_ChuBaiXe_TaiKhoan     UNIQUE (IDTaiKhoan),      
    CONSTRAINT UQ_ChuBaiXe_Email        UNIQUE (Email),
    CONSTRAINT UQ_ChuBaiXe_CCCD         UNIQUE (CCCD),
    CONSTRAINT CK_ChuBaiXe_SDT          CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_ChuBaiXe_Email        CHECK (Email LIKE '%@%.%')
);
GO

-- ============================================================
-- 8. Admin (Administrators Profile)
-- ============================================================
CREATE TABLE Admin (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    IDTaiKhoan          INT                 NOT NULL,
    HoTen               NVARCHAR(100)       NOT NULL,
    SDT                 VARCHAR(11)         NULL,
    Email               VARCHAR(100)        NULL,

    CONSTRAINT PK_Admin             PRIMARY KEY (ID),
    CONSTRAINT FK_Admin_TaiKhoan    FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT UQ_Admin_TaiKhoan    UNIQUE (IDTaiKhoan),
    CONSTRAINT UQ_Admin_Email       UNIQUE (Email),
    CONSTRAINT CK_Admin_SDT         CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_Admin_Email       CHECK (Email LIKE '%@%.%')
);
GO

-- ============================================================
-- 9. DangKyBaiXe (Parking Lot Registration Applications)
-- ============================================================
CREATE TABLE DangKyBaiXe (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    TenChuBai           NVARCHAR(100)       NOT NULL,
    SDT                 VARCHAR(11)         NOT NULL,
    Email               VARCHAR(100)        NOT NULL,
    CCCD                VARCHAR(20)         NOT NULL,
    TenBai              NVARCHAR(100)       NOT NULL,
    MaXa                VARCHAR(20)         NOT NULL,
    DiaChiChiTiet       NVARCHAR(255)       NOT NULL,
    SucChua             INT                 NOT NULL,
    HinhAnh             NVARCHAR(255)       NOT NULL, -- Bắt buộc đăng tải hình ảnh
    GiayPhepKinhDoanh   NVARCHAR(255)       NULL,     -- Tài liệu chứng minh
    NgayGui             DATETIME            NOT NULL DEFAULT GETDATE(),
    TrangThai           NVARCHAR(50)        NOT NULL DEFAULT N'Chờ duyệt', -- N'Chờ duyệt', N'Đã duyệt', N'Từ chối'
    GhiChu              NVARCHAR(MAX)       NULL,     -- Lý do từ chối nếu có

    CONSTRAINT PK_DangKyBaiXe           PRIMARY KEY (ID),
    CONSTRAINT FK_DangKyBaiXe_Xa        FOREIGN KEY (MaXa) REFERENCES XaPhuong(MaXa),
    CONSTRAINT CK_DangKyBaiXe_SucChua   CHECK (SucChua > 0),
    CONSTRAINT CK_DangKyBaiXe_TrangThai CHECK (TrangThai IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối')),
    CONSTRAINT CK_DangKyBaiXe_SDT       CHECK (SDT LIKE '[0-9]%' AND LEN(SDT) BETWEEN 10 AND 11),
    CONSTRAINT CK_DangKyBaiXe_Email     CHECK (Email LIKE '%@%.%')
);
GO

-- ============================================================
-- 10. BaiXe (Approved Parking Lots)
-- ============================================================
CREATE TABLE BaiXe (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    IDChuBai            INT                 NOT NULL,
    IDDangKy            INT                 NULL, -- Liên kết ngược tới đơn đăng ký đã duyệt
    TenBai              NVARCHAR(100)       NOT NULL,
    MaXa                VARCHAR(20)         NOT NULL,
    DiaChiChiTiet       NVARCHAR(255)       NOT NULL,
    SucChua             INT                 NOT NULL,
    PhanTramChietKhau   DECIMAL(5,2)        NOT NULL DEFAULT 10.00, -- Thu nhập admin nhận được (% trên hóa đơn)
    TrangThai           NVARCHAR(50)        NOT NULL DEFAULT N'Hoạt động', -- N'Hoạt động', N'Tạm đóng', N'Bảo trì'
    HinhAnh             NVARCHAR(255)       NULL,

    CONSTRAINT PK_BaiXe                 PRIMARY KEY (ID),
    CONSTRAINT FK_BaiXe_ChuBai          FOREIGN KEY (IDChuBai) REFERENCES ChuBaiXe(ID),
    CONSTRAINT FK_BaiXe_DangKy          FOREIGN KEY (IDDangKy) REFERENCES DangKyBaiXe(ID),
    CONSTRAINT FK_BaiXe_Xa              FOREIGN KEY (MaXa) REFERENCES XaPhuong(MaXa),
    CONSTRAINT CK_BaiXe_SucChua         CHECK (SucChua > 0),
    CONSTRAINT CK_BaiXe_ChietKhau       CHECK (PhanTramChietKhau BETWEEN 0 AND 100),
    CONSTRAINT CK_BaiXe_TrangThai       CHECK (TrangThai IN (N'Hoạt động', N'Tạm đóng', N'Bảo trì'))
);
GO

-- ============================================================
-- 11. LoaiXe (Vehicle Types)
-- ============================================================
CREATE TABLE LoaiXe (
    ID          INT IDENTITY(1,1)   NOT NULL,
    TenLoaiXe   NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_LoaiXe        PRIMARY KEY (ID),
    CONSTRAINT UQ_LoaiXe_Ten    UNIQUE (TenLoaiXe)
);
GO

-- ============================================================
-- 12. Xe (Vehicles)
-- ============================================================
CREATE TABLE Xe (
    BienSoXe    VARCHAR(20)         NOT NULL,
    IDLoaiXe    INT                 NOT NULL,
    TenXe       NVARCHAR(100)       NULL,
    Hang        NVARCHAR(50)        NULL,
    MauSac      NVARCHAR(50)        NULL,

    CONSTRAINT PK_Xe            PRIMARY KEY (BienSoXe),
    CONSTRAINT FK_Xe_LoaiXe     FOREIGN KEY (IDLoaiXe) REFERENCES LoaiXe(ID)
);
GO

-- ============================================================
-- 13. KhachHang_Xe (Customer-Vehicle Mapping)
-- ============================================================
CREATE TABLE KhachHang_Xe (
    IDKhachHang INT             NOT NULL,
    IDXe        VARCHAR(20)     NOT NULL,
    LoaiSoHuu   NVARCHAR(50)    NOT NULL DEFAULT N'Cá nhân',

    CONSTRAINT PK_KhachHang_Xe          PRIMARY KEY (IDKhachHang, IDXe),
    CONSTRAINT FK_KhachHang_Xe_KH       FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_KhachHang_Xe_Xe       FOREIGN KEY (IDXe)        REFERENCES Xe(BienSoXe) ON DELETE CASCADE,
    CONSTRAINT CK_KhachHang_Xe_Loai     CHECK (LoaiSoHuu IN (N'Cá nhân', N'Doanh nghiệp'))
);
GO

-- ============================================================
-- 14. KhuVuc (Areas inside a Parking Lot)
-- ============================================================
CREATE TABLE KhuVuc (
    ID          INT IDENTITY(1,1)   NOT NULL,
    IDBaiXe     INT                 NOT NULL,
    IDLoaiXe    INT                 NOT NULL, -- Ràng buộc loại xe đỗ ở khu vực này
    TenKhuVuc   NVARCHAR(50)        NOT NULL,
    SucChua     INT                 NOT NULL,

    CONSTRAINT PK_KhuVuc            PRIMARY KEY (ID),
    CONSTRAINT FK_KhuVuc_BaiXe      FOREIGN KEY (IDBaiXe) REFERENCES BaiXe(ID),
    CONSTRAINT FK_KhuVuc_LoaiXe     FOREIGN KEY (IDLoaiXe) REFERENCES LoaiXe(ID),
    CONSTRAINT CK_KhuVuc_SucChua    CHECK (SucChua > 0)
);
GO

-- ============================================================
-- 15. ChoDauXe (Specific Parking Spots with Smart Barriers)
-- ============================================================
CREATE TABLE ChoDauXe (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    IDKhuVuc            INT                 NOT NULL,
    TenChoDau           NVARCHAR(20)        NOT NULL,
    KichThuoc           VARCHAR(50)         NULL,
    MaSoKhoa            VARCHAR(50)         NULL, -- Mã số định danh của khóa/barie thông minh tại chỗ đỗ này
    TrangThaiKhoa       NVARCHAR(50)        NOT NULL DEFAULT N'Đóng', -- Trạng thái vật lý khóa: N'Đóng', N'Mở', N'Lỗi'
    TrangThaiO          NVARCHAR(50)        NOT NULL DEFAULT N'Trống', -- Trạng thái đặt chỗ: N'Trống', N'Đã đặt', N'Đang đỗ', N'Bảo trì'

    CONSTRAINT PK_ChoDauXe              PRIMARY KEY (ID),
    CONSTRAINT FK_ChoDauXe_KhuVuc       FOREIGN KEY (IDKhuVuc) REFERENCES KhuVuc(ID),
    CONSTRAINT UQ_ChoDauXe_Khoa         UNIQUE (MaSoKhoa),
    CONSTRAINT CK_ChoDauXe_TrangThaiK   CHECK (TrangThaiKhoa IN (N'Đóng', N'Mở', N'Lỗi')),
    CONSTRAINT CK_ChoDauXe_TrangThaiO   CHECK (TrangThaiO IN (N'Trống', N'Đã đặt', N'Đang đỗ', N'Bảo trì'))
);
GO

-- ============================================================
-- 16. BangGia (Pricing Tariff)
-- ============================================================
CREATE TABLE BangGia (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDBaiXe         INT                 NOT NULL,
    IDLoaiXe        INT                 NOT NULL,
    TenBangGia      NVARCHAR(100)       NOT NULL,
    GiaTheoGio      DECIMAL(18,2)       NOT NULL DEFAULT 0,
    GiaQuaDem       DECIMAL(18,2)       NOT NULL DEFAULT 0,
    GiaTheoThang    DECIMAL(18,2)       NOT NULL DEFAULT 0,
    GiaDatCho       DECIMAL(18,2)       NOT NULL DEFAULT 0, -- Phí giữ chỗ đỗ trước
    TrangThai       BIT                 NOT NULL DEFAULT 1,

    CONSTRAINT PK_BangGia           PRIMARY KEY (ID),
    CONSTRAINT FK_BangGia_BaiXe     FOREIGN KEY (IDBaiXe)   REFERENCES BaiXe(ID),   
    CONSTRAINT FK_BangGia_LoaiXe    FOREIGN KEY (IDLoaiXe)  REFERENCES LoaiXe(ID),
    CONSTRAINT CK_BangGia_Gia       CHECK (GiaTheoGio >= 0 AND GiaQuaDem >= 0
                                        AND GiaTheoThang >= 0 AND GiaDatCho >= 0)
);
GO

-- ============================================================
-- 17. DatCho (Reservations/Bookings)
-- ============================================================
CREATE TABLE DatCho (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDChoDau        INT                 NOT NULL,
    BienSoXe        VARCHAR(20)         NOT NULL, -- Biển số xe thực hiện đặt chỗ đỗ
    NgayDat         DATETIME            NOT NULL DEFAULT GETDATE(),
    TgianBatDau     DATETIME            NOT NULL,
    TgianKetThuc    DATETIME            NOT NULL,
    TienCoc         DECIMAL(18,2)       NOT NULL DEFAULT 0, -- Tiền thanh toán đặt chỗ trước
    TrangThai       NVARCHAR(50)        NOT NULL DEFAULT N'Đã đặt', -- N'Đã đặt', N'Đang đỗ', N'Hoàn thành', N'Đã hủy', N'Quá hạn'

    CONSTRAINT PK_DatCho                PRIMARY KEY (ID),
    CONSTRAINT FK_DatCho_KhachHang      FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_DatCho_ChoDau         FOREIGN KEY (IDChoDau)    REFERENCES ChoDauXe(ID),
    CONSTRAINT CK_DatCho_ThoiGian       CHECK (TgianKetThuc > TgianBatDau),
    CONSTRAINT CK_DatCho_TienCoc        CHECK (TienCoc >= 0),
    CONSTRAINT CK_DatCho_TrangThai      CHECK (TrangThai IN (N'Đã đặt', N'Đang đỗ', N'Hoàn thành', N'Đã hủy', N'Quá hạn'))
);
GO

-- ============================================================
-- 18. LogDieuKhienBarrier (Barrier Control logs by Customer)
-- ============================================================
CREATE TABLE LogDieuKhienBarrier (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDDatCho        INT                 NOT NULL,
    IDTaiKhoan      INT                 NOT NULL, -- Người gửi lệnh (tài khoản khách hàng nắm quyền)
    ThoiGianLệnh    DATETIME            NOT NULL DEFAULT GETDATE(),
    HanhDong        NVARCHAR(50)        NOT NULL, -- N'Mở khóa', N'Khóa lại'
    KetQua          NVARCHAR(50)        NOT NULL, -- N'Thành công', N'Thất bại'
    GhiChu          NVARCHAR(255)       NULL,

    CONSTRAINT PK_LogDieuKhienBarrier       PRIMARY KEY (ID),
    CONSTRAINT FK_LogDieuKhien_DatCho       FOREIGN KEY (IDDatCho) REFERENCES DatCho(ID),
    CONSTRAINT FK_LogDieuKhien_TaiKhoan     FOREIGN KEY (IDTaiKhoan) REFERENCES TaiKhoan(ID),
    CONSTRAINT CK_LogDieuKhien_HanhDong     CHECK (HanhDong IN (N'Mở khóa', N'Khóa lại')),
    CONSTRAINT CK_LogDieuKhien_KetQua       CHECK (KetQua IN (N'Thành công', N'Thất bại'))
);
GO

-- ============================================================
-- 19. HoaDon (Invoices with Split Income)
-- ============================================================
CREATE TABLE HoaDon (
    ID                  INT IDENTITY(1,1)   NOT NULL,
    IDDatCho            INT                 NOT NULL,
    TongTien            DECIMAL(18,2)       NOT NULL DEFAULT 0,
    TienChietKhauAdmin  DECIMAL(18,2)       NOT NULL DEFAULT 0, -- Phần trăm chiết khấu thuộc về Admin hệ thống
    TienChuBaiNhan      DECIMAL(18,2)       NOT NULL DEFAULT 0, -- Số tiền chủ bãi xe nhận được
    NgayTao             DATETIME            NOT NULL DEFAULT GETDATE(),
    TrangThai           NVARCHAR(50)        NOT NULL DEFAULT N'Chưa thanh toán', -- N'Chưa thanh toán', N'Đã thanh toán', N'Đã hoàn tiền'

    CONSTRAINT PK_HoaDon                PRIMARY KEY (ID),
    CONSTRAINT FK_HoaDon_DatCho         FOREIGN KEY (IDDatCho) REFERENCES DatCho(ID),
    CONSTRAINT UQ_HoaDon_DatCho         UNIQUE (IDDatCho),
    CONSTRAINT CK_HoaDon_Tien           CHECK (TongTien >= 0 AND TienChietKhauAdmin >= 0 AND TienChuBaiNhan >= 0),
    CONSTRAINT CK_HoaDon_KhauTru        CHECK (TongTien = TienChietKhauAdmin + TienChuBaiNhan),
    CONSTRAINT CK_HoaDon_TrangThai      CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán', N'Đã hoàn tiền'))
);
GO

-- ============================================================
-- 20. ThanhToan (Payments)
-- ============================================================
CREATE TABLE ThanhToan (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDHoaDon        INT                 NOT NULL,
    PhuongThuc      NVARCHAR(50)        NOT NULL, -- N'VNPAY', N'MoMo', N'Ví điện tử'
    SoTien          DECIMAL(18,2)       NOT NULL,
    TrangThai       BIT                 NOT NULL DEFAULT 0, -- 0 = Chờ thanh toán, 1 = Thành công
    NgayThanhToan   DATETIME            NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_ThanhToan             PRIMARY KEY (ID),
    CONSTRAINT FK_ThanhToan_HoaDon      FOREIGN KEY (IDHoaDon) REFERENCES HoaDon(ID),
    CONSTRAINT CK_ThanhToan_PhuongThuc  CHECK (PhuongThuc IN (N'VNPAY', N'MoMo', N'Ví điện tử', N'Thẻ ngân hàng')),
    CONSTRAINT CK_ThanhToan_SoTien      CHECK (SoTien >= 0)
);
GO

-- ============================================================
-- 21. DanhGiaBinhLuan (Reviews & Comments)
-- ============================================================
CREATE TABLE DanhGiaBinhLuan (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDBaiXe         INT                 NOT NULL,
    IDDatCho        INT                 NULL, -- Chỉ cho phép đánh giá nếu đã từng đặt chỗ tại bãi này
    DiemDanhGia     INT                 NOT NULL, -- 1-5 sao
    NoiDungBinhLuan NVARCHAR(MAX)       NULL,
    NgayTao         DATETIME            NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_DanhGiaBinhLuan       PRIMARY KEY (ID),
    CONSTRAINT FK_DanhGia_KhachHang     FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_DanhGia_BaiXe         FOREIGN KEY (IDBaiXe) REFERENCES BaiXe(ID),
    CONSTRAINT FK_DanhGia_DatCho        FOREIGN KEY (IDDatCho) REFERENCES DatCho(ID),
    CONSTRAINT UQ_DanhGia_DatCho        UNIQUE (IDDatCho), -- Mỗi đặt chỗ chỉ được đánh giá 1 lần duy nhất
    CONSTRAINT CK_DanhGia_Diem          CHECK (DiemDanhGia BETWEEN 1 AND 5)
);
GO

-- ============================================================
-- 22. PhienChat (Chat Sessions)
-- ============================================================
CREATE TABLE PhienChat (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDChuBai        INT                 NOT NULL,
    NgayBatDau      DATETIME            NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_PhienChat             PRIMARY KEY (ID),
    CONSTRAINT FK_PhienChat_KhachHang   FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_PhienChat_ChuBai      FOREIGN KEY (IDChuBai) REFERENCES ChuBaiXe(ID),
    CONSTRAINT UQ_PhienChat_Khach_Chu   UNIQUE (IDKhachHang, IDChuBai) -- 1 luồng chat duy nhất giữa khách hàng và chủ bãi
);
GO

-- ============================================================
-- 23. TinNhan (Chat Messages)
-- ============================================================
CREATE TABLE TinNhan (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDPhienChat     INT                 NOT NULL,
    IDTaiKhoanGui   INT                 NOT NULL, -- Tài khoản của người gửi tin nhắn (khách hàng hoặc chủ bãi)
    NoiDung         NVARCHAR(MAX)       NOT NULL,
    NgayGui         DATETIME            NOT NULL DEFAULT GETDATE(),
    DaDoc           BIT                 NOT NULL DEFAULT 0, -- 0 = Chưa đọc, 1 = Đã đọc

    CONSTRAINT PK_TinNhan               PRIMARY KEY (ID),
    CONSTRAINT FK_TinNhan_PhienChat     FOREIGN KEY (IDPhienChat) REFERENCES PhienChat(ID) ON DELETE CASCADE,
    CONSTRAINT FK_TinNhan_TaiKhoanGui   FOREIGN KEY (IDTaiKhoanGui) REFERENCES TaiKhoan(ID)
);
GO

-- ============================================================
-- 24. KhieuNai (Complaints Filed to Admin)
-- ============================================================
CREATE TABLE KhieuNai (
    ID              INT IDENTITY(1,1)   NOT NULL,
    IDKhachHang     INT                 NOT NULL,
    IDBaiXe         INT                 NOT NULL,
    IDDatCho        INT                 NULL, -- Liên kết với mã đặt chỗ bị lỗi/gặp sự cố (nếu có)
    TieuDe          NVARCHAR(150)       NOT NULL,
    NoiDung         NVARCHAR(MAX)       NOT NULL,
    NgayGui         DATETIME            NOT NULL DEFAULT GETDATE(),
    TrangThai       NVARCHAR(50)        NOT NULL DEFAULT N'Chờ xử lý', -- N'Chờ xử lý', N'Đang xử lý', N'Đã giải quyết', N'Từ chối'
    IDAdminXuLy     INT                 NULL, -- Admin chịu trách nhiệm giải quyết
    GhiChuAdmin     NVARCHAR(MAX)       NULL,

    CONSTRAINT PK_KhieuNai              PRIMARY KEY (ID),
    CONSTRAINT FK_KhieuNai_KhachHang    FOREIGN KEY (IDKhachHang) REFERENCES KhachHang(ID),
    CONSTRAINT FK_KhieuNai_BaiXe        FOREIGN KEY (IDBaiXe) REFERENCES BaiXe(ID),
    CONSTRAINT FK_KhieuNai_DatCho       FOREIGN KEY (IDDatCho) REFERENCES DatCho(ID),
    CONSTRAINT FK_KhieuNai_Admin        FOREIGN KEY (IDAdminXuLy) REFERENCES Admin(ID),
    CONSTRAINT CK_KhieuNai_TrangThai    CHECK (TrangThai IN (N'Chờ xử lý', N'Đang xử lý', N'Đã giải quyết', N'Từ chối'))
);
GO


-- ============================================================
-- SEED DATA (DỮ LIỆU MẪU ĐẦY ĐỦ VÀ CHUẨN HÓA)
-- ============================================================

-- 1. Thêm Tỉnh Thành
INSERT INTO TinhThanh (MaTinh, TenTinh) VALUES 
('81', N'Gia Lai'),
('43', N'Đà Nẵng');
GO

-- 2. Thêm Quận Huyện
INSERT INTO QuanHuyen (MaHuyen, MaTinh, TenHuyen) VALUES 
('811', '81', N'Thành phố Pleiku'),
('812', '81', N'Thị xã An Khê'),
('431', '43', N'Quận Hải Châu');
GO

-- 3. Thêm Xã Phường
INSERT INTO XaPhuong (MaXa, MaHuyen, TenXa) VALUES 
('8121', '812', N'Phường Tây Sơn'),
('8122', '812', N'Phường An Bình'),
('8111', '811', N'Phường Diên Hồng'),
('4311', '431', N'Phường Hải Châu I');
GO

-- 4. Thêm Vai trò
INSERT INTO VaiTro (TenVaiTro) VALUES 
(N'Admin'),
(N'Khách hàng'),
(N'Chủ bãi xe');
GO

-- 5. Thêm Loại Xe
INSERT INTO LoaiXe (TenLoaiXe) VALUES 
(N'Xe máy'),
(N'Ô tô'),
(N'Xe bán tải'),
(N'Xe đạp điện');
GO

-- 6. Thêm Tài khoản
INSERT INTO TaiKhoan (IDVaiTro, TenDangNhap, MatKhau, AnhDaiDien, TrangThai) VALUES
((SELECT ID FROM VaiTro WHERE TenVaiTro = N'Admin'), 'admin01', '123456', NULL, 1),
((SELECT ID FROM VaiTro WHERE TenVaiTro = N'Khách hàng'), 'khachhang01', '123456', NULL, 1),
((SELECT ID FROM VaiTro WHERE TenVaiTro = N'Khách hàng'), 'khachhang02', '123456', NULL, 1),
((SELECT ID FROM VaiTro WHERE TenVaiTro = N'Chủ bãi xe'), 'owner01@gmail.com', 'owner_pass_123', NULL, 1);
GO

-- 7. Thêm Khách hàng
INSERT INTO KhachHang (IDTaiKhoan, HoTen, SDT, Email, CCCD, BangLaiXe, MaXa, DiaChiChiTiet) VALUES
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang01'), 
    N'Nguyễn Văn A', 
    '0912345678', 
    'nguyenvana@gmail.com', 
    '123456789001', 
    'BLX001', 
    '8121', 
    N'Số 12 Nguyễn Huệ'
),
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang02'), 
    N'Trần Thị B', 
    '0987654321', 
    'tranthib@gmail.com', 
    '123456789002', 
    'BLX002', 
    '4311', 
    N'120 Bạch Đằng'
);
GO

-- 8. Thêm Chủ bãi xe
INSERT INTO ChuBaiXe (IDTaiKhoan, TenChuBai, SDT, Email, CCCD, MaXa, DiaChiChiTiet) VALUES
(
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'owner01@gmail.com'), 
    N'Lê Văn Chủ', 
    '0909999999', 
    'owner01@gmail.com', 
    '999999999999', 
    '8121', 
    N'45 Quang Trung'
);
GO

-- 9. Thêm Admin
INSERT INTO Admin (IDTaiKhoan, HoTen, SDT, Email) VALUES
((SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'admin01'), N'Trần Admin', '0905111222', 'admin@smartparking.com');
GO

-- 10. Đơn Đăng ký bãi xe (Dành cho chức năng Đăng ký & Xét duyệt bãi xe)
INSERT INTO DangKyBaiXe (TenChuBai, SDT, Email, CCCD, TenBai, MaXa, DiaChiChiTiet, SucChua, HinhAnh, GiayPhepKinhDoanh, NgayGui, TrangThai, GhiChu) VALUES
(N'Lê Văn Chủ', '0909999999', 'owner01@gmail.com', '999999999999', N'Bãi Xe Thông Minh Quang Trung', '8121', N'45 Quang Trung, An Khê', 100, 'bairuixe_quangtrung.jpg', 'gpkd_quangtrung.pdf', '2026-05-20 08:00:00', N'Đã duyệt', N'Hồ sơ hợp lệ, đã tự động tạo tài khoản cho chủ bãi.'),
(N'Nguyễn Văn Khách', '0935123456', 'owner02@gmail.com', '888888888888', N'Bãi Xe Đỗ Diên Hồng', '8111', N'78 Hùng Vương, Pleiku', 50, 'bairuixe_dienhong.jpg', 'gpkd_dienhong.pdf', '2026-05-21 12:00:00', N'Chờ duyệt', NULL);
GO

-- 11. Bãi xe (Đã được duyệt)
INSERT INTO BaiXe (IDChuBai, IDDangKy, TenBai, MaXa, DiaChiChiTiet, SucChua, PhanTramChietKhau, TrangThai, HinhAnh) VALUES
(
    (SELECT ID FROM ChuBaiXe WHERE Email = 'owner01@gmail.com'), 
    (SELECT ID FROM DangKyBaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'),
    N'Bãi Xe Thông Minh Quang Trung', 
    '8121', 
    N'45 Quang Trung, Thị xã An Khê', 
    100, 
    10.00, -- Admin hưởng 10% doanh thu mỗi lần khách thuê
    N'Hoạt động', 
    'bairuixe_quangtrung.jpg'
);
GO

-- 12. Thêm Xe
INSERT INTO Xe (BienSoXe, IDLoaiXe, TenXe, Hang, MauSac) VALUES
('81A-12345', (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Ô tô'), N'Vios', N'Toyota', N'Trắng'),
('81B1-67890', (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Xe máy'), N'Vision', N'Honda', N'Đỏ');
GO

-- Liên kết Xe - Khách hàng
INSERT INTO KhachHang_Xe (IDKhachHang, IDXe, LoaiSoHuu) VALUES
((SELECT ID FROM KhachHang WHERE CCCD = '123456789001'), '81A-12345', N'Cá nhân'),
((SELECT ID FROM KhachHang WHERE CCCD = '123456789002'), '81B1-67890', N'Cá nhân');
GO

-- 13. Khu vực của bãi đỗ
INSERT INTO KhuVuc (IDBaiXe, IDLoaiXe, TenKhuVuc, SucChua) VALUES
((SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'), (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Ô tô'), N'Khu A - Ô tô', 50),
((SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'), (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Xe máy'), N'Khu B - Xe máy', 50);
GO

-- 14. Chỗ đậu xe cụ thể (Mỗi chỗ đậu tương ứng với 1 Barrier/Lock thông minh)
INSERT INTO ChoDauXe (IDKhuVuc, TenChoDau, KichThuoc, MaSoKhoa, TrangThaiKhoa, TrangThaiO) VALUES
((SELECT ID FROM KhuVuc WHERE TenKhuVuc = N'Khu A - Ô tô'), 'A-01', '5x2.5m', 'LOCK-QT-A01', N'Đóng', N'Đang đỗ'),
((SELECT ID FROM KhuVuc WHERE TenKhuVuc = N'Khu A - Ô tô'), 'A-02', '5x2.5m', 'LOCK-QT-A02', N'Đóng', N'Trống'),
((SELECT ID FROM KhuVuc WHERE TenKhuVuc = N'Khu A - Ô tô'), 'A-03', '5x2.5m', 'LOCK-QT-A03', N'Mở', N'Đã đặt'),
((SELECT ID FROM KhuVuc WHERE TenKhuVuc = N'Khu B - Xe máy'), 'B-01', '2x1m', 'LOCK-QT-B01', N'Đóng', N'Trống');
GO

-- 15. Bảng giá
INSERT INTO BangGia (IDBaiXe, IDLoaiXe, TenBangGia, GiaTheoGio, GiaQuaDem, GiaTheoThang, GiaDatCho, TrangThai) VALUES
((SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'), (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Ô tô'), N'Bảng Giá Ô Tô', 20000, 80000, 600000, 10000, 1),
((SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'), (SELECT ID FROM LoaiXe WHERE TenLoaiXe = N'Xe máy'), N'Bảng Giá Xe Máy', 5000, 15000, 100000, 3000, 1);
GO

-- 16. Đặt chỗ mẫu
-- Khách hàng A đặt chỗ ô tô A-03 từ 14:00 đến 18:00 ngày 21/05/2026 (4 giờ)
-- Đơn giá: GiaDatCho (10,000) + 4 * GiaTheoGio (20,000) = 90,000. Tiền cọc: 10,000.
INSERT INTO DatCho (IDKhachHang, IDChoDau, BienSoXe, NgayDat, TgianBatDau, TgianKetThuc, TienCoc, TrangThai) VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    (SELECT ID FROM ChoDauXe WHERE TenChoDau = 'A-03'),
    '81A-12345',
    '2026-05-21 10:00:00',
    '2026-05-21 14:00:00',
    '2026-05-21 18:00:00',
    10000.00,
    N'Đã đặt'
);
GO

-- Khách hàng A đặt chỗ đỗ trước đó và đã đỗ thành công tại vị trí A-01 (đã hoàn thành giao dịch)
INSERT INTO DatCho (IDKhachHang, IDChoDau, BienSoXe, NgayDat, TgianBatDau, TgianKetThuc, TienCoc, TrangThai) VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    (SELECT ID FROM ChoDauXe WHERE TenChoDau = 'A-01'),
    '81A-12345',
    '2026-05-21 07:00:00',
    '2026-05-21 08:00:00',
    '2026-05-21 10:00:00',
    10000.00,
    N'Hoàn thành'
);
GO

-- 17. Nhật ký điều khiển barrier (Khách hàng tự thao tác thông qua ứng dụng)
INSERT INTO LogDieuKhienBarrier (IDDatCho, IDTaiKhoan, ThoiGianLệnh, HanhDong, KetQua, GhiChu) VALUES
(
    (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành'),
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang01'),
    '2026-05-21 07:58:00',
    N'Mở khóa',
    N'Thành công',
    N'Khách hàng kích hoạt mở barie thông qua điện thoại di động khi đến vị trí đỗ.'
),
(
    (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành'),
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang01'),
    '2026-05-21 09:59:00',
    N'Khóa lại',
    N'Thành công',
    N'Khách hàng hoàn thành lượt đỗ và ra khỏi bãi đỗ xe.'
);
GO

-- 18. Hóa đơn (Thống kê chiết khấu và lợi nhuận)
-- Tổng hóa đơn đỗ xe A-01 là 50,000 VNĐ.
-- Chiết khấu admin thu 10% (5,000 VNĐ). Chủ bãi nhận 90% (45,000 VNĐ)
INSERT INTO HoaDon (IDDatCho, TongTien, TienChietKhauAdmin, TienChuBaiNhan, NgayTao, TrangThai) VALUES
(
    (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành'),
    50000.00,
    5000.00,
    45000.00,
    '2026-05-21 10:01:00',
    N'Đã thanh toán'
);
GO

-- 19. Thanh toán hóa đơn
INSERT INTO ThanhToan (IDHoaDon, PhuongThuc, SoTien, TrangThai, NgayThanhToan) VALUES
(
    (SELECT ID FROM HoaDon WHERE IDDatCho = (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành')),
    N'VNPAY',
    50000.00,
    1,
    '2026-05-21 10:02:00'
);
GO

-- 20. Đánh giá và bình luận (Nội dung đánh giá thực tế của khách hàng)
INSERT INTO DanhGiaBinhLuan (IDKhachHang, IDBaiXe, IDDatCho, DiemDanhGia, NoiDungBinhLuan, NgayTao) VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    (SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'),
    (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành'),
    5,
    N'Bãi xe tự động thông minh, tôi tự mở khóa barie rất tiện, không lo xe khác đỗ nhầm chỗ.',
    '2026-05-21 10:15:00'
);
GO

-- 21. Phân hệ Chat Nhắn tin trực tuyến (Hỗ trợ thắc mắc khách hàng và chủ bãi)
INSERT INTO PhienChat (IDKhachHang, IDChuBai, NgayBatDau) VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    (SELECT ID FROM ChuBaiXe WHERE Email = 'owner01@gmail.com'),
    '2026-05-21 13:00:00'
);
GO

INSERT INTO TinNhan (IDPhienChat, IDTaiKhoanGui, NoiDung, NgayGui, DaDoc) VALUES
(
    (SELECT ID FROM PhienChat WHERE IDKhachHang = (SELECT ID FROM KhachHang WHERE CCCD = '123456789001')),
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'khachhang01'),
    N'Chào bạn, bãi xe của mình ô tô bán tải gầm cao có đỗ vừa chỗ A-03 không vậy?',
    '2026-05-21 13:01:00',
    1
),
(
    (SELECT ID FROM PhienChat WHERE IDKhachHang = (SELECT ID FROM KhachHang WHERE CCCD = '123456789001')),
    (SELECT ID FROM TaiKhoan WHERE TenDangNhap = 'owner01@gmail.com'),
    N'Chào bạn, chỗ đỗ A-03 có kích thước rộng rãi 5m x 2.5m, hoàn toàn vừa vặn cho xe bán tải nhé!',
    '2026-05-21 13:03:00',
    0
);
GO

-- 22. Phân hệ Khiếu nại (Gửi cho Admin giải quyết tranh chấp chính thức)
INSERT INTO KhieuNai (IDKhachHang, IDBaiXe, IDDatCho, TieuDe, NoiDung, NgayGui, TrangThai, IDAdminXuLy, GhiChuAdmin) VALUES
(
    (SELECT ID FROM KhachHang WHERE CCCD = '123456789001'),
    (SELECT ID FROM BaiXe WHERE TenBai = N'Bãi Xe Thông Minh Quang Trung'),
    (SELECT ID FROM DatCho WHERE TrangThai = N'Hoàn thành'),
    N'Lỗi trừ tiền hai lần',
    N'Tôi thanh toán 50.000 VNĐ qua VNPAY nhưng tài khoản ngân hàng báo trừ hai lần giao dịch. Mong Admin kiểm tra và hoàn lại.',
    '2026-05-21 11:00:00',
    N'Đã giải quyết',
    (SELECT ID FROM Admin WHERE HoTen = N'Trần Admin'),
    N'Đã xác minh ngân hàng đối tác bị duplicate lệnh giao dịch. Đã lập lệnh hoàn trả 50.000 VNĐ cho khách ngày 21/05/2026.'
);
GO
