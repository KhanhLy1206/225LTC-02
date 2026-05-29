# TỔNG QUAN PHẦN PUBLIC & CUSTOMER (BÁO CÁO THUYẾT TRÌNH)

---

## 1. Cấu trúc thư mục & File đã thực hiện

### 📂 PHẦN PUBLIC (Mọi người dùng truy cập được)
* **`Controllers/HomeController.cs`**:
  * Action `Index`: Hiển thị trang chủ giới thiệu dự án (Landing page).
* **`Controllers/AccountController.cs`**:
  * Action `Register`: Tiếp nhận đăng ký tài khoản khách hàng mới.
  * Action `Login`: Xác thực tài khoản khách hàng, thiết lập Cookie Authentication và phân quyền truy cập.
  * Action `Logout`: Đăng xuất tài khoản.
* **`Views/Home/Index.cshtml`**: Giao diện giới thiệu trang chủ.
* **`Views/Account/Login.cshtml` & `Register.cshtml`**: Giao diện đăng nhập và đăng ký tài khoản.

### 📂 PHẦN CUSTOMER (Khách hàng sau khi đăng nhập)
* **`Areas/Customer/Controllers/HomeController.cs`**:
  * Action `Index` / `Search`: Tìm kiếm bãi đỗ xe và xem sơ đồ chỗ đỗ trống trực quan.
  * Action `BookSpot`: Khởi tạo thông tin đặt chỗ và sinh đường dẫn sang cổng VNPAY.
  * Action `VnpayReturn`: Tiếp nhận kết quả thanh toán từ VNPAY, xác thực chữ ký bảo mật và cập nhật trạng thái đơn hàng.
  * Action `ControlBarrier`: Tiếp nhận lệnh mở/đóng Barrier vật lý từ khách hàng và ghi nhật ký hoạt động.
* **`Areas/Customer/Views/Home/Search.cshtml`**: Bản đồ bãi xe hiển thị trực quan các ô đỗ trống (xanh), đầy (đỏ), đang bảo trì (xám) và xử lý AJAX gửi yêu cầu đặt chỗ.
* **`Areas/Customer/Views/Home/Bookings.cshtml`**: Danh sách lịch sử đặt chỗ và các nút bấm điều khiển Barrier trực tiếp.

---

## 2. Vị trí 3 kỹ thuật cốt lõi trong phần Public & Customer

### A. Data Annotation (Chú thích dữ liệu)
* **Ánh xạ Database (Entities):** [Models/Entities/BaiXe.cs]
  * `[Table("BaiXe")]` (Ánh xạ tên bảng trong DB).
  * `[Key]` (Khóa chính), `[ForeignKey]` (Khóa ngoại).
  * `[Required]` (Trường bắt buộc), `[Range(1, 10000)]` (Khoảng giá trị hợp lệ).
* **Kiểm tra dữ liệu nhập (ViewModels):** [Models/ViewModels/RegisterViewModel.cs]
  * `[Required]` (Bắt buộc nhập), `[EmailAddress]` (Đúng cấu trúc Email), `[Compare("MatKhau")]` (Trùng khớp mật khẩu).

### B. Custom Validate (Kiểm tra tự viết)
* **Mã nguồn các bộ xác thực:** Nằm trong thư mục [Validation/]
  * `[VietnamesePhone]`: Kiểm tra số điện thoại Việt Nam hợp lệ (dùng Regex).
  * `[UniqueUsername]`: Truy vấn DB kiểm tra tên tài khoản đăng ký mới không bị trùng lặp.
  * `[UniquePlateNumber]`: Kiểm tra biển số xe đăng ký mới không trùng lặp trong hệ thống.
* **Sử dụng:**
  * Áp dụng `[UniqueUsername]` và `[VietnamesePhone]` tại [RegisterViewModel.cs] (Phần Public).
  * Áp dụng `[UniquePlateNumber]` tại [AddVehicleViewModel.cs] (Phần Customer).
* **Cách lấy DB Context động:** Trong hàm `IsValid` của Custom Validator, lấy DB Context qua Service Locator:
  ```csharp
  var context = (AppDbContext?)validationContext.GetService(typeof(AppDbContext));
  ```

### C. Dependency Injection (DI)
* **Đăng ký dịch vụ (Program.cs):**
  * `builder.Services.AddDbContext<AppDbContext>(...)` (Đăng ký DB Context).
  * `builder.Services.AddScoped<IAccountService, AccountService>()` (Đăng ký dịch vụ xử lý tài khoản).
* **Sử dụng thực tế (Constructor Injection):**
  * **Tầng Public:** Inject `IAccountService` vào constructor của [AccountController.cs] để gọi hàm đăng nhập/đăng ký.
  * **Tầng Customer:** Inject `AppDbContext` và `IConfiguration` vào constructor của [HomeController.cs] (phần Customer) để thao tác dữ liệu và cấu hình VNPAY.
