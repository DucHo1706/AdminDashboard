using AdminDashboard.Models;
using AdminDashboard.Patterns.State;

namespace AdminDashboard.Models.Decorators
{
    public class DonHangDecorator
    {
        private readonly DonHang _donHang;
        private readonly DonHangContext _context;

        public DonHangDecorator(DonHang donHang)
        {
            _donHang = donHang;
            _context = new DonHangContext(donHang);
        }

        public string LayMauSacTrangThai()
        {
            return _context.LayMauSacTrangThai();
        }

        public string LayNhanTrangThai()
        {
            return _context.LayNhanHienThi();
        }

        public string LayBadgeClass()
        {
            return _context.LayBadgeClass();
        }
    }
}