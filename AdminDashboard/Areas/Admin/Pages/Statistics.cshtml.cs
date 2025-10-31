using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

// Đảm bảo namespace chính xác với vị trí file của bạn
namespace AdminDashboard.Areas.Admin.Pages
{
    public class StatisticsModel : PageModel
    {
        private readonly Db27524Context _context;

        public StatisticsModel(Db27524Context context)
        {
            _context = context;
        }

        [BindProperty]
        public int TongKhachHang { get; set; }
        [BindProperty]
        public int TongDonHang { get; set; }
        [BindProperty]
        public decimal DoanhThuHomNay { get; set; }
        [BindProperty]
        public IEnumerable<NguoiDung> DanhSachKhachHang { get; set; }


        // 3. Viết logic vào đây
        public async Task OnGetAsync()
        {
          
        }
    }
}