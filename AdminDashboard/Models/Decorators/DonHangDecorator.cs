using AdminDashboard.Models;

namespace AdminDashboard.Models.Decorators
{
    public class DonHangDecorator
    {
        private readonly DonHang _donHang;

        public DonHangDecorator(DonHang donHang)
        {
            _donHang = donHang;
        }

        public string LayMauSacTrangThai()
        {
            if (_donHang.TrangThaiThanhToan == "Da thanh toan" &&
                _donHang.ChuyenXe?.TrangThai == AdminDashboard.Models.TrangThai.TrangThaiChuyenXe.DaHoanThanh)
                return "status-paid"; // Màu xanh lá (Hoàn thành)

            if (_donHang.TrangThaiThanhToan == "Da thanh toan")
                return "status-paid"; // Màu xanh lá (Đã thanh toán)

            if (_donHang.TrangThaiThanhToan == "DangChoThanhToan")
                return "status-pending"; // Màu vàng (Chờ)

            return "status-cancelled"; // Màu xám (Hủy)
        }

        public string LayNhanTrangThai()
        {
            if (_donHang.TrangThaiThanhToan == "Da thanh toan" &&
                _donHang.ChuyenXe?.TrangThai == AdminDashboard.Models.TrangThai.TrangThaiChuyenXe.DaHoanThanh)
                return "Hoàn thành";

            if (_donHang.TrangThaiThanhToan == "Da thanh toan")
                return "Đã thanh toán";

            if (_donHang.TrangThaiThanhToan == "DangChoThanhToan")
                return "Chờ thanh toán";

            return "Đã hủy";
        }

        public string LayBadgeClass()
        {
            if (_donHang.TrangThaiThanhToan == "Da thanh toan") return "badge-paid";
            if (_donHang.TrangThaiThanhToan == "DangChoThanhToan") return "badge-pending";
            return "badge-cancelled";
        }
    }
}