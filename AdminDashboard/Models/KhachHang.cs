using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class KhachHang
    {
        [Key]
        [StringLength(10)]
        public string IDKhachHang { get; set; }

        [Required, StringLength(100)]
        public string TenKhachHang { get; set; }

        [Required, StringLength(100)]
        public string DiaChiMail { get; set; }

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public NguoiDung? NguoiDung { get; set; }
    }
}
