using System.Collections.Concurrent;

namespace WebApplication1.Services
{
    /// <summary>
    /// Lưu OTP tạm thời trong memory (hết hạn sau 10 phút)
    /// </summary>
    public class OtpService
    {
        private readonly ConcurrentDictionary<string, (string Otp, DateTime HetHan)> _store = new();

        public string TaoOtp(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _store[email.ToLower()] = (otp, DateTime.Now.AddMinutes(10));
            return otp;
        }

        public bool XacThucOtp(string email, string otp)
        {
            var key = email.ToLower();
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.HetHan > DateTime.Now && entry.Otp == otp)
                {
                    _store.TryRemove(key, out _); // dùng 1 lần
                    return true;
                }
            }
            return false;
        }

        public bool DaGuiOtp(string email) =>
            _store.TryGetValue(email.ToLower(), out var e) && e.HetHan > DateTime.Now;
    }
}
