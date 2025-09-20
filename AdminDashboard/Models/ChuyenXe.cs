using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class ChuyenXe
    {
        [Key]
        [StringLength(10)]
        public string ChuyenId { get; set; }

        [Required, StringLength(10)]
        public string LoTrinhId { get; set; }

        [Required, StringLength(10)]
        public string XeId { get; set; }

        [Required]
        public DateTime NgayDi { get; set; }

        [Required]
        public TimeSpan GioDi { get; set; }

        [Required]
        public TimeSpan GioDenDuKien { get; set; }

        [Required, StringLength(50)]
        public string TrangThai { get; set; }

        [ForeignKey(nameof(LoTrinhId))]
        public LoTrinh LoTrinh { get; set; }

        [ForeignKey(nameof(XeId))]
        public Xe Xe { get; set; }

    }
}
