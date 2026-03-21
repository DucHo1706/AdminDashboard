using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Services
{
    public interface ITramService
    {
        Task<List<Tram>> GetAllAsync();
        Task<Tram> GetByIdAsync(string id);
        Task<bool> CreateAsync(Tram tram);
        Task<bool> UpdateAsync(Tram tram);
        Task<bool> DeleteAsync(string id);
        Task<string> GenerateNewIdAsync();
    }

    public class TramService : ITramService
    {
        private readonly Db27524Context _context;

        public TramService(Db27524Context context)
        {
            _context = context;
        }

        public async Task<List<Tram>> GetAllAsync()
        {
            return await _context.Tram.OrderBy(t => t.IdTram).ToListAsync();
        }

        public async Task<Tram> GetByIdAsync(string id)
        {
            return await _context.Tram.FindAsync(id);
        }

        public async Task<bool> CreateAsync(Tram tram)
        {
            try
            {
                tram.IdTram = await GenerateNewIdAsync();
                _context.Tram.Add(tram);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Tram tram)
        {
            try
            {
                _context.Tram.Update(tram);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var tram = await GetByIdAsync(id);
                if (tram == null) return false;

                _context.Tram.Remove(tram);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateNewIdAsync()
        {
            var lastTram = await _context.Tram
                .OrderByDescending(t => t.IdTram)
                .FirstOrDefaultAsync();

            if (lastTram == null) return "T001";

            if (int.TryParse(lastTram.IdTram.Substring(1), out int lastNumber))
            {
                return $"T{(lastNumber + 1):D3}";
            }

            // Fallback nếu định dạng ID không đúng
            var maxId = await _context.Tram
                .Where(t => t.IdTram.StartsWith("T"))
                .Select(t => t.IdTram)
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync();

            return maxId == null ? "T001" : $"T{(int.Parse(maxId.Substring(1)) + 1):D3}";
        }
    }
}