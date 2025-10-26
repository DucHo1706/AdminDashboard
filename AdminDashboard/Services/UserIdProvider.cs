using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AdminDashboard.Services
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Lấy userId từ Claims (Identity)
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? connection.User?.Identity?.Name;
        }

    }
}
