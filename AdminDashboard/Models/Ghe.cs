using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class Ghe
    {
        [Key]
        [StringLength(10)]
        public string GheID { get; set; }

        [Required, StringLength(10)]
        public string XeId { get; set; }

        [Required, StringLength(6)]
        public string SoGhe { get; set; }

        [Required, StringLength(50)]
        public string TrangThai { get; set; }

        [ForeignKey(nameof(XeId))]
        public Xe Xe { get; set; }


    }
}
