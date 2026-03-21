using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class ChuyenXeImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        [StringLength(255)]
        public string ChuyenId { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ChuyenId))]
        public virtual ChuyenXe ChuyenXe { get; set; }
    }
}