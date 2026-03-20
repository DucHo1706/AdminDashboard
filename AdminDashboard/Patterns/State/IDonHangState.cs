using AdminDashboard.Models;

namespace AdminDashboard.Patterns.State
{
    public interface IDonHangState
    {
        string TenTrangThai { get; }

        void ThanhToan(DonHangContext context);
        void HuyDon(DonHangContext context);

        string LayNhanHienThi(DonHang donHang);
        string LayBadgeClass(DonHang donHang);
        string LayMauSacTrangThai(DonHang donHang);
    }
}