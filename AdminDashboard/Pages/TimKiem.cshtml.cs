using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Pages
{
    public class TimKiemModel : PageModel
    {
        private readonly Db27524Context _context;

        public TimKiemModel(Db27524Context context)
        {
            _context = context;
        }

        public List<ChuyenXe> KetQuaTimKiem { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? DiemDiId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? DiemDenId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? NgayDi { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? GioDi { get; set; } // Format: "HH:mm"
        
        [BindProperty(SupportsGet = true)]
        public string? GioDenDuKien { get; set; } // Format: "HH:mm"

        public SelectList DanhSachTram { get; set; }

        public string? TenTramDi { get; set; }
        public string? TenTramDen { get; set; }

        public async Task OnGetAsync()
        {
            // Load danh sách trạm
            var danhSachTram = await _context.Tram.ToListAsync();
            DanhSachTram = new SelectList(danhSachTram, "IdTram", "TenTram");

            // Nếu có tham số tìm kiếm, thực hiện tìm kiếm
            if (!string.IsNullOrEmpty(DiemDiId) && !string.IsNullOrEmpty(DiemDenId) && NgayDi.HasValue)
            {
                var query = _context.ChuyenXe
                    .Include(c => c.LoTrinh)
                        .ThenInclude(lt => lt.TramDiNavigation)
                    .Include(c => c.LoTrinh)
                        .ThenInclude(lt => lt.TramToiNavigation)
                    .Include(c => c.Xe)
                        .ThenInclude(x => x.LoaiXe)
                    .Where(c => c.LoTrinh.TramDi == DiemDiId &&
                                c.LoTrinh.TramToi == DiemDenId &&
                                c.NgayDi.Date == NgayDi.Value.Date &&
                                c.TrangThai == TrangThaiChuyenXe.DaLenLich) // CHỈ TÌM CHUYẾN ĐÃ LÊN LỊCH
                    .AsQueryable();

                // Lọc theo giờ đi nếu có
                if (!string.IsNullOrEmpty(GioDi) && TimeSpan.TryParse(GioDi, out TimeSpan gioDiParsed))
                {
                    query = query.Where(c => c.GioDi == gioDiParsed);
                }

                // Lọc theo giờ đến dự kiến nếu có (bỏ qua nếu là "Tự động")
                if (!string.IsNullOrEmpty(GioDenDuKien) && TimeSpan.TryParse(GioDenDuKien, out TimeSpan gioDenParsed))
                {
                    query = query.Where(c => c.GioDenDuKien == gioDenParsed);
                }
                // Nếu GioDenDuKien rỗng, không filter (tự động +4 giờ - để server xử lý logic này nếu cần)

                KetQuaTimKiem = await query
                    .OrderBy(c => c.GioDi)
                    .ToListAsync();

                // Lấy tên trạm để hiển thị
                var tramDi = await _context.Tram.FindAsync(DiemDiId);
                var tramDen = await _context.Tram.FindAsync(DiemDenId);
                TenTramDi = tramDi?.TenTram;
                TenTramDen = tramDen?.TenTram;
            }
        }
    }
}
