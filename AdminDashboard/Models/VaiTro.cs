using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class VaiTro
    {
        [Key]
        [StringLength(10)]
        public string RoleId { get; set; }

        [Required, StringLength(50)]
        public string TenVaiTro { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
