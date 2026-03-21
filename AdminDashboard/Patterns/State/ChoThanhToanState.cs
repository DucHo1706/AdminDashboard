using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;

namespace AdminDashboard.Patterns.State
{
    public class ChoThanhToanState : IDonHangState
    {
        public string TenTrangThai => "Cho thanh toan";

        public void ThanhToan(DonHangContext context)
        {
            context.DonHang.TrangThaiThanhToan = "Da thanh toan";
            context.SetState(new DaThanhToanState());
        }

        public void HuyDon(DonHangContext context)
        {
            context.DonHang.TrangThaiThanhToan = "Da huy";
            context.SetState(new DaHuyState());
        }

        public string LayNhanHienThi(DonHang donHang)
        {
            return "Chờ thanh toán";
        }

        public string LayBadgeClass(DonHang donHang)
        {
            return "badge-pending";
        }

        public string LayMauSacTrangThai(DonHang donHang)
        {
            return "status-pending";
        }
    }
}