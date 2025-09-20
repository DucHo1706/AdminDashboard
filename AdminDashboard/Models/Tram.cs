using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AdminDashboard.Models
{
    public class Tram
    {
        [Key]
        [StringLength(10)]
        public string IdTram { get; set; }

        [Required, StringLength(100)]
        public string TenTram { get; set; }

        [Required, StringLength(255)]
        public string DiaChiTram { get; set; }
    }
}
