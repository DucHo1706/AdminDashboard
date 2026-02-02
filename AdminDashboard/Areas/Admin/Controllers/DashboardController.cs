using AdminDashboard.Areas.Admin.Components;
using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.TransportDBContext;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly Db27524Context _context;
        public DashboardController(Db27524Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Thongke()
        {
            var model = new ThongKeViewModel();
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);

            model.TongDoanhThu = await _context.DonHang
                .Where(d => d.TrangThaiThanhToan == "Da thanh toan")
                .SumAsync(d => d.TongTien);

            model.TongDonHang = await _context.DonHang.CountAsync();

            model.TongSoNguoiDung = await _context.NguoiDung.CountAsync();

            model.TongNhaXe = await _context.NhaXe.CountAsync();

            model.TongTram = await _context.Tram.CountAsync();

            var chuyenHomNay = await _context.ChuyenXes
                .Where(c => c.NgayDi.Date == today)
                .ToListAsync();

            model.SoChuyenXeHomNay = chuyenHomNay.Count;

            int countSapChay = chuyenHomNay.Count(c => (int)c.TrangThai >= 0 && (int)c.TrangThai <= 2);
            int countDangChay = chuyenHomNay.Count(c => (int)c.TrangThai == 3);
            int countHoanThanh = chuyenHomNay.Count(c => (int)c.TrangThai == 4);
            int countHuy = chuyenHomNay.Count(c => (int)c.TrangThai == 5);

            model.StatusLabels = new List<string> { "Sắp chạy", "Đang chạy", "Hoàn thành", "Hủy" };
            model.StatusData = new List<int> { countSapChay, countDangChay, countHoanThanh, countHuy };

            var revenueData = await _context.DonHang
                .Where(d => d.NgayDat >= sevenDaysAgo && d.TrangThaiThanhToan == "Da thanh toan")
                .GroupBy(d => d.NgayDat.Date)
                .Select(g => new {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .ToListAsync();

            for (int i = 0; i <= 6; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                var record = revenueData.FirstOrDefault(r => r.Ngay == date);
                model.LabelsBieuDo.Add(date.ToString("dd/MM"));
                model.DataBieuDo.Add(record != null ? record.DoanhThu : 0);
            }

            model.CacChuyenXeSapChay = await _context.ChuyenXes
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation) 
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation) 
                .Include(c => c.TaiXe)
                .Where(c => c.NgayDi >= today)
                .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi)
                .Take(5)
                .Select(c => new ChuyenXeDashboardItem
                {
                    ChuyenId = c.ChuyenId,
                    TenLoTrinh = (c.LoTrinh.TramDiNavigation != null ? c.LoTrinh.TramDiNavigation.TenTram : c.LoTrinh.TramDi)
                                 + " ➝ " +
                                 (c.LoTrinh.TramToiNavigation != null ? c.LoTrinh.TramToiNavigation.TenTram : c.LoTrinh.TramToi),
                    
                    TenTaiXe = c.TaiXe != null ? c.TaiXe.HoTen : null,
                    GioDi = c.GioDi,
                    NgayDi = c.NgayDi,
                    PhanTramLapDay = 60,
                    TrangThai = "Sắp chạy"
                })
                .ToListAsync();


            model.TopLoTrinh = await _context.DonHang

    .Include(d => d.ChuyenXe)
        .ThenInclude(cx => cx.LoTrinh)
        .ThenInclude(lt => lt.TramDiNavigation) // Lấy thông tin Trạm Đi
    .Include(d => d.ChuyenXe)
        .ThenInclude(cx => cx.LoTrinh)
        .ThenInclude(lt => lt.TramToiNavigation) // Lấy thông tin Trạm Đến
    .Where(d => d.TrangThaiThanhToan == "Da thanh toan") 
    .GroupBy(d => new {
        d.ChuyenXe.LoTrinhId,
        TenTramDi = d.ChuyenXe.LoTrinh.TramDiNavigation.TenTram,
        TenTramToi = d.ChuyenXe.LoTrinh.TramToiNavigation.TenTram
    })
    .Select(g => new TopLoTrinhItem
    {
        TenLoTrinh = g.Key.TenTramDi + " ➝ " + g.Key.TenTramToi,
        DoanhThu = g.Sum(x => x.TongTien),
        SoVeBan = g.Count()
    })
    .OrderByDescending(x => x.DoanhThu)
    .Take(5)
    .ToListAsync();

            return View(model);
        }
    }
}