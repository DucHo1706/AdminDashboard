using System;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.ViewModels
{
    public class TaiXeRegistrationViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Bằng lái xe không được để trống")]
        [Display(Name = "Bằng lái xe")]
        public string BangLaiXe { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }
    }
}
