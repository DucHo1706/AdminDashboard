using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    [Table("NhaXe")]
    public class NhaXe
    {
        [Key]
        [StringLength(255)]
        public string NhaXeId { get; set; }

        [Required(ErrorMessage = "Tên nhà xe không được để trống")]
        [StringLength(100)]
        public string TenNhaXe { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(200)]
        public string? DiaChi { get; set; }

        // 0: Chờ duyệt, 1: Đang hoạt động, 2: Bị khóa
        public int TrangThai { get; set; } = 0;

        // Liên kết ngược về danh sách nhân viên/chủ xe (nếu cần dùng sau này)
        public ICollection<NguoiDung> NhanViens { get; set; }
    }
}