using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations.Schema;
>>>>>>> master

namespace AdminDashboard.Models
{
    public class OtpCode
    {
        [Key]
<<<<<<< HEAD
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [EmailAddress]
        [StringLength(255)]
=======
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
>>>>>>> master
        public string Email { get; set; }

        [Required]
        [StringLength(6)]
        public string Code { get; set; }

        [Required]
<<<<<<< HEAD
        public DateTime CreatedAt { get; set; } = DateTime.Now;
=======
        public DateTime CreatedAt { get; set; }
>>>>>>> master

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

<<<<<<< HEAD
        [StringLength(50)]
        public string Purpose { get; set; } = "ResetPassword"; // ResetPassword, VerifyEmail, etc.
=======
        public DateTime? UsedAt { get; set; }
>>>>>>> master
    }
}
