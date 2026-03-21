using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class DonHang
    {
        [Key]
        [StringLength(255)]
        public string DonHangId { get; set; }
        [StringLength(255)]
        public string? IDKhachHang { get; set; }

        [Required, StringLength(255)]
        public string ChuyenId { get; set; }

        [Required]
        public DateTime NgayDat { get; set; }

        [Required, Column(TypeName = "numeric(10,2)")]
        public decimal TongTien { get; set; }

        [Required, StringLength(50)]
        public string TrangThaiThanhToan { get; set; }

        public DateTime ThoiGianHetHan { get; set; }

        [StringLength(100)]
        public string? HoTenNguoiDat { get; set; } 

        [StringLength(20)]
        public string? SdtNguoiDat { get; set; }  

        [StringLength(100)]
        public string? EmailNguoiDat { get; set; } 

        public string? GhiChu { get; set; }     

        [ForeignKey(nameof(IDKhachHang))]
        public virtual NguoiDung? nguoiDung { get; set; }

        [ForeignKey(nameof(ChuyenId))]
        public virtual ChuyenXe ChuyenXe { get; set; }
    }
}