using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public interface IBanVeService
    {
        Task<List<ChuyenXe>> GetChuyenXeBanVeAsync(string nhaXeId, DateTime ngayDi);

        Task<SoDoGheDTO> GetSoDoGheAsync(string chuyenId, string nhaXeId);

        Task<KetQuaBanVe> DatVeTaiQuayAsync(DatVeTaiQuayRequest req, string nhaXeId, string tenNhanVien);

        Task<KetQuaBanVe> HuyVeAsync(string chuyenId, string soGhe, string nhaXeId);
        // Tìm vé theo: Mã vé, SĐT, hoặc Mã đơn hàng
        Task<VeBanInfo> TraCuuVeAsync(string keyword, string nhaXeId);

        // Đổi ghế (Vé ID cũ -> Số ghế mới muốn ngồi)
        Task<KetQuaBanVe> DoiGheAsync(string veId, string soGheMoi, string nhaXeId);
    }

    public class SoDoGheDTO
    {
        public ChuyenXe ChuyenXe { get; set; }
        public List<VeBanInfo> VeDaBan { get; set; }
    }

    public class VeBanInfo
    {
        public string GheID { get; set; }
        public string SoGhe { get; set; }
        public string TenKhach { get; set; }
        public string Sdt { get; set; }
        public string TrangThai { get; set; }
        public string ChuyenId { get; set; }
    }
        
    public class KetQuaBanVe
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}