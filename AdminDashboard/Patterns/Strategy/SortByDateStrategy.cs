using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Strategy
{
    public class SortByDateStrategy : ISortStrategy
    {
        public List<ChuyenXe> Sort(List<ChuyenXe> data)
        {
            return data.OrderBy(c => c.NgayDi)
                       .ThenBy(c => c.GioDi)
                       .ToList();
        }
    }
}

