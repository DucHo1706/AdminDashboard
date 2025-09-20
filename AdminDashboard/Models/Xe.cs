using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class Xe
    {
        [Key]
        [StringLength(10)]
        public string XeId { get; set; }

        [Required, StringLength(9)]
        public string BienSoXe { get; set; }

        [Required, StringLength(10)]
        public string LoaiXeId { get; set; }

        [ForeignKey(nameof(LoaiXeId))]
        public LoaiXe LoaiXe { get; set; }
    }
}
