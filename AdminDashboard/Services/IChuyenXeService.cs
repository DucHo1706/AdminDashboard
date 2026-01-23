using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public interface IChuyenXeService
    {
    
        Task<KetQuaTaoLich> TaoLichTuDongAsync(TaoLichChayRequest request, string nhaXeId);

     
        Task<string> UpdateChuyenXeAsync(ChuyenXe model, string deletedImages, IFormFileCollection newImages, string nhaXeId);

      
        Task<string> DeleteChuyenXeAsync(string id, string nhaXeId);
        Task<int> DuyetNhieuChuyenAsync(List<string> ids, string adminId);
    }
}