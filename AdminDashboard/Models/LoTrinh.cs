using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class LoTrinh
    {
        [Key]
        [StringLength(10)]
        public string LoTrinhId { get; set; }

        [Required, StringLength(10)]
        public string TramDi { get; set; }

        [Required, StringLength(10)]
        public string TramToi { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? GiaVeCoDinh { get; set; }

        [ForeignKey(nameof(TramDi))]
        public Tram TramDiNavigation { get; set; }

        [ForeignKey(nameof(TramToi))]
        public Tram TramToiNavigation { get; set; }
    }
}
