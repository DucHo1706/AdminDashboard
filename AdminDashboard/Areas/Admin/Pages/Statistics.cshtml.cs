using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace AdminDashboard.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class StatisticsModel : PageModel
    {
        private readonly Db27524Context _context;
        private const int PAGE_SIZE = 10;
        private const string KHACH_HANG_ROLE = "KhachHang";

        public StatisticsModel(Db27524Context context)
        {
            _context = context;
        }

        // Tổng quan thống kê
        public int TongKhachHang { get; set; }
        public int TongDonHang { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuTong { get; set; }

        // Thống kê khách hàng theo độ tuổi
        public Dictionary<string, int> ThongKeTheoDoTuoi { get; set; } = new();
        
        // Danh sách khách hàng (phân trang)
        public PaginatedList<NguoiDungViewModel> KhachHangs { get; set; }
        public int KhachHangPageIndex { get; set; } = 1;
        
        // Danh sách đơn hàng (phân trang)
        public PaginatedList<DonHangViewModel> DonHangs { get; set; }
        public int DonHangPageIndex { get; set; } = 1;

        // Thống kê đơn hàng theo thời gian
        public ThongKeDonHangViewModel ThongKeDonHang { get; set; } = new();

        // Filter thời gian
        public string FilterType { get; set; } = "all"; // all, day, month, year
        public DateTime? FilterDate { get; set; }
        public int? FilterMonth { get; set; }
        public int? FilterYear { get; set; }

        // Lỗi
        public string ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public async Task OnGetAsync(int? khachHangPage, int? donHangPage, string filterType = "all", 
            DateTime? filterDate = null, int? filterMonth = null, int? filterYear = null)
        {
            try
            {
                KhachHangPageIndex = khachHangPage ?? 1;
                DonHangPageIndex = donHangPage ?? 1;
                FilterType = filterType ?? "all";
                FilterDate = filterDate;
                FilterMonth = filterMonth;
                FilterYear = filterYear ?? DateTime.Now.Year;

                await LoadTongQuanThongKe();
                await LoadThongKeKhachHangTheoDoTuoi();
                await LoadThongKeDonHang();
                await LoadDanhSachKhachHang(KhachHangPageIndex);
                await LoadDanhSachDonHang(DonHangPageIndex);
            }
            catch (DbUpdateException ex)
            {
                HandleDatabaseError(ex);
            }
            catch (SqlException ex)
            {
                HandleSqlError(ex);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi không xác định: {ex.Message}";
            }
        }

        private async Task LoadTongQuanThongKe()
        {
            try
            {
                // Lấy RoleId của KhachHang
                var khachHangRoleId = await _context.VaiTro
                    .Where(r => r.TenVaiTro == KHACH_HANG_ROLE)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (khachHangRoleId != null)
                {
                    TongKhachHang = await _context.UserRole
                        .Where(ur => ur.RoleId == khachHangRoleId)
                        .CountAsync();
                }

                TongDonHang = await _context.DonHang.CountAsync();

                var today = DateTime.Today;
                DoanhThuHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today && d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DoanhThuThangNay = await _context.DonHang
                    .Where(d => d.NgayDat >= firstDayOfMonth && d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                DoanhThuTong = await _context.DonHang
                    .Where(d => d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải tổng quan thống kê: {ex.Message}", ex);
            }
        }

        private async Task LoadThongKeKhachHangTheoDoTuoi()
        {
            try
            {
                var khachHangRoleId = await _context.VaiTro
                    .Where(r => r.TenVaiTro == KHACH_HANG_ROLE)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (khachHangRoleId == null) return;

                var khachHangs = await _context.UserRole
                    .Where(ur => ur.RoleId == khachHangRoleId)
                    .Join(_context.NguoiDung,
                        ur => ur.UserId,
                        nd => nd.UserId,
                        (ur, nd) => nd)
                    .Where(nd => nd.NgaySinh != null)
                    .ToListAsync();

                var now = DateTime.Now;
                ThongKeTheoDoTuoi = new Dictionary<string, int>
                {
                    { "Dưới 18 tuổi", 0 },
                    { "18-25 tuổi", 0 },
                    { "26-35 tuổi", 0 },
                    { "36-45 tuổi", 0 },
                    { "46-60 tuổi", 0 },
                    { "Trên 60 tuổi", 0 }
                };

                foreach (var kh in khachHangs)
                {
                    if (kh.NgaySinh.HasValue)
                    {
                        var tuoi = now.Year - kh.NgaySinh.Value.Year;
                        if (kh.NgaySinh.Value.Date > now.AddYears(-tuoi)) tuoi--;

                        if (tuoi < 18)
                            ThongKeTheoDoTuoi["Dưới 18 tuổi"]++;
                        else if (tuoi <= 25)
                            ThongKeTheoDoTuoi["18-25 tuổi"]++;
                        else if (tuoi <= 35)
                            ThongKeTheoDoTuoi["26-35 tuổi"]++;
                        else if (tuoi <= 45)
                            ThongKeTheoDoTuoi["36-45 tuổi"]++;
                        else if (tuoi <= 60)
                            ThongKeTheoDoTuoi["46-60 tuổi"]++;
                        else
                            ThongKeTheoDoTuoi["Trên 60 tuổi"]++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thống kê theo độ tuổi: {ex.Message}", ex);
            }
        }

        private async Task LoadDanhSachKhachHang(int pageIndex)
        {
            try
            {
                var khachHangRoleId = await _context.VaiTro
                    .Where(r => r.TenVaiTro == KHACH_HANG_ROLE)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (khachHangRoleId == null)
                {
                    KhachHangs = new PaginatedList<NguoiDungViewModel>(new List<NguoiDungViewModel>(), 0, pageIndex, PAGE_SIZE);
                    return;
                }

                var query = _context.UserRole
                    .Where(ur => ur.RoleId == khachHangRoleId)
                    .Join(_context.NguoiDung,
                        ur => ur.UserId,
                        nd => nd.UserId,
                        (ur, nd) => nd)
                    .Select(nd => new NguoiDungViewModel
                    {
                        UserId = nd.UserId,
                        HoTen = nd.HoTen,
                        Email = nd.Email,
                        SoDienThoai = nd.SoDienThoai ?? "—",
                        NgaySinh = nd.NgaySinh,
                        Tuoi = nd.NgaySinh != null ? 
                            DateTime.Now.Year - nd.NgaySinh.Value.Year - 
                            (DateTime.Now.DayOfYear < nd.NgaySinh.Value.DayOfYear ? 1 : 0) : (int?)null,
                        TrangThai = nd.TrangThai
                    })
                    .OrderBy(nd => nd.HoTen);

                var count = await query.CountAsync();
                var items = await query
                    .Skip((pageIndex - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .ToListAsync();

                KhachHangs = new PaginatedList<NguoiDungViewModel>(items, count, pageIndex, PAGE_SIZE);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách khách hàng: {ex.Message}", ex);
            }
        }

        private async Task LoadThongKeDonHang()
        {
            try
            {
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var firstDayOfYear = new DateTime(today.Year, 1, 1);
                var lastDayOfYear = new DateTime(today.Year, 12, 31);

                // Thống kê hôm nay
                var donHangHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today)
                    .CountAsync();
                var doanhThuHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today && d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                // Thống kê tháng này
                var donHangThangNay = await _context.DonHang
                    .Where(d => d.NgayDat >= firstDayOfMonth && d.NgayDat <= lastDayOfMonth)
                    .CountAsync();
                var doanhThuThangNay = await _context.DonHang
                    .Where(d => d.NgayDat >= firstDayOfMonth && d.NgayDat <= lastDayOfMonth && d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                // Thống kê năm nay
                var donHangNamNay = await _context.DonHang
                    .Where(d => d.NgayDat >= firstDayOfYear && d.NgayDat <= lastDayOfYear)
                    .CountAsync();
                var doanhThuNamNay = await _context.DonHang
                    .Where(d => d.NgayDat >= firstDayOfYear && d.NgayDat <= lastDayOfYear && d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                // Thống kê theo filter
                var filteredQuery = _context.DonHang.AsQueryable();
                
                if (FilterType == "day" && FilterDate.HasValue)
                {
                    filteredQuery = filteredQuery.Where(d => d.NgayDat.Date == FilterDate.Value.Date);
                }
                else if (FilterType == "month" && FilterMonth.HasValue && FilterYear.HasValue)
                {
                    var startOfMonth = new DateTime(FilterYear.Value, FilterMonth.Value, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    filteredQuery = filteredQuery.Where(d => d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth);
                }
                else if (FilterType == "year" && FilterYear.HasValue)
                {
                    var startOfYear = new DateTime(FilterYear.Value, 1, 1);
                    var endOfYear = new DateTime(FilterYear.Value, 12, 31);
                    filteredQuery = filteredQuery.Where(d => d.NgayDat >= startOfYear && d.NgayDat <= endOfYear);
                }

                var donHangFiltered = await filteredQuery.CountAsync();
                var doanhThuFiltered = await filteredQuery
                    .Where(d => d.TrangThaiThanhToan == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                // Thống kê theo trạng thái hôm nay
                var daThanhToanHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today && d.TrangThaiThanhToan == "Đã thanh toán")
                    .CountAsync();
                var choThanhToanHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today && d.TrangThaiThanhToan == "DangChoThanhToan")
                    .CountAsync();
                var daHuyHomNay = await _context.DonHang
                    .Where(d => d.NgayDat.Date == today && (d.TrangThaiThanhToan == "Da huy" || d.TrangThaiThanhToan == "Đã hủy"))
                    .CountAsync();

                ThongKeDonHang = new ThongKeDonHangViewModel
                {
                    DonHangHomNay = donHangHomNay,
                    DoanhThuHomNay = doanhThuHomNay,
                    DonHangThangNay = donHangThangNay,
                    DoanhThuThangNay = doanhThuThangNay,
                    DonHangNamNay = donHangNamNay,
                    DoanhThuNamNay = doanhThuNamNay,
                    DonHangFiltered = donHangFiltered,
                    DoanhThuFiltered = doanhThuFiltered,
                    DaThanhToanHomNay = daThanhToanHomNay,
                    ChoThanhToanHomNay = choThanhToanHomNay,
                    DaHuyHomNay = daHuyHomNay
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thống kê đơn hàng: {ex.Message}", ex);
            }
        }

        private async Task LoadDanhSachDonHang(int pageIndex)
        {
            try
            {
                var query = _context.DonHang
                    .Include(d => d.nguoiDung)
                    .Include(d => d.ChuyenXe)
                    .AsQueryable();

                // Áp dụng filter
                if (FilterType == "day" && FilterDate.HasValue)
                {
                    query = query.Where(d => d.NgayDat.Date == FilterDate.Value.Date);
                }
                else if (FilterType == "month" && FilterMonth.HasValue && FilterYear.HasValue)
                {
                    var startOfMonth = new DateTime(FilterYear.Value, FilterMonth.Value, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    query = query.Where(d => d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth);
                }
                else if (FilterType == "year" && FilterYear.HasValue)
                {
                    var startOfYear = new DateTime(FilterYear.Value, 1, 1);
                    var endOfYear = new DateTime(FilterYear.Value, 12, 31);
                    query = query.Where(d => d.NgayDat >= startOfYear && d.NgayDat <= endOfYear);
                }

                var donHangQuery = query
                    .Select(d => new DonHangViewModel
                    {
                        DonHangId = d.DonHangId,
                        KhachHangId = d.IDKhachHang,
                        KhachHangTen = d.nguoiDung.HoTen,
                        ChuyenId = d.ChuyenId,
                        NgayDat = d.NgayDat,
                        TongTien = d.TongTien,
                        TrangThaiThanhToan = d.TrangThaiThanhToan,
                        ThoiGianHetHan = d.ThoiGianHetHan
                    })
                    .OrderByDescending(d => d.NgayDat);

                var count = await donHangQuery.CountAsync();
                var items = await donHangQuery
                    .Skip((pageIndex - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .ToListAsync();

                DonHangs = new PaginatedList<DonHangViewModel>(items, count, pageIndex, PAGE_SIZE);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách đơn hàng: {ex.Message}", ex);
            }
        }

        private void HandleDatabaseError(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                HandleSqlError(sqlEx);
            }
            else
            {
                ErrorMessage = $"Lỗi cập nhật cơ sở dữ liệu: {ex.Message}";
            }
        }

        private void HandleSqlError(SqlException ex)
        {
            switch (ex.Number)
            {
                case 547: // Foreign key constraint
                    ErrorMessage = "Không thể thực hiện thao tác này do vi phạm ràng buộc khóa ngoại. Vui lòng kiểm tra dữ liệu liên quan.";
                    break;
                case 2627: // Primary key violation
                case 2601:
                    ErrorMessage = "Lỗi khóa chính: Dữ liệu đã tồn tại trong hệ thống.";
                    break;
                case 2:
                    ErrorMessage = "Không thể kết nối đến cơ sở dữ liệu. Vui lòng thử lại sau.";
                    break;
                default:
                    ErrorMessage = $"Lỗi SQL (Mã: {ex.Number}): {ex.Message}";
                    break;
            }
        }

        // AJAX Handler để xóa khách hàng
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostXoaKhachHang(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return new JsonResult(new { success = false, message = "ID khách hàng không hợp lệ" });

                // Kiểm tra xem có đơn hàng liên quan không
                var coDonHang = await _context.DonHang.AnyAsync(d => d.IDKhachHang == userId);
                if (coDonHang)
                {
                    return new JsonResult(new { success = false, message = "Không thể xóa khách hàng vì có đơn hàng liên quan." });
                }

                var nguoiDung = await _context.NguoiDung.FindAsync(userId);
                if (nguoiDung == null)
                    return new JsonResult(new { success = false, message = "Không tìm thấy khách hàng" });

                // Xóa UserRole trước (tránh lỗi foreign key)
                var userRoles = _context.UserRole.Where(ur => ur.UserId == userId);
                _context.UserRole.RemoveRange(userRoles);

                _context.NguoiDung.Remove(nguoiDung);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Xóa khách hàng thành công" });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    return new JsonResult(new { success = false, message = "Không thể xóa vì có dữ liệu liên quan." });
                }
                return new JsonResult(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    // ViewModels
    public class NguoiDungViewModel
    {
        public string UserId { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? Tuoi { get; set; }
        public TrangThaiNguoiDung TrangThai { get; set; }
    }

    public class DonHangViewModel
    {
        public string DonHangId { get; set; }
        public string KhachHangId { get; set; }
        public string KhachHangTen { get; set; }
        public string ChuyenId { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public DateTime ThoiGianHetHan { get; set; }
    }

    // Thống kê đơn hàng ViewModel
    public class ThongKeDonHangViewModel
    {
        public int DonHangHomNay { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public int DonHangThangNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int DonHangNamNay { get; set; }
        public decimal DoanhThuNamNay { get; set; }
        public int DonHangFiltered { get; set; }
        public decimal DoanhThuFiltered { get; set; }
        public int DaThanhToanHomNay { get; set; }
        public int ChoThanhToanHomNay { get; set; }
        public int DaHuyHomNay { get; set; }
    }

    // PaginatedList helper
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}