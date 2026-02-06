using AdminDashboard.Models;

namespace AdminDashboard.Models.ViewModels
{
    public class LichSuMuaVeViewModel
    {
        public List<DonHang> DonHangHienTai { get; set; } = new List<DonHang>();

        public List<DonHang> DonHangDaDi { get; set; } = new List<DonHang>();

        public List<DonHang> DonHangDaHuy { get; set; } = new List<DonHang>();
    }
}