using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class LoTrinh
    {
        [Key]
        [StringLength(255)]
        [Display(Name = "Mã Lộ Trình")]
        public string LoTrinhId { get; set; }

        [Required(ErrorMessage = "Trạm đi là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Trạm Đi")]
        public string TramDi { get; set; }

        [Required(ErrorMessage = "Trạm đến là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Trạm Đến")]
        public string TramToi { get; set; }

        [Display(Name = "Giá Vé Cố Định")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaVeCoDinh { get; set; }

        [ForeignKey("TramDi")]
        [Display(Name = "Trạm Đi")]
        public virtual Tram TramDiNavigation { get; set; }

        [ForeignKey("TramToi")]
        [Display(Name = "Trạm Đến")]
        public virtual Tram TramToiNavigation { get; set; }
       
    }
}