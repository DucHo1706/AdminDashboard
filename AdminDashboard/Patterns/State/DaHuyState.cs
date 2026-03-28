using AdminDashboard.Models;

namespace AdminDashboard.Patterns.State
{
    public class DaHuyState : IDonHangState
    {
        public string TenTrangThai => "Da huy";

        public void ThanhToan(DonHangContext context)
        {
            // Đã hủy thì không thanh toán lại
        }

        public void HuyDon(DonHangContext context)
        {
            // Đã hủy rồi thì không làm gì nữa
        }

        public string LayNhanHienThi(DonHang donHang)
        {
            return "Đã hủy";
        }

        public string LayBadgeClass(DonHang donHang)
        {
            return "badge-cancelled";
        }

        public string LayMauSacTrangThai(DonHang donHang)
        {
            return "status-cancelled";
        }
    }
}