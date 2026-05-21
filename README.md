# 🚗 Smart Parking Management System (SPMS) - ASP.NET Core MVC & SQL Server

Dự án **Quản Lý Bãi Đỗ Xe Thông Minh (Smart Parking Management System - SPMS)** là ứng dụng web được xây dựng trên mô hình **ASP.NET Core Web App (Model-View-Controller)** sử dụng **.NET 8.0** và hệ quản trị cơ sở dữ liệu **Microsoft SQL Server**. 

Hệ thống hướng tới việc tự động hóa toàn bộ quy trình đỗ xe đô thị mà không cần nhân sự trực bãi thông qua hệ thống barie/khóa thông minh ở mỗi chỗ đỗ xe.

---

## 📝 Giới Thiệu Đề Tài

**SPMS** là giải pháp chuyển đổi số toàn diện kết nối giữa **Khách hàng** có nhu cầu đỗ xe và các **Chủ bãi xe**:
- **Khách hàng:** Tìm kiếm bãi xe theo cấp hành chính (Tỉnh - Huyện - Xã), đặt chỗ trước, thanh toán trực tuyến và **tự nắm quyền điều khiển mở/đóng Barie** của vị trí đỗ thông qua giao diện Web sau khi đặt chỗ thành công.
- **Chủ bãi xe:** Đăng ký bãi xe trực tuyến (yêu cầu hình ảnh, vị trí, giấy phép), thiết lập sơ đồ bãi đỗ (chia Khu vực và các Chỗ đỗ có gắn Barie), cấu hình giá vé, phản hồi Chat hỗ trợ khách hàng và theo dõi thống kê doanh thu thực nhận.
- **Quản trị viên (Admin):** Kiểm duyệt hồ sơ bãi xe (hệ thống tự động cấp tài khoản chủ bãi và gửi email thông tin đăng nhập), cấu hình tỷ lệ hoa hồng chiết khấu trên mỗi lượt đỗ xe và quản lý khiếu nại tài chính.

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

### 1. Backend & Hệ Thống
- **Framework chính:** **ASP.NET Core Web App (Model-View-Controller)** (.NET 8.0)
- **Cơ sở dữ liệu:** **Microsoft SQL Server**
- **ORM (Object-Relational Mapping):** **Entity Framework Core (EF Core)**
- **Xác thực & Phân quyền:** Cookie-based Authentication (`Microsoft.AspNetCore.Authentication.Cookies`) kết hợp phân quyền theo Vai trò (Claim-based Roles: Admin, Chủ bãi xe, Khách hàng).
- **Thời gian thực (Real-time):** **SignalR** phục vụ truyền tin nhắn Chat trực tuyến tức thời.
- **Dịch vụ nền (Background Worker):** `IHostedService` / `BackgroundService` tự động kiểm tra giải phóng chỗ đỗ và cập nhật trạng thái đơn đặt chỗ sang "Quá hạn" (Expired) nếu khách hàng không đến đúng giờ.
- **Thanh toán & Gửi mail:** Cổng VNPAY API, thư viện MailKit (SMTP) để tự động gửi thông báo mật khẩu cho chủ bãi.

### 2. Frontend
- **Template Engine:** Razor Pages (`.cshtml`)
- **UI Framework:** Bootstrap 5, FontAwesome 6, SweetAlert2 (thông báo đẹp mắt)
- **Javascript Libraries:** jQuery, SignalR Client, Chart.js (vẽ biểu đồ doanh thu trực quan)

---

## 👥 Phân Quyền & Tính Năng Chi Tiết (Actors & Use Cases)

```mermaid
graph TD
    A[Hệ thống Smart Parking] --> B(Khách Hàng)
    A --> C(Chủ Bãi Xe)
    A --> D(Admin Hệ Thống)

    B --> B1[Đăng ký/Đăng nhập & Quản lý xe]
    B --> B2[Tìm kiếm bãi xe theo Tỉnh/Huyện/Xã]
    B --> B3[Đặt chỗ trước & Thanh toán VNPAY]
    B --> B4[Điều khiển Barie/Khóa thông minh tại chỗ đỗ]
    B --> B5[Chat thời gian thực & Đánh giá/Khiếu nại]

    C --> C1[Đăng ký bãi đỗ & Gửi hồ sơ hình ảnh]
    C --> C2[Cấu hình Khu vực, Chỗ đỗ & Mã hóa Barie]
    C --> C3[Thiết lập bảng giá vé dịch vụ]
    C --> C4[Thống kê doanh số sau khi trừ chiết khấu]
    C --> C5[Chat trực tuyến tư vấn khách hàng]

    D --> D1[Xét duyệt đơn đăng ký & Gửi Email mật khẩu tự động]
    D --> D2[Quản lý tài khoản người dùng]
    D --> D3[Thống kê hoa hồng chiết khấu hệ thống]
    D --> D4[Admin tiếp nhận & Giải quyết khiếu nại]
```

