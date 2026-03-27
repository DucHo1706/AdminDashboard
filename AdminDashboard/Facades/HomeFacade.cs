using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;
using System.Linq; 

namespace AdminDashboard.Facades
{
    // Interface định nghĩa các hành động "đơn giản" mà Controller cần
    public interface IHomeFacade
    {
        List<TuyenXeViewModel> LayTuyenXeNoiBat();
        List<ChuyenXe> LayChuyenXeHomNay();

        LichSuMuaVeViewModel LayLichSuDonHang(string userId);
    }

    public class HomeFacade : IHomeFacade
    {
        private readonly Db27524Context _context;

        public HomeFacade(Db27524Context context)
        {
            _context = context;
        }

        public List<TuyenXeViewModel> LayTuyenXeNoiBat()
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayInVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).Date;

            var allUpcomingTrips = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Images)
                // Thay today bằng todayInVietnam
                .Where(c => c.NgayDi.Date >= todayInVietnam &&
                            c.TrangThai == TrangThaiChuyenXe.DangMoBanVe)
                .ToList();

            // Logic gom nhóm và chuyển đổi sang ViewModel (đưa logic phức tạp vào đây)
            var routesByDeparture = allUpcomingTrips
                .Where(c => c.LoTrinh?.TramDiNavigation != null)
                .GroupBy(c => new {
                    TenTram = c.LoTrinh.TramDiNavigation.TenTram,
                    Tinh = c.LoTrinh.TramDiNavigation.Tinh ?? "",
                    ImageUrl = c.Images?.FirstOrDefault()?.ImageUrl ?? "/images/slider/hcm.png"
                })
                .Select(g => new TuyenXeViewModel
                {
                    Tinh = g.Key.Tinh,
                    TenTram = g.Key.TenTram,
                    ImageUrl = g.Key.ImageUrl,
                    TuyenXe = g.Where(c => c.LoTrinh?.TramToiNavigation != null)
                        .GroupBy(c => c.LoTrinh.TramToiNavigation.TenTram)
                        .Select(group => group.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi).First())
                        .Select(c => new TuyenXeItemViewModel
                        {
                            ChuyenId = c.ChuyenId,
                            DiemDen = c.LoTrinh.TramToiNavigation.TenTram,
                            NgayDi = c.NgayDi,
                            GioDi = c.GioDi,
                            GioDenDuKien = c.GioDenDuKien,
                            ThoiGian = (c.GioDenDuKien - c.GioDi).TotalHours >= 1
                                ? $"{(int)(c.GioDenDuKien - c.GioDi).TotalHours} giờ"
                                : $"{(int)((c.GioDenDuKien - c.GioDi).TotalMinutes)} phút",
                            GiaVe = c.LoTrinh.GiaVeCoDinh ?? 0,
                            ImageUrl = c.Images?.FirstOrDefault()?.ImageUrl ?? g.Key.ImageUrl
                        })
                        .OrderBy(t => t.DiemDen)
                        .Take(3)
                        .ToList()
                })
                .Where(r => r.TuyenXe != null && r.TuyenXe.Any())
                .Take(3)
                .ToList();

            return routesByDeparture;
        }

        public List<ChuyenXe> LayChuyenXeHomNay()
        {
            // 1. Ép lấy múi giờ Việt Nam (UTC+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var todayInVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).Date;

            // 2. Truy vấn Database dựa trên giờ Việt Nam
            var chuyenXeHomNay = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Images)
                // Dùng biến todayInVietnam.Date để so sánh
                .Where(c => c.NgayDi.Date == todayInVietnam)
                .ToList();

            return chuyenXeHomNay;
        }

        public LichSuMuaVeViewModel LayLichSuDonHang(string userId)
        {
            // 1. Lấy tất cả đơn hàng của user từ Database (Kèm thông tin chuyến xe, lộ trình)
            var allOrders = _context.DonHang
                .Where(d => d.IDKhachHang == userId)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(d => d.ChuyenXe).ThenInclude(cx => cx.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            // 2. Khởi tạo ViewModel
            var viewModel = new LichSuMuaVeViewModel();

            // 3. Lọc danh sách "Hiện tại"
            // (Logic cũ: Chưa hoàn thành chuyến VÀ (Đang chờ thanh toán HOẶC Đã thanh toán))
            viewModel.DonHangHienTai = allOrders
                .Where(d => d.ChuyenXe != null)
                .Where(d => d.ChuyenXe.TrangThai != TrangThaiChuyenXe.DaHoanThanh)
                .Where(d => d.TrangThaiThanhToan == "DangChoThanhToan" || d.TrangThaiThanhToan == "Da thanh toan")
                .OrderBy(d => d.ChuyenXe.NgayDi).ThenBy(d => d.ChuyenXe.GioDi)
                .ToList();

            // 4. Lọc danh sách "Đã đi"
            // (Logic cũ: Đã thanh toán VÀ Chuyến xe đã hoàn thành)
            viewModel.DonHangDaDi = allOrders
                .Where(d => d.ChuyenXe != null)
                .Where(d => d.TrangThaiThanhToan == "Da thanh toan")
                .Where(d => d.ChuyenXe.TrangThai == TrangThaiChuyenXe.DaHoanThanh)
                .OrderByDescending(d => d.ChuyenXe.NgayDi)
                .ToList();

            // 5. Lọc danh sách "Đã hủy"
            viewModel.DonHangDaHuy = allOrders
                .Where(d => d.TrangThaiThanhToan == "Da huy" || d.TrangThaiThanhToan == "Giao dich that bai")
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return viewModel;
        }
    }
}
