using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class VaiTro
    {
        [Key]
        [StringLength(255)]
        public string RoleId { get; set; }

        [Required, StringLength(50)]
        public string TenVaiTro { get; set; }

        // Mối quan hệ nhiều-nhiều
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
