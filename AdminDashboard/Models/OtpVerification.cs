using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(6)]
        public string OtpCode { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        [StringLength(255)]
        public string? UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual NguoiDung? User { get; set; }
    }
}
