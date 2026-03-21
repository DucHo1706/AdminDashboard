using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;

namespace AdminDashboard.Patterns.State
{
    public class DaThanhToanState : IDonHangState
    {
        public string TenTrangThai => "Da thanh toan";

        public void ThanhToan(DonHangContext context)
        {
            // Đã thanh toán rồi thì không làm gì nữa
        }

        public void HuyDon(DonHangContext context)
        {
            context.DonHang.TrangThaiThanhToan = "Da huy";
            context.SetState(new DaHuyState());
        }

        public string LayNhanHienThi(DonHang donHang)
        {
            if (donHang.ChuyenXe?.TrangThai == TrangThaiChuyenXe.DaHoanThanh)
                return "Hoàn thành";

            return "Đã thanh toán";
        }

        public string LayBadgeClass(DonHang donHang)
        {
            return "badge-paid";
        }

        public string LayMauSacTrangThai(DonHang donHang)
        {
            return "status-paid";
        }
    }
}