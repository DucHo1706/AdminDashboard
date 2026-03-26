using AdminDashboard.Models.TrangThai;

namespace AdminDashboard.Patterns.Prototype
{
    public class ChuyenXeCloneOptions
    {
        public DateTime NgayDi { get; set; }
        public TimeSpan GioDi { get; set; }
        public TimeSpan GioDenDuKien { get; set; }
        public string XeId { get; set; } = string.Empty;
        public string? TaiXeId { get; set; }
        public bool SaoChepHinhAnh { get; set; } = true;
        public TrangThaiChuyenXe TrangThaiMacDinh { get; set; } = TrangThaiChuyenXe.ChoDuyet;
    }
}