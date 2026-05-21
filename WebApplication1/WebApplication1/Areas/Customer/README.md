# 🚗 Phân Hệ Khách Hàng (Customer Area)

Thành viên được phân công làm tính năng **Khách Hàng (Customer)** sẽ làm việc hoàn toàn trong thư mục này.

## 📁 Cấu trúc thư mục khuyến nghị:
- `Controllers/`: Chứa các controller của Khách hàng (ví dụ: `SearchController.cs`, `BookingController.cs`, `VehicleController.cs`). Các controller này phải được khai báo với thuộc tính `[Area("Customer")]`.
- `Models/`: Chứa các ViewModel hoặc InputModel dùng riêng cho Khách hàng.
- `Views/`: Chứa các file giao diện `.cshtml` của Khách hàng.

## ⚠️ Quy tắc phát triển:
1. Đảm bảo tất cả Controller đều có attribute `[Area("Customer")]` ở trên cùng của class.
2. Không chỉnh sửa các file thuộc phân hệ của các thành viên khác (`Areas/Admin` hoặc `Areas/Owner`).
3. Đăng ký các dịch vụ bổ sung của bạn trong `WebApplication1/Extensions/CustomerServiceRegistration.cs` thay vì sửa đổi `Program.cs`.
