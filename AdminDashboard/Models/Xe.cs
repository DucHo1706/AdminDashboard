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
        public virtual ICollection<Ghe>? DanhSachGhe { get; set; }

        [NotMapped]
<<<<<<< HEAD
        public int SoLuongGhe { get; set; } = 40;
=======
        public int SoLuongGhe { get; set; } = 0;
>>>>>>> origin/ThanhToanMuaVe
    }
}