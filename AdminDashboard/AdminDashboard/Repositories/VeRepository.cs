using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Repositories
{
    public class VeRepository : IVeRepository
    {
        private readonly Db27524Context _context;

        public VeRepository(Db27524Context context)
        {
            _context = context;
        }

        public async Task<Ve> GetByIdAsync(string veId)
        {
            return await _context.Ve
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.VeId == veId);
        }

        public async Task<List<Ve>> GetVeByChuyenIdAsync(string chuyenId)
        {
            return await _context.Ve
                .Include(v => v.DonHang)
                .Include(v => v.Ghe)
                .Where(v => v.DonHang.ChuyenId == chuyenId)
                .ToListAsync();
        }

        public async Task AddAsync(Ve ve)
        {
            await _context.Ve.AddAsync(ve);
        }

        public async Task UpdateAsync(Ve ve)
        {
            _context.Ve.Update(ve);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string veId)
        {
            var ve = await _context.Ve.FindAsync(veId);
            if (ve != null) _context.Ve.Remove(ve);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}