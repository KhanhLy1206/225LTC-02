using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.DataContext;

namespace WebApplication1.Extensions
{
    public static class BookingExtensions
    {
        public static async Task AutoReleaseExpiredBookingsAsync(this AppDbContext context)
        {
            var now = DateTime.Now;

            // Find all active bookings (TrangThai is "Đã đặt" or "Đang đỗ") that have expired (TgianKetThuc < now)
            var expiredBookings = await context.DatChos
                .Include(d => d.ChoDauXe)
                .Where(d => (d.TrangThai == "Đã đặt" || d.TrangThai == "Đang đỗ") && d.TgianKetThuc < now)
                .ToListAsync();

            if (expiredBookings.Any())
            {
                foreach (var booking in expiredBookings)
                {
                    // Update booking status
                    if (booking.TrangThai == "Đang đỗ")
                    {
                        booking.TrangThai = "Hoàn thành";
                    }
                    else if (booking.TrangThai == "Đã đặt")
                    {
                        booking.TrangThai = "Quá hạn";
                    }

                    // Release the spot if it's currently occupied by this booking (and not in Maintenance / Bảo trì)
                    if (booking.ChoDauXe != null && booking.ChoDauXe.TrangThaiO != "Bảo trì")
                    {
                        booking.ChoDauXe.TrangThaiO = "Trống";
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
