# 🛡️ Phân Hệ Admin (Admin Area)

Thành viên được phân công làm tính năng **Admin** sẽ làm việc hoàn toàn trong thư mục này.

## 📁 Cấu trúc thư mục khuyến nghị:
- `Controllers/`: Chứa các controller quản lý của Admin (ví dụ: `ApproveParkingController.cs`, `ReportController.cs`). Các controller này phải được khai báo với thuộc tính `[Area("Admin")]`.
- `Models/`: Chứa các ViewModel hoặc InputModel dùng riêng cho giao diện Admin.
- `Views/`: Chứa các file giao diện `.cshtml` của Admin.

## ⚠️ Quy tắc phát triển:
1. Đảm bảo tất cả Controller đều có attribute `[Area("Admin")]` ở trên cùng của class.
2. Không chỉnh sửa các file thuộc phân hệ của các thành viên khác (`Areas/Owner` hoặc `Areas/Customer`).
3. Đăng ký các dịch vụ bổ sung của bạn trong `WebApplication1/Extensions/AdminServiceRegistration.cs` thay vì sửa đổi `Program.cs`.
