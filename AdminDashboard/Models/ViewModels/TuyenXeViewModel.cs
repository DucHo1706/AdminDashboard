namespace AdminDashboard.Models.ViewModels
{
    public class TuyenXeViewModel
    {
        public string Tinh { get; set; }
        public string TenTram { get; set; }
        public string ImageUrl { get; set; }
        public List<TuyenXeItemViewModel> TuyenXe { get; set; }
    }

    public class TuyenXeItemViewModel
    {
        public string ChuyenId { get; set; }
        public string DiemDen { get; set; }
        public DateTime NgayDi { get; set; }
        public TimeSpan GioDi { get; set; }
        public TimeSpan GioDenDuKien { get; set; }
        public string ThoiGian { get; set; }
        public decimal GiaVe { get; set; }
        public string ImageUrl { get; set; }
    }
}

