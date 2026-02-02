using System.Collections.Generic;
using AdminDashboard.Areas.Admin.Components;

namespace AdminDashboard.Areas.Admin.Models
{
    public class ThongKeViewModel
    {
        // 1. CÁC CHỈ SỐ CƠ BẢN
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int TongSoNguoiDung { get; set; }
        public int SoChuyenXeHomNay { get; set; }

        public int TongNhaXe { get; set; }  

        public int TongTram { get; set; }

        // 2. DỮ LIỆU BIỂU ĐỒ DOANH THU (Line Chart)
        // Quan trọng: Phải là List<T> mới dùng được hàm .Add()
        public List<string> LabelsBieuDo { get; set; } = new List<string>();
        public List<decimal> DataBieuDo { get; set; } = new List<decimal>();

        // 3. DỮ LIỆU BIỂU ĐỒ TRÒN (Status Chart)
        public List<string> StatusLabels { get; set; } = new List<string>();
        public List<int> StatusData { get; set; } = new List<int>();

        // 4. DANH SÁCH CHI TIẾT
        public List<ChuyenXeDashboardItem> CacChuyenXeSapChay { get; set; } = new List<ChuyenXeDashboardItem>();
        public List<TopLoTrinhItem> TopLoTrinh { get; set; } = new List<TopLoTrinhItem>();
    }

    public class ChuyenXeDashboardItem
    {
        public string ChuyenId { get; set; }
        public string TenLoTrinh { get; set; }
        public string TenTaiXe { get; set; }
        public DateTime NgayDi { get; set; }
        public TimeSpan GioDi { get; set; }
        public int PhanTramLapDay { get; set; }
        public string TrangThai { get; set; }
    }

    public class TopLoTrinhItem
    {
        public string TenLoTrinh { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoVeBan { get; set; }
    }
}