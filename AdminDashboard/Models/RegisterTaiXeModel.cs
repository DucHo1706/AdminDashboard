using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class RegisterTaiXeModel
    {
        [Required]
        public string TenDangNhap { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string HoTen { get; set; }

        public string SoDienThoai { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required]
        public string SoBangLai { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayCap { get; set; }

        [Required]
        public string NoiCap { get; set; }
    }
}
