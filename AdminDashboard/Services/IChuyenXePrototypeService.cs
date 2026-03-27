using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;

namespace AdminDashboard.Services
{
    public interface IChuyenXePrototypeService
    {
        Task<ChuyenXe?> GetSourceTripAsync(string chuyenId, string nhaXeId);
        Task<NhanBanChuyenXeResult> NhanBanTuMauAsync(NhanBanChuyenXeRequest request, string nhaXeId);
    }
}