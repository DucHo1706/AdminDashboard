using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class OtpCode
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(6)]
        public string Code { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [StringLength(50)]
        public string Purpose { get; set; } = "ResetPassword"; // ResetPassword, VerifyEmail, etc.
    }
}
