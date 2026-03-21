using AdminDashboard.Areas.Admin.Components;
using AdminDashboard.Areas.Admin.Models;
using System.Security.Claims;

namespace AdminDashboard.Services
{
    public interface IThongKeService
    {
        Task<ThongKeViewModel> LayThongKeAsync(ClaimsPrincipal user);
    }
}