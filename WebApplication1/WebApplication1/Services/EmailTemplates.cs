namespace WebApplication1.Services
{
    public static class EmailTemplates
    {
        private static string Wrap(string tenChuBai, string tieuDe, string noiDung, string mauBadge, string nhanBadge)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f0f2f5;font-family:Segoe UI,sans-serif'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr><td align='center' style='padding:32px 16px'>
      <table width='560' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)'>
        <!-- Header -->
        <tr><td style='background:#4f46e5;padding:28px 32px'>
          <div style='color:#fff;font-size:20px;font-weight:700'>🅿️ SmartParking SPMS</div>
          <div style='color:#c7d2fe;font-size:13px;margin-top:4px'>Hệ thống quản lý bãi đỗ xe thông minh</div>
        </td></tr>
        <!-- Body -->
        <tr><td style='padding:32px'>
          <div style='font-size:22px;font-weight:700;color:#111827;margin-bottom:8px'>{tieuDe}</div>
          <div style='margin-bottom:20px'>
            <span style='background:{mauBadge};color:#fff;padding:4px 14px;border-radius:20px;font-size:13px;font-weight:600'>{nhanBadge}</span>
          </div>
          <p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>Xin chào <strong>{tenChuBai}</strong>,</p>
          {noiDung}
          <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0'>
          <p style='color:#6b7280;font-size:13px;margin:0'>
            Email này được gửi tự động từ hệ thống SmartParking SPMS.<br>
            Vui lòng không trả lời email này.
          </p>
        </td></tr>
        <!-- Footer -->
        <tr><td style='background:#f8fafc;padding:16px 32px;text-align:center'>
          <div style='color:#9ca3af;font-size:12px'>© 2026 SmartParking SPMS · support@smartparking.vn</div>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        public static (string subject, string html) GuiOtp(string tenNguoiDung, string otp, string loai)
        {
            var subject = $"🔐 Mã xác thực OTP đăng ký {loai} - SmartParking";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Bạn vừa yêu cầu đăng ký tài khoản <strong>{loai}</strong> trên hệ thống SmartParking.
</p>
<div style='text-align:center;margin:24px 0'>
  <div style='display:inline-block;background:#4f46e5;color:#fff;font-size:32px;font-weight:800;
              letter-spacing:8px;padding:16px 32px;border-radius:14px'>
    {otp}
  </div>
</div>
<div style='background:#fef9c3;border:1.5px solid #fde047;border-radius:10px;padding:14px 18px;margin-bottom:16px'>
  <div style='font-size:13px;color:#92400e'>
    ⏱️ Mã OTP có hiệu lực trong <strong>10 phút</strong>.<br>
    🔒 Không chia sẻ mã này với bất kỳ ai.
  </div>
</div>
<p style='color:#6b7280;font-size:13px;margin:0'>
  Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.
</p>";
            return (subject, Wrap(tenNguoiDung, "Xác Thực Email Đăng Ký", noi, "#4f46e5", "🔐 OTP"));
        }

        public static (string subject, string html) DuyetBaiVoiTaiKhoan(
            string tenChuBai, string tenBai, string diaChi, string email, string matKhau)
        {
            var subject = $"✅ Bãi xe \"{tenBai}\" đã được duyệt — Thông tin đăng nhập của bạn";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Chúc mừng! Bãi xe của bạn đã được <strong>phê duyệt</strong> và chính thức hoạt động trên hệ thống SmartParking.
</p>
<div style='background:#f0fdf4;border:1.5px solid #86efac;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>🗺️ Địa chỉ:</strong> {diaChi}</div>
  <div style='font-size:14px;color:#16a34a'><strong>✅ Trạng thái:</strong> Hoạt động</div>
</div>
<div style='background:#eff6ff;border:1.5px solid #93c5fd;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:15px;font-weight:700;color:#1d4ed8;margin-bottom:12px'>🔑 Thông Tin Đăng Nhập</div>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'>
    <strong>Tên đăng nhập:</strong>
    <code style='background:#dbeafe;padding:2px 8px;border-radius:6px;font-size:13px'>{email}</code>
  </div>
  <div style='font-size:14px;color:#374151;margin-bottom:12px'>
    <strong>Mật khẩu:</strong>
    <code style='background:#dbeafe;padding:2px 8px;border-radius:6px;font-size:13px'>{matKhau}</code>
  </div>
  <div style='font-size:13px;color:#dc2626'>⚠️ Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu.</div>
</div>
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Đăng nhập tại: <a href='http://localhost:5200/Account/Login' style='color:#4f46e5'>SmartParking SPMS</a>
</p>";
            return (subject, Wrap(tenChuBai, "Bãi Xe Đã Được Duyệt", noi, "#16a34a", "✅ Đã duyệt"));
        }

        public static (string subject, string html) DuyetBai(string tenChuBai, string tenBai, string diaChi)
        {
            var subject = $"✅ Bãi xe \"{tenBai}\" đã được duyệt hoạt động";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Chúng tôi vui mừng thông báo rằng bãi xe của bạn đã được <strong>phê duyệt</strong> và chính thức hoạt động trên hệ thống SmartParking.
</p>
<div style='background:#f0fdf4;border:1.5px solid #86efac;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>🗺️ Địa chỉ:</strong> {diaChi}</div>
  <div style='font-size:14px;color:#16a34a'><strong>✅ Trạng thái:</strong> Hoạt động</div>
</div>
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Khách hàng đã có thể tìm kiếm và đặt chỗ tại bãi xe của bạn. Hãy đăng nhập vào hệ thống để quản lý bãi xe và theo dõi doanh thu.
</p>";
            return (subject, Wrap(tenChuBai, "Bãi Xe Đã Được Duyệt", noi, "#16a34a", "✅ Đã duyệt"));
        }

        public static (string subject, string html) TuChoiBai(string tenChuBai, string tenBai, string lyDo)
        {
            var subject = $"❌ Đơn đăng ký bãi xe \"{tenBai}\" bị từ chối";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Rất tiếc, đơn đăng ký bãi xe của bạn đã bị <strong>từ chối</strong> sau khi xem xét.
</p>
<div style='background:#fef2f2;border:1.5px solid #fca5a5;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:#dc2626'><strong>❌ Lý do từ chối:</strong> {lyDo}</div>
</div>
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Bạn có thể chỉnh sửa thông tin và gửi lại đơn đăng ký sau khi khắc phục các vấn đề trên.
</p>";
            return (subject, Wrap(tenChuBai, "Đơn Đăng Ký Bị Từ Chối", noi, "#dc2626", "❌ Từ chối"));
        }

        public static (string subject, string html) TamDongBai(string tenChuBai, string tenBai, string lyDo)
        {
            var subject = $"⏸️ Bãi xe \"{tenBai}\" bị tạm đóng";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Bãi xe của bạn đã bị <strong>tạm đóng</strong> bởi quản trị viên hệ thống.
</p>
<div style='background:#fefce8;border:1.5px solid #fde047;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:#ca8a04'><strong>⏸️ Lý do:</strong> {lyDo}</div>
</div>
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Trong thời gian tạm đóng, khách hàng sẽ không thể đặt chỗ tại bãi xe của bạn. Vui lòng liên hệ với chúng tôi để được hỗ trợ.
</p>";
            return (subject, Wrap(tenChuBai, "Bãi Xe Bị Tạm Đóng", noi, "#ca8a04", "⏸️ Tạm đóng"));
        }

        public static (string subject, string html) MoKhoaBai(string tenChuBai, string tenBai)
        {
            var subject = $"🔓 Bãi xe \"{tenBai}\" đã được mở khóa";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Bãi xe của bạn đã được <strong>mở khóa</strong> và hoạt động trở lại bình thường.
</p>
<div style='background:#f0fdf4;border:1.5px solid #86efac;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:#16a34a'><strong>✅ Trạng thái:</strong> Hoạt động</div>
</div>
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Khách hàng đã có thể đặt chỗ tại bãi xe của bạn. Chúc bạn kinh doanh thuận lợi!
</p>";
            return (subject, Wrap(tenChuBai, "Bãi Xe Đã Được Mở Khóa", noi, "#16a34a", "✅ Hoạt động"));
        }

        public static (string subject, string html) DoiTrangThai(string tenChuBai, string tenBai, string trangThaiMoi, string? lyDo)
        {
            var (mau, icon) = trangThaiMoi switch {
                "Hoạt động" => ("#16a34a", "✅"),
                "Tạm đóng"  => ("#ca8a04", "⏸️"),
                "Bảo trì"   => ("#6b7280", "🔧"),
                _           => ("#4f46e5", "ℹ️")
            };
            var subject = $"{icon} Bãi xe \"{tenBai}\" đổi trạng thái: {trangThaiMoi}";
            var noi = $@"
<p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 16px'>
  Trạng thái bãi xe của bạn đã được cập nhật bởi quản trị viên.
</p>
<div style='background:#f8fafc;border:1.5px solid #e5e7eb;border-radius:12px;padding:20px;margin-bottom:20px'>
  <div style='font-size:14px;color:#374151;margin-bottom:8px'><strong>📍 Tên bãi xe:</strong> {tenBai}</div>
  <div style='font-size:14px;color:{mau};margin-bottom:8px'><strong>{icon} Trạng thái mới:</strong> {trangThaiMoi}</div>
  {(string.IsNullOrEmpty(lyDo) ? "" : $"<div style='font-size:14px;color:#374151'><strong>📝 Ghi chú:</strong> {lyDo}</div>")}
</div>";
            return (subject, Wrap(tenChuBai, "Cập Nhật Trạng Thái Bãi Xe", noi, mau, $"{icon} {trangThaiMoi}"));
        }
    }
}
