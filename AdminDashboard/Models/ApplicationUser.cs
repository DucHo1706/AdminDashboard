using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; }
        public bool IsOnline { get; set; } = false;
    }
}
