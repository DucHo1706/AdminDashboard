using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    [Table("UserRole")]
    public class UserRole
    {

        // Composite Key (Khóa phức hợp)
        public string UserId { get; set; }
        public string RoleId { get; set; }

        // Navigation properties
        public NguoiDung NguoiDung { get; set; }
        public VaiTro VaiTro { get; set; }
    }
}
