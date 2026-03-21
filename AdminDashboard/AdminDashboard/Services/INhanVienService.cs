using AdminDashboard.Models.ViewModels;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public interface INhanVienService
    {
        Task<string> TaoNhanVienAsync(TaoNhanVienRequest req, string nhaXeId);
    }
}