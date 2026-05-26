using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Hubs;
using WebApplication1.Models.DataContext;
using WebApplication1.Models.Entities;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    [Authorize(Roles = "Chủ bãi xe")]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCurrentOwnerId()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("AccountId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                return id;
            }
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var tk = _context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == username);
                if (tk != null) return tk.ID;
            }
            return 0;
        }

        private int GetChuBaiId(int accountId)
        {
            var chuBai = _context.ChuBaiXes.FirstOrDefault(c => c.IDTaiKhoan == accountId);
            return chuBai != null ? chuBai.ID : 0;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int? activeChatId)
        {
            int ownerAccountId = GetCurrentOwnerId();
            if (ownerAccountId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Load all chat sessions for this owner
            var chatSessions = await _context.PhienChats
                .Include(pc => pc.KhachHang)
                .Include(pc => pc.TinNhans)
                .Where(pc => pc.IDChuBai == chuBaiId)
                .ToListAsync();

            // Find active chat session
            PhienChat? activeChatSession = null;
            if (activeChatId.HasValue)
            {
                activeChatSession = chatSessions.FirstOrDefault(pc => pc.ID == activeChatId.Value);
            }
            if (activeChatSession == null)
            {
                activeChatSession = chatSessions.FirstOrDefault();
            }

            // Load messages for the active session
            List<TinNhan> messages = new List<TinNhan>();
            if (activeChatSession != null)
            {
                messages = await _context.TinNhans
                    .Where(m => m.IDPhienChat == activeChatSession.ID)
                    .OrderBy(m => m.NgayGui)
                    .ToListAsync();

                // Mark unread messages from customer as read
                var unreadMessages = messages.Where(m => m.IDTaiKhoanGui != ownerAccountId && !m.DaDoc).ToList();
                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.DaDoc = true;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.ChatSessions = chatSessions;
            ViewBag.ActiveSession = activeChatSession;
            ViewBag.Messages = messages;
            ViewBag.UserAccountId = ownerAccountId;

            return View();
        }

        // POST: /Owner/Chat/SendChatMessage
        [HttpPost("SendChatMessage")]
        public async Task<IActionResult> SendChatMessage(int sessionId, string content)
        {
            int ownerAccountId = GetCurrentOwnerId();
            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0 || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });
            }

            var session = await _context.PhienChats.FirstOrDefaultAsync(pc => pc.ID == sessionId && pc.IDChuBai == chuBaiId);
            if (session == null)
            {
                return Json(new { success = false, message = "Phiên chat không tồn tại." });
            }

            var message = new TinNhan
            {
                IDPhienChat = sessionId,
                IDTaiKhoanGui = ownerAccountId,
                NoiDung = content,
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _context.TinNhans.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast the message via SignalR to all users in this session group
            await _hubContext.Clients.Group($"Session_{sessionId}").SendAsync("ReceiveMessage", new
            {
                id = message.ID,
                senderId = message.IDTaiKhoanGui,
                content = message.NoiDung,
                time = message.NgayGui.ToString("HH:mm")
            });

            return Json(new { success = true, messageId = message.ID, time = message.NgayGui.ToString("HH:mm") });
        }

        // GET: /Owner/Chat/GetChatMessages
        [HttpGet("GetChatMessages")]
        public async Task<IActionResult> GetChatMessages(int sessionId)
        {
            int ownerAccountId = GetCurrentOwnerId();
            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0)
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var session = await _context.PhienChats.FirstOrDefaultAsync(pc => pc.ID == sessionId && pc.IDChuBai == chuBaiId);
            if (session == null)
            {
                return Json(new { success = false, message = "Phiên chat không tồn tại." });
            }

            var messages = await _context.TinNhans
                .Where(m => m.IDPhienChat == sessionId)
                .OrderBy(m => m.NgayGui)
                .Select(m => new
                {
                    id = m.ID,
                    senderId = m.IDTaiKhoanGui,
                    content = m.NoiDung,
                    time = m.NgayGui.ToString("HH:mm"),
                    isMe = m.IDTaiKhoanGui == ownerAccountId
                })
                .ToListAsync();

            // Mark unread messages from customer as read
            var unreadMessages = await _context.TinNhans
                .Where(m => m.IDPhienChat == sessionId && m.IDTaiKhoanGui != ownerAccountId && !m.DaDoc)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.DaDoc = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, messages = messages });
        }

        // GET: /Owner/Chat/GetCustomerDetails
        [HttpGet("GetCustomerDetails")]
        public async Task<IActionResult> GetCustomerDetails(int customerId)
        {
            int ownerAccountId = GetCurrentOwnerId();
            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0)
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var customer = await _context.KhachHangs
                .Include(k => k.DatChos)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd!.KhuVuc)
                            .ThenInclude(kv => kv.BaiXe)
                .FirstOrDefaultAsync(k => k.ID == customerId);

            if (customer == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            // Get owner's lot IDs
            var ownerLotIds = await _context.BaiXes
                .Where(b => b.IDChuBai == chuBaiId)
                .Select(b => b.ID)
                .ToListAsync();

            // Filter bookings that belong to this owner's lots
            var ownerRelatedBookings = customer.DatChos
                .Where(d => d.ChoDauXe != null && d.ChoDauXe.KhuVuc != null && ownerLotIds.Contains(d.ChoDauXe.KhuVuc.IDBaiXe))
                .ToList();

            var totalBookings = ownerRelatedBookings.Count;

            // Format "Member since" - earliest booking at this owner's lots, or if none, current year
            var earliestBooking = ownerRelatedBookings.OrderBy(d => d.NgayDat).FirstOrDefault();
            var memberSince = earliestBooking != null
                ? $"Tháng {earliestBooking.NgayDat.Month}/{earliestBooking.NgayDat.Year}"
                : $"Năm {DateTime.Now.Year}";

            // Active/Current bookings: status is "Đã đặt" or "Đang đỗ"
            var activeBookings = ownerRelatedBookings
                .Where(d => d.TrangThai == "Đã đặt" || d.TrangThai == "Đang đỗ")
                .Select(d => new
                {
                    code = $"BK-{d.ID}",
                    lot = d.ChoDauXe?.KhuVuc?.BaiXe?.TenBai ?? "Bãi xe",
                    spot = d.ChoDauXe?.TenChoDau ?? "Vị trí",
                    date = d.TgianBatDau.ToString("dd/MM/yyyy")
                })
                .ToList();

            return Json(new
            {
                success = true,
                name = customer.HoTen,
                phone = customer.SDT ?? "Chưa có",
                email = customer.Email ?? "Chưa có",
                status = "online", // Default status
                totalBookings = totalBookings,
                since = memberSince,
                bookings = activeBookings
            });
        }

        [HttpGet("GetUnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int ownerAccountId = GetCurrentOwnerId();
            if (ownerAccountId == 0)
            {
                return Ok(new { count = 0 });
            }

            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0)
            {
                return Ok(new { count = 0 });
            }

            var unreadCount = await _context.TinNhans
                .CountAsync(m => m.PhienChat.IDChuBai == chuBaiId && m.IDTaiKhoanGui != ownerAccountId && !m.DaDoc);

            return Json(new { count = unreadCount });
        }

        [HttpGet("GetOwnerSessions")]
        public async Task<IActionResult> GetOwnerSessions()
        {
            int ownerAccountId = GetCurrentOwnerId();
            if (ownerAccountId == 0)
            {
                return Json(new List<int>());
            }

            int chuBaiId = GetChuBaiId(ownerAccountId);
            if (chuBaiId == 0)
            {
                return Json(new List<int>());
            }

            var sessionIds = await _context.PhienChats
                .Where(pc => pc.IDChuBai == chuBaiId)
                .Select(pc => pc.ID)
                .ToListAsync();

            return Json(sessionIds);
        }
    }
}
