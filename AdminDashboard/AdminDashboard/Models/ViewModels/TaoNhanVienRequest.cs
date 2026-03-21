using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.ViewModels
{
    public class TaoNhanVienRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email sẽ dùng để đăng nhập")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string MatKhau { get; set; }

        public string? CCCD { get; set; }
        public string? SoBangLai { get; set; }
        public string? HangBangLai { get; set; }

        [Required]
        public VaiTroNhanVien VaiTro { get; set; } // 1=Tài xế, 2=Bán vé

        public IFormFile? Avatar { get; set; }
    }
}