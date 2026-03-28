using AdminDashboard.Models;

namespace AdminDashboard.Patterns.State
{
    public class DonHangContext
    {
        public DonHang DonHang { get; private set; }
        private IDonHangState _state;

        public DonHangContext(DonHang donHang)
        {
            DonHang = donHang;
            _state = TaoStateTuDonHang(donHang);
        }

        public void SetState(IDonHangState state)
        {
            _state = state;
        }

        public string LayTenTrangThai()
        {
            return _state.TenTrangThai;
        }

        public void ThanhToan()
        {
            _state.ThanhToan(this);
        }

        public void HuyDon()
        {
            _state.HuyDon(this);
        }

        public string LayNhanHienThi()
        {
            return _state.LayNhanHienThi(DonHang);
        }

        public string LayBadgeClass()
        {
            return _state.LayBadgeClass(DonHang);
        }

        public string LayMauSacTrangThai()
        {
            return _state.LayMauSacTrangThai(DonHang);
        }

        private IDonHangState TaoStateTuDonHang(DonHang donHang)
        {
            return donHang.TrangThaiThanhToan switch
            {
                "Da thanh toan" => new DaThanhToanState(),
                "Da huy" => new DaHuyState(),
                _ => new ChoThanhToanState()
            };
        }
    }
}