using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Generic;
using WebApplication1.Models.DataContext;
using System.Linq;
using WebApplication1.Models.Entities;
using System.Threading.Tasks;
using System;

namespace WebApplication1.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Route("Owner/[controller]")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetThongKe")]
        public async Task<IActionResult> GetThongKe(int? baiXeId, string range = "30days")
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var taiKhoanId))
            {
                return Unauthorized();
            }

            var chuBai = await _context.ChuBaiXes
                .Include(c => c.BaiXes)
                .FirstOrDefaultAsync(c => c.IDTaiKhoan == taiKhoanId);

            if (chuBai == null)
            {
                return Forbid();
            }

            var baiXeIds = chuBai.BaiXes.Select(b => b.ID).ToList();
            if (baiXeId.HasValue)
            {
                if (!baiXeIds.Contains(baiXeId.Value)) return Forbid();
                baiXeIds = new List<int> { baiXeId.Value };
            }

            var today = DateTime.Today;
            DateTime start;
            switch ((range ?? "30days").ToLower())
            {
                case "today": start = today; break;
                case "7days": start = today.AddDays(-6); break;
                case "30days": start = today.AddDays(-29); break;
                case "year": start = new DateTime(today.Year, 1, 1); break;
                default: start = today.AddDays(-29); break;
            }

            // Load invoices with related navigations (DatCho -> ChoDauXe -> KhuVuc -> BaiXe/LoaiXe)
            var hoaDons = await _context.HoaDons
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.BaiXe)
                .Include(h => h.DatCho)
                    .ThenInclude(d => d.ChoDauXe)
                        .ThenInclude(cd => cd.KhuVuc)
                            .ThenInclude(k => k.LoaiXe)
                .Include(h => h.ThanhToans)
                .Where(h => h.NgayTao >= start
                            && (h.ThanhToans.Any(t => t.TrangThai) || h.TrangThai == "Đã thanh toán")
                            && (h.DatCho != null && h.DatCho.ChoDauXe != null && h.DatCho.ChoDauXe.KhuVuc != null && baiXeIds.Contains(h.DatCho.ChoDauXe.KhuVuc.IDBaiXe)))
                .ToListAsync();

            // Map license plates to Xe with LoaiXe (DatCho doesn't have Xe nav property)
            var plateNumbers = hoaDons
                .Select(h => h.DatCho?.BienSoXe)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            var xeMap = new Dictionary<string, string>(); // plate -> TenLoai
            if (plateNumbers.Any())
            {
                var xes = await _context.Xes
                    .Include(x => x.LoaiXe)
                    .Where(x => plateNumbers.Contains(x.BienSoXe))
                    .ToListAsync();

                foreach (var xe in xes)
                {
                    xeMap[xe.BienSoXe] = xe.LoaiXe?.TenLoaiXe ?? string.Empty;
                }
            }

            var tongDoanhThu = hoaDons.Sum(x => x.TienChuBaiNhan);
            var luotDoXe = hoaDons.Count;
            var doanhThuTB = luotDoXe > 0 ? tongDoanhThu / luotDoXe : 0;

            // Transactions payload
            var giaoDich = hoaDons
                .OrderByDescending(x => x.NgayTao)
                .Take(50)
                .Select(x => {
                    var bien = x.DatCho?.BienSoXe;
                    var typeName = string.Empty;
                    if (!string.IsNullOrEmpty(bien) && xeMap.ContainsKey(bien))
                    {
                        typeName = xeMap[bien];
                    }
                    else
                    {
                        typeName = x.DatCho?.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? string.Empty;
                    }

                    return new
                    {
                        id = x.ID,
                        bienSo = bien,
                        lotName = x.DatCho?.ChoDauXe?.KhuVuc?.BaiXe?.TenBai ?? string.Empty,
                        spotName = x.DatCho?.ChoDauXe?.TenChoDau ?? string.Empty,
                        type = typeName,
                        tongTien = x.TongTien,
                        thoiGian = x.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        trangThai = x.TrangThai
                    };
                })
                .ToList();

            // Chart aggregation
            List<string> chartLabels = new();
            List<decimal> chartData = new();

            if ((range ?? "30days").ToLower() == "today")
            {
                // hourly
                for (int h = 0; h < 24; h++)
                {
                    var hourStart = today.AddHours(h);
                    var sum = hoaDons.Where(x => x.NgayTao.Date == today && x.NgayTao.Hour == h).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add(h.ToString("D2") + ":00");
                    chartData.Add(sum);
                }
            }
            else if ((range ?? "30days").ToLower() == "7days")
            {
                for (int i = 0; i < 7; i++)
                {
                    var d = start.AddDays(i);
                    var sum = hoaDons.Where(x => x.NgayTao.Date == d.Date).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add(d.ToString("ddd", new CultureInfo("vi-VN")));
                    chartData.Add(sum);
                }
            }
            else if ((range ?? "30days").ToLower() == "30days")
            {
                // 4 weekly buckets
                for (int w = 0; w < 4; w++)
                {
                    var weekStart = start.AddDays(w * 7);
                    var weekEnd = weekStart.AddDays(7).AddTicks(-1);
                    var sum = hoaDons.Where(x => x.NgayTao >= weekStart && x.NgayTao <= weekEnd).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add($"Tuần {w + 1}");
                    chartData.Add(sum);
                }
            }
            else // year
            {
                for (int m = 1; m <= 12; m++)
                {
                    var monthStart = new DateTime(today.Year, m, 1);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                    var sum = hoaDons.Where(x => x.NgayTao >= monthStart && x.NgayTao <= monthEnd).Sum(x => x.TienChuBaiNhan);
                    chartLabels.Add("Tháng " + m);
                    chartData.Add(sum);
                }
            }

            // Vehicle breakdown by LoaiXe
            var breakdown = hoaDons
                .Select(h => {
                    var bien = h.DatCho?.BienSoXe;
                    var t = "Khác";
                    if (!string.IsNullOrEmpty(bien) && xeMap.ContainsKey(bien)) t = xeMap[bien];
                    else t = h.DatCho?.ChoDauXe?.KhuVuc?.LoaiXe?.TenLoaiXe ?? "Khác";
                    return new { type = t, value = h.TienChuBaiNhan };
                })
                .GroupBy(x => x.type)
                .Select(g => new { type = g.Key ?? "Khác", value = g.Sum(x => x.value) })
                .OrderByDescending(x => x.value)
                .ToList();

            var doughnutLabels = breakdown.Select(b => b.type).ToList();
            var doughnutData = breakdown.Select(b => (decimal)b.value).ToList();

            return Json(new
            {
                tongDoanhThu,
                luotDoXe,
                doanhThuTB,
                giaoDich,
                chartLabels,
                chartData,
                doughnutLabels,
                doughnutData
            });
        }
    }
}
