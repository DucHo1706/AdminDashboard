using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class LoaiXe
    {
        [Key]
        [StringLength(10)]
        public string LoaiXeId { get; set; }

        [Required, StringLength(50)]
        public string TenLoaiXe { get; set; }
    }
}
