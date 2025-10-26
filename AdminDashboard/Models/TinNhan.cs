using System;

namespace AdminDashboard.Models
{
    public class TinNhan
    {
        public int Id { get; set; }
        public string NguoiGuiId { get; set; }
        public string NguoiNhanId { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}
