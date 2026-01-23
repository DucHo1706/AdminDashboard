using AdminDashboard.Models.TrangThai;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần thư viện này

namespace AdminDashboard.Models
{
    public class ChuyenXe
    {
        [Key]
        [StringLength(255)]
        public string ChuyenId { get; set; }

        // ... (Các trường LoTrinhId, XeId giữ nguyên) ...
        [Required, StringLength(255)]
        public string LoTrinhId { get; set; }

        [Required, StringLength(255)]
        public string XeId { get; set; }

        [StringLength(255)]
        public string? TaiXeId { get; set; }

        // --- SỬA ĐOẠN NÀY ---
        [Required]
        public DateTime NgayDi { get; set; } // Ngày riêng

        [Required]
        [Column(TypeName = "time")] // 
        public TimeSpan GioDi { get; set; }

        [Required]
        [Column(TypeName = "time")] // <--- THÊM DÒNG NÀY
        public TimeSpan GioDenDuKien { get; set; }
        // --------------------

        [Required]
        public TrangThaiChuyenXe TrangThai { get; set; }

        // ... (Các Navigation properties giữ nguyên) ...
        [ForeignKey(nameof(LoTrinhId))]
        public virtual LoTrinh LoTrinh { get; set; }

        [ForeignKey(nameof(XeId))]
        public virtual Xe Xe { get; set; }

        [ForeignKey(nameof(TaiXeId))]
        public virtual NguoiDung TaiXe { get; set; }

        public virtual ICollection<ChuyenXeImage> Images { get; set; } = new List<ChuyenXeImage>();
    }
}