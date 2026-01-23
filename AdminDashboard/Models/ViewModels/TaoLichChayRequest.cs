using Microsoft.AspNetCore.Http;
using System;

namespace AdminDashboard.Models.ViewModels
{
    public class TaoLichChayRequest
    {
        public string LoTrinhId { get; set; }
        public string XeId { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public TimeSpan KhungGioTu { get; set; }
        public TimeSpan KhungGioDen { get; set; }
        public int GianCachPhut { get; set; }
        public TimeSpan ThoiGianDiChuyen { get; set; }
        public IFormFileCollection Images { get; set; } // Nhận file ảnh từ form
    }

    // Class trả về kết quả để Controller biết đường thông báo
    public class KetQuaTaoLich
    {
        public int Success { get; set; }
        public int Skipped { get; set; }
        public string Message { get; set; }
    }
}