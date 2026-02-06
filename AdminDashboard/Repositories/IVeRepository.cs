using AdminDashboard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDashboard.Repositories
{
    public interface IVeRepository
    {
        Task<Ve> GetByIdAsync(string veId);
        Task<List<Ve>> GetVeByChuyenIdAsync(string chuyenId);
        Task AddAsync(Ve ve);
        Task UpdateAsync(Ve ve);
        Task DeleteAsync(string veId);
        Task SaveChangesAsync();
    }
}