using AdminDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Services
{
    public class PaginationService : IPaginationService
    {
        public async Task<PaginatedList<T>> CreatePagedListAsync<T>(
            IQueryable<T> query, 
            int pageIndex, 
            int pageSize = 5
        ) where T : class
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public PaginatedList<T> CreatePagedList<T>(
            IEnumerable<T> source, 
            int count, 
            int pageIndex, 
            int pageSize = 5
        ) where T : class
        {
            return new PaginatedList<T>(source.ToList(), count, pageIndex, pageSize);
        }
    }
}

