using System.ComponentModel.DataAnnotations;
namespace AdminDashboard.Models.TrangThai

{
    public enum TrangThaiChuyenXe
    {
        [Display(Name = "Chờ Admin Duyệt")]
        ChoDuyet = -1,

        [Display(Name = "Đã Lên Lịch")]
        DaLenLich = 0, // Admin duyệt xong sẽ chuyển sang trạng thái này

        [Display(Name = "Đang Mở Bán Vé")]
        DangMoBanVe = 1,

        [Display(Name = "Chờ Khởi Hành")]
        ChoKhoiHanh = 2,

        [Display(Name = "Đang Di Chuyển")]
        DangDiChuyen = 3,

        [Display(Name = "Đã Hoàn Thành")]
        DaHoanThanh = 4,

        [Display(Name = "Đã Hủy")]
        DaHuy = 5
    }
}
