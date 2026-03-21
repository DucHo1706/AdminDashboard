using AdminDashboard.Models;
using System;

namespace AdminDashboard.Patterns
{
    // Mẫu Factory Method: Chuyên trách việc sản xuất đối tượng
    public static class TicketFactory
    {
        // Hàm tạo Đơn hàng chuẩn
        public static DonHang CreateDonHang(string chuyenId, decimal tongTien, string hoTen, string sdt, string ghiChu, bool daThanhToan, string nhanVienName)
        {
            return new DonHang
            {
                DonHangId = Guid.NewGuid().ToString("N"),
                ChuyenId = chuyenId,
                IDKhachHang = null,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThaiThanhToan = daThanhToan ? "Da thanh toan" : "Cho thanh toan",
                HoTenNguoiDat = hoTen,
                SdtNguoiDat = sdt,
                GhiChu = ghiChu ?? $"Bán tại quầy bởi {nhanVienName}",
                ThoiGianHetHan = DateTime.Now.AddMinutes(15) 
            };
        }

        // Hàm tạo Vé chuẩn
        public static Ve CreateVe(string donHangId, string gheId, decimal giaVe)
        {
            return new Ve
            {
                VeId = Guid.NewGuid().ToString(),
                DonHangId = donHangId,
                GheID = gheId,
                Gia = giaVe
            };
        }
    }
}