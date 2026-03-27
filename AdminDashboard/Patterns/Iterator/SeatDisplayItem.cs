namespace AdminDashboard.Patterns.Iterator
{
    public class SeatDisplayItem
    {
        public string GheId { get; set; } = string.Empty;
        public string SoGhe { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Trong"; // Trong / DaBan
        public string? TenKhach { get; set; }
        public string? SoDienThoai { get; set; }

        public bool CoTheChon => TrangThai == "Trong";
    }
}