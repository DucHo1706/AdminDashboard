using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class BaiViet
    {
        [Key]
        [StringLength(255)]
        public string Id { get; set; }

        [Required, StringLength(200)]
        public string TieuDe { get; set; }

        [Required]
        public string NoiDung { get; set; }

        [Required]
        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Required, StringLength(50)]
        public string TrangThai { get; set; } = "Đã Đăng";

        [StringLength(255)]
        public string? AdminId { get; set; }

        [ForeignKey(nameof(AdminId))]
        public NguoiDung? Admin { get; set; }
    }
}
