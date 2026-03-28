using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Strategy
{
    public interface ISortStrategy
    {
        List<ChuyenXe> Sort(List<ChuyenXe> data);
    }

}

