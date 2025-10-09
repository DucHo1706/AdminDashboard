using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.Login
{
    public class DangKy
    {  
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }

       

        [Required(ErrorMessage = "Họ tên bắt buộc")]
        public string HoTen { get; set; }

       

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }
    }
}