### 1. Khách Hàng (Customer)
*   **Đăng ký/Đăng nhập:** Tạo tài khoản, đăng nhập hệ thống, cập nhật thông tin cá nhân.
*   **Quản lý xe:** Lưu danh sách biển số xe cá nhân để rút ngắn thời gian làm thủ tục đặt chỗ.
*   **Tìm kiếm & Lọc bãi xe:** Tìm kiếm bãi xe theo Tỉnh -> Huyện -> Xã. Lọc theo giá vé tốt nhất hoặc đánh giá cao nhất.
*   **Đặt chỗ & Thanh toán:** Chọn thời gian gửi dự kiến, thanh toán trực tuyến (VNPAY/MoMo) để hoàn tất giữ chỗ.
*   **Điều khiển Barie thông minh:** 
    *   Trong thời gian đơn đặt chỗ có hiệu lực, khách hàng được cấp quyền điều khiển Barie của chỗ đỗ đó.
    *   Khách hàng nhấn nút **"Mở Barie"** trên giao diện web để đỗ xe vào vị trí, và nhấn **"Khóa lại/Đóng Barie"** khi rời bãi.
*   **Giao tiếp & Phản hồi:**
    *   **Chat trực tuyến (SignalR):** Nhắn tin trực tiếp hỏi đáp kỹ thuật với chủ bãi xe.
    *   **Đơn khiếu nại:** Gửi khiếu nại đến Admin nếu gặp sự cố thanh toán lỗi hoặc chỗ đỗ bị chiếm dụng.
    *   **Đánh giá (Verified Review):** Chỉ cho phép đánh giá số sao (1-5★) và bình luận sau khi đã thực hiện giao dịch đỗ xe thành công.

### 2. Chủ Bãi Xe (Parking Owner)
*   **Đăng ký bãi xe:** Đăng tải thông tin chi tiết, hình ảnh thực tế bãi xe, số điện thoại, email liên hệ và giấy phép kinh doanh. Nhận email chứa tài khoản và mật khẩu ngẫu nhiên sau khi được Admin phê duyệt.
*   **Quản lý Sơ đồ bãi đỗ:**
    *   Chia bãi xe thành nhiều **Khu vực** (ví dụ: Khu A dành cho Ô tô, Khu B dành cho Xe máy) để dễ dàng quản lý.
    *   Tạo danh sách các **Chỗ đỗ xe** trong từng khu vực, cấu hình mã API định danh của khóa thông minh (`MaSoKhoa`) tương ứng.
*   **Quản lý Bảng giá:** Thiết lập mức phí giữ chỗ (`GiaDatCho`), giá đỗ theo giờ (`GiaTheoGio`), giá đỗ qua đêm (`GiaQuaDem`).
*   **Hỗ trợ khách hàng:** Chat trực tuyến phản hồi thắc mắc của khách hàng tại bãi xe.
*   **Báo cáo doanh số:** Biểu đồ doanh thu thực nhận (đã khấu trừ chiết khấu hoa hồng của Admin) theo ngày/tuần/tháng/năm và xuất file Excel.

### 3. Admin Hệ Thống (Administrator)
*   **Xét duyệt hồ sơ đối tác:** Kiểm duyệt hình ảnh, giấy phép bãi xe đăng ký mới. Bấm Duyệt để kích hoạt bãi xe, đồng thời hệ thống tự động gọi SMTP Server gửi email thông tin đăng nhập cho chủ bãi.
*   **Quản trị tài khoản:** Quản lý tất cả tài khoản khách hàng, chủ bãi xe; khóa các tài khoản vi phạm.
*   **Thống kê tài chính toàn hệ thống:** Thống kê tổng doanh thu thu nhập từ phí hoa hồng chiết khấu trên mỗi hóa đơn đặt chỗ của các bãi xe đối tác.
*   **Giải quyết khiếu nại:** Tiếp nhận khiếu nại từ khách hàng và làm trung gian phân xử hoàn tiền hoặc cảnh cáo chủ bãi.

---

## 📁 Cấu Trúc Mã Nguồn Dự Án

Mã nguồn được tổ chức theo cấu trúc phân lớp chuẩn của một dự án ASP.NET Core MVC doanh nghiệp:

```text
CUOIKICSHARP/
│
├── src/
│   ├── SmartParking.Web/             # Lớp giao diện (Presentation Layer)
│   │   ├── Areas/                   # Phân hệ quản lý
│   │   │   ├── Admin/               # Quản lý xét duyệt đơn bãi xe, thống kê chiết khấu
│   │   │   └── Owner/               # Quản lý cấu hình khóa barie, chat trực tiếp
│   │   ├── Controllers/             # Xử lý các request (Account, Booking, Chat, Barrier...)
│   │   ├── Models/                  # ViewModels, InputModels hiển thị và nhận dữ liệu
│   │   ├── Views/                   # Razor Views (.cshtml) chứa mã HTML/CSS/JS
│   │   ├── Hubs/                    # SignalR Hubs (ChatHub.cs xử lý tin nhắn thời gian thực)
│   │   ├── wwwroot/                 # Thư mục chứa CSS, JS, hình ảnh bãi xe tải lên
│   │   ├── Program.cs               # File cấu hình Services, DI Container và Route chính
│   │   └── appsettings.json         # Cấu hình SQL Server Connection String và API VNPAY
│   │
│   ├── SmartParking.Data/            # Lớp truy cập cơ sở dữ liệu (Data Access Layer)
│   │   ├── Context/                 # SmartParkingDbContext kết nối SQL Server
│   │   ├── Entities/                # Định nghĩa các bảng trong DB (User, ParkingLot, Booking, XaPhuong...)
│   │   └── Migrations/              # Quản lý các phiên bản cập nhật database của EF Core
│   │
│   └── SmartParking.Service/         # Lớp xử lý nghiệp vụ logic (Business Logic Layer)
│       ├── Interfaces/              # Khai báo các dịch vụ (IEmailService, IBarrierService, IPaymentService...)
│       └── Implementations/         # Hiện thực hóa logic nghiệp vụ (gửi mail, thanh toán VNPAY, mở khóa barie)
│
├── database/                        # Chứa file script tạo CSDL SQL Server [db_QuanLyBaiDoXe.sql]
├── docs/                            # Tài liệu phân tích thiết kế hệ thống, sơ đồ cơ sở dữ liệu
└── README.md                        # Tài liệu hướng dẫn này
```

---

## ⚙️ Cấu Hình Hệ Thống & Cài Đặt

### 1. Yêu cầu hệ thống
- **Cơ sở dữ liệu:** Microsoft SQL Server 2019 trở lên (Express hoặc LocalDB).
- **Môi trường chạy:** .NET SDK 8.0.
- **Công cụ phát triển (IDE):** Visual Studio 2022.

### 2. Thiết lập cơ sở dữ liệu
Hệ thống sử dụng cơ sở dữ liệu đã chuẩn hóa cấu trúc phân cấp địa chỉ hành chính Việt Nam và cấu trúc barie thông minh.
1. Mở **SQL Server Management Studio (SSMS)**.
2. Kết nối tới SQL Server của bạn.
3. Mở file [db_QuanLyBaiDoXe.sql](file:///d:/HK225/CSHARP/CUOIKICSHARP/database/db_QuanLyBaiDoXe.sql) và nhấn **Execute (F5)** để tạo Database `QuanLyBaiXe` và nạp sẵn dữ liệu mẫu.

### 3. Cấu hình chuỗi kết nối trong `appsettings.json`
Mở file `src/SmartParking.Web/appsettings.json` và cấu hình lại chuỗi kết nối SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER_NAME;Database=QuanLyBaiXe;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Smart Parking System",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "VnPay": {
    "TmnCode": "YOUR_VNPAY_TMNCODE",
    "HashKey": "YOUR_VNPAY_HASHKEY",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
  }
}
```

### 4. Thiết lập Dependency Injection & Routing (`Program.cs`)
Trong file `Program.cs`, dự án cấu hình kết nối SQL Server và các dịch vụ bổ trợ:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using SmartParking.Data.Context;
using SmartParking.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối SQL Server qua Entity Framework Core
builder.Services.AddDbContext<SmartParkingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Cookie Authentication & Phân quyền
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// 3. Đăng ký các Business Services (Dependency Injection)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBarrierService, BarrierService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// 4. Kích hoạt SignalR cho chức năng Chat trực tuyến thời gian thực
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình Routing cho phân hệ Areas (Admin và Owner) và Route mặc định
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");

app.Run();
```

---

## 🚀 Cách Chạy Dự Án

### Sử dụng Visual Studio 2022:
1. Mở file solution `.sln` trong thư mục `src`.
2. Kiểm tra xem dự án **SmartParking.Web** đã được chọn làm **Startup Project** chưa (Nhấp chuột phải vào dự án -> *Set as Startup Project*).
3. Nhấn **F5** hoặc bấm vào nút **Start** màu xanh để build và chạy ứng dụng.

### Sử dụng .NET CLI (Terminal):
Di chuyển vào thư mục dự án web và chạy lệnh:
```bash
cd src/SmartParking.Web
dotnet run
```
Trình duyệt sẽ tự động mở hoặc bạn có thể truy cập qua địa chỉ: `https://localhost:7001` hoặc `http://localhost:5001`.

---

## 👥 Thành Viên Thực Hiện

| MSSV | Họ và Tên | Vai Trò / Nhiệm Vụ |
| :--- | :--- | :--- |
| `12345678` | [Tên Thành Viên 1] | Trưởng nhóm, Thiết kế DB, Phát triển Backend (Phần Admin & Chủ bãi) |
| `87654321` | [Tên Thành Viên 2] | Thiết kế UI/UX, Phát triển Frontend & Chức năng Tìm kiếm, Đặt chỗ |
| `11223344` | [Tên Thành Viên 3] | Tích hợp cổng thanh toán trực tuyến, Giao thức kết nối Barrier & Viết báo cáo |

---

## 📝 Giấy Phép (License)

Dự án này được phân phối dưới giấy phép **MIT License** - xem chi tiết tại file [LICENSE](LICENSE) (nếu có).
