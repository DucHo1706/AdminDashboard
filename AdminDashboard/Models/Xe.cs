using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class Xe
    {
        [Key]
        [StringLength(255)]
        public string XeId { get; set; }

        [Required]
        [StringLength(20)]
        public string BienSoXe { get; set; }

        [Required]
        [StringLength(255)]
        public string LoaiXeId { get; set; }

        [ForeignKey("LoaiXeId")]
        public virtual LoaiXe? LoaiXe { get; set; }

        // Navigation property - sửa lại
        [StringLength(255)] // Quan trọng: Phải cùng kiểu dữ liệu với NhaXeId bên bảng NhaXe
        public string NhaXeId { get; set; }

        [ForeignKey("NhaXeId")]
        public virtual NhaXe NhaXe { get; set; }
        public virtual ICollection<Ghe>? DanhSachGhe { get; set; }

        [NotMapped]
        public int SoLuongGhe { get; set; } = 40;
    }
}