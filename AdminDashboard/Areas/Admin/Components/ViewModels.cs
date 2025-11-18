using AdminDashboard.Models;

namespace AdminDashboard.Areas.Admin.Components
{
    public class NguoiDungViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }
        public int? Tuoi { get; set; }
        public TrangThaiNguoiDung TrangThai { get; set; }
    }

    public class DonHangViewModel
    {
        public string DonHangId { get; set; } = string.Empty;
        public string KhachHangId { get; set; } = string.Empty;
        public string KhachHangTen { get; set; } = string.Empty;
        public string ChuyenId { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiThanhToan { get; set; } = string.Empty;
        public DateTime ThoiGianHetHan { get; set; }
    }
}

