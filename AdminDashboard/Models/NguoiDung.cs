using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class NguoiDung
    {
        [Key]
        [StringLength(10)]
        public string UserId { get; set; }

        [Required, StringLength(255)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Email chưa được điền...")]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "Hoạt động";

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
