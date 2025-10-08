using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    [Table("UserRole")]
    public class UserRole
    {

        [Key, Column(Order = 0)]
        [ForeignKey(nameof(NguoiDung))]
        public string UserId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey(nameof(VaiTro))]
        public string RoleId { get; set; }

        public NguoiDung NguoiDung { get; set; }
        public VaiTro VaiTro { get; set; }
    }
}
