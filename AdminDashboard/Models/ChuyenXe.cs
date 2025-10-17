using AdminDashboard.Models.TrangThai;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public TimeSpan GioDi { get; set; }

        [Required]
        public TimeSpan GioDenDuKien { get; set; }

        [Required]
        public TrangThaiChuyenXe TrangThai { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LoTrinhId))]
        public virtual LoTrinh LoTrinh { get; set; }

        [ForeignKey(nameof(XeId))]
        public virtual Xe Xe { get; set; }

		[ForeignKey(nameof(TaiXeId))]
		public virtual NguoiDung TaiXe { get; set; }



	}
}
