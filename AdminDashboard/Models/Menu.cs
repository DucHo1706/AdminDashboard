using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class Menu
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; }

        [Required, StringLength(100)]
        public string TenMenu { get; set; }

        [Required, StringLength(255)]
        public string Link { get; set; }

        [Required]
        public int ViTri { get; set; } // 1=Header, 2=Sidebar, 3=Footer

        public int ThuTu { get; set; } = 0;
    }
}
