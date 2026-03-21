using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Prototype
{
    public class ChuyenXePrototypeResult
    {
        public ChuyenXePrototypeResult(ChuyenXe chuyenXe, IEnumerable<ChuyenXeImage>? images)
        {
            ChuyenXe = chuyenXe;
            Images = images?.ToList() ?? new List<ChuyenXeImage>();
        }

        public ChuyenXe ChuyenXe { get; }
        public IReadOnlyCollection<ChuyenXeImage> Images { get; }
    }
}