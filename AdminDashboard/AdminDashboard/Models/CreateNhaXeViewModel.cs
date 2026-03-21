using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Areas.Admin.Models
{
    public class CreateNhaXeViewModel
    {
        // --- Phần 1: Thông tin Nhà xe ---
        [Required(ErrorMessage = "Tên nhà xe là bắt buộc")]
        public string TenNhaXe { get; set; }

        [Required]
        public string SoDienThoaiNhaXe { get; set; }

        public string DiaChi { get; set; }

        // --- Phần 2: Thông tin Tài khoản Chủ xe (Cấp cho họ) ---
        [Required(ErrorMessage = "Email chủ xe là bắt buộc")]
        [EmailAddress]
        public string EmailChuXe { get; set; }

        [Required]
        public string HoTenChuXe { get; set; }

        [Required]
        public string MatKhauMacDinh { get; set; } // Admin đặt pass mặc định (VD: 123456)
    }
}