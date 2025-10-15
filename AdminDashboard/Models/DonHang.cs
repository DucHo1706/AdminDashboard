using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class DonHang
    {
        [Key]
        [StringLength(255)]
        public string DonHangId { get; set; }

        [Required, StringLength(255)]
        public string IDKhachHang { get; set; }

        [Required, StringLength(255)]
        public string ChuyenId { get; set; }

        [Required]
        public DateTime NgayDat { get; set; }

        [Required, Column(TypeName = "numeric(10,2)")]
        public decimal TongTien { get; set; }

        [Required, StringLength(50)]
        public string TrangThaiThanhToan { get; set; }
<<<<<<< HEAD
=======
        public DateTime ThoiGianHetHan { get; set; }
>>>>>>> origin/ThanhToanMuaVe

        [ForeignKey(nameof(IDKhachHang))]
        public NguoiDung nguoiDung { get; set; }

        [ForeignKey(nameof(ChuyenId))]
        public ChuyenXe ChuyenXe { get; set; }
    }
}
