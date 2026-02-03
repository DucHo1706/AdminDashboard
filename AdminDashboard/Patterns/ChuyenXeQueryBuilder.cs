using AdminDashboard.Models;
using System;
using System.Linq;

namespace AdminDashboard.Patterns
{
    public class ChuyenXeQueryBuilder
    {
        private IQueryable<ChuyenXe> _query;

        public ChuyenXeQueryBuilder(IQueryable<ChuyenXe> query)
        {
            _query = query;
        }

        public ChuyenXeQueryBuilder FilterByNhaXe(string nhaXeId)
        {
            if (!string.IsNullOrEmpty(nhaXeId))
            {
                _query = _query.Where(c => c.Xe.NhaXeId == nhaXeId);
            }
            return this;
        }

        public ChuyenXeQueryBuilder FilterByDate(DateTime? date)
        {
            if (date.HasValue)
            {
                _query = _query.Where(c => c.NgayDi.Date == date.Value.Date);
            }
            return this;
        }

        public ChuyenXeQueryBuilder FilterByRoute(string noiDi, string noiDen)
        {
            // Logic tìm kiếm theo lộ trình (giả sử có TenTram)
            if (!string.IsNullOrEmpty(noiDi))
            {
                _query = _query.Where(c => c.LoTrinh.TramDiNavigation.TenTram.Contains(noiDi));
            }
            if (!string.IsNullOrEmpty(noiDen))
            {
                _query = _query.Where(c => c.LoTrinh.TramToiNavigation.TenTram.Contains(noiDen));
            }
            return this;
        }

        public ChuyenXeQueryBuilder SortByTime()
        {
            _query = _query.OrderBy(c => c.GioDi);
            return this;
        }

        public IQueryable<ChuyenXe> Build()
        {
            return _query;
        }
    }
}