using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.Login
{
    public class DangNhap
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
        public string EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
    }
}
