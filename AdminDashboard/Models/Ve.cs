using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class Ve
    {
        [Key]
        [StringLength(10)]
        public string VeId { get; set; }

        [Required, StringLength(10)]
        public string DonHangId { get; set; }

        [Required, StringLength(10)]
        public string GheID { get; set; }

        [Required, Column(TypeName = "numeric(10,2)")]
        public decimal Gia { get; set; }

        [ForeignKey(nameof(DonHangId))]
        public DonHang DonHang { get; set; }

        [ForeignKey(nameof(GheID))]
        public Ghe Ghe { get; set; }

    }
}
