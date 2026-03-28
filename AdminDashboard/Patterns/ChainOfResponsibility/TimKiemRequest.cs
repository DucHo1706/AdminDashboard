using AdminDashboard.Models;

namespace AdminDashboard.Patterns.ChainOfResponsibility
{
    public class TimKiemRequest
    {
        public IQueryable<ChuyenXe> Query { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public string NgayDi { get; set; }
    }
}