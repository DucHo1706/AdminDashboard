using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Patterns.Iterator;
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
        public ChuyenXe ChuyenXe { get; set; } = null!;
        public List<VeBanInfo> VeDaBan { get; set; } = new();
        public List<SeatDisplayItem> SoDoGhe { get; set; } = new();
        public int TongSoGhe { get; set; }
        public int SoGheDaBan { get; set; }
        public int SoGheTrong { get; set; }
    }

    public class VeBanInfo
    {
        public string GheID { get; set; } = string.Empty;
        public string SoGhe { get; set; } = string.Empty;
        public string TenKhach { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string ChuyenId { get; set; } = string.Empty;
    }

    public class KetQuaBanVe
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}