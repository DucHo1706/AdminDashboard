using AdminDashboard.Models.TrangThai;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class ChuyenXe
    {
        [Key]
        [StringLength(255)]
        public string ChuyenId { get; set; }

        [Required, StringLength(255)]
        public string LoTrinhId { get; set; }

        [Required, StringLength(255)]
        public string XeId { get; set; }

        [StringLength(255)]
        public string? TaiXeId { get; set; }

        [Required]
        public DateTime NgayDi { get; set; }

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan GioDi { get; set; }

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan GioDenDuKien { get; set; }

        [Required]
        public TrangThaiChuyenXe TrangThai { get; set; }

        [ForeignKey(nameof(LoTrinhId))]
        public virtual LoTrinh LoTrinh { get; set; }

        [ForeignKey(nameof(XeId))]
        public virtual Xe Xe { get; set; }

        [ForeignKey(nameof(TaiXeId))]
        public virtual NhanVien? TaiXe { get; set; }
        public virtual ICollection<ChuyenXeImage> Images { get; set; } = new List<ChuyenXeImage>();
    }
}