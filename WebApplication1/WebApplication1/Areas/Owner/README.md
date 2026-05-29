# 🔑 Phân Hệ Chủ Bãi Xe (Owner Area)

Thành viên được phân công làm tính năng **Chủ Bãi Xe (Owner)** sẽ làm việc hoàn toàn trong thư mục này.

## 📁 Cấu trúc thư mục khuyến nghị:
- `Controllers/`: Chứa các controller của Chủ bãi (ví dụ: `ParkingLotManagerController.cs`, `PricingController.cs`, `OwnerChatController.cs`). Các controller này phải được khai báo với thuộc tính `[Area("Owner")]`.
- `Models/`: Chứa các ViewModel hoặc InputModel dùng riêng cho Chủ bãi xe.
- `Views/`: Chứa các file giao diện `.cshtml` của Chủ bãi.

## ⚠️ Quy tắc phát triển:
1. Đảm bảo tất cả Controller đều có attribute `[Area("Owner")]` ở trên cùng của class.
2. Không chỉnh sửa các file thuộc phân hệ của các thành viên khác (`Areas/Admin` hoặc `Areas/Customer`).
3. Đăng ký các dịch vụ bổ sung của bạn trong `WebApplication1/Extensions/OwnerServiceRegistration.cs` thay vì sửa đổi `Program.cs`.
