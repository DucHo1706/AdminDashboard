using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Một số DB không có cột này → bỏ ánh xạ để tránh lỗi truy vấn
        [NotMapped]
        [Required, StringLength(100)]
        public string HoTen { get; set; }

        // Một số DB không có cột này → bỏ ánh xạ để tránh lỗi truy vấn
        [NotMapped]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        // Một số DB không có cột này → bỏ ánh xạ để tránh lỗi truy vấn
        [NotMapped]
        public DateTime? NgaySinh { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "Hoạt động";

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
