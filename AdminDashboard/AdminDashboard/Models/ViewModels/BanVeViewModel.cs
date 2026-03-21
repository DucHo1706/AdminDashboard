using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.ViewModels
{
    public class DatVeTaiQuayRequest
    {
        public string ChuyenId { get; set; }
        public string SoGhe { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }
        public decimal GiaVe { get; set; }
        public bool DaThanhToan { get; set; }
    }

}