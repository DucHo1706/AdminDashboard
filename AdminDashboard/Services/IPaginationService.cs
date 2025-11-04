using AdminDashboard.Models;

namespace AdminDashboard.Services
{
    public interface IPaginationService
    {
        /// <summary>
        /// Tạo PaginatedList từ query và tham số phân trang
        /// </summary>
        Task<PaginatedList<T>> CreatePagedListAsync<T>(
            IQueryable<T> query, 
            int pageIndex, 
            int pageSize = 5
        ) where T : class;

        /// <summary>
        /// Tạo PaginatedList từ list có sẵn
        /// </summary>
        PaginatedList<T> CreatePagedList<T>(
            IEnumerable<T> source, 
            int count, 
            int pageIndex, 
            int pageSize = 5
        ) where T : class;
    }
}

