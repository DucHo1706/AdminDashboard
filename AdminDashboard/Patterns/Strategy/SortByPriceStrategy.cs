using AdminDashboard.Models;
using AdminDashboard.Patterns.Strategy;

public class SortByPriceStrategy : ISortStrategy
{
    public List<ChuyenXe> Sort(List<ChuyenXe> data)
    {
        return data.OrderBy(x => x.LoTrinh.GiaVeCoDinh).ToList();
    }
}