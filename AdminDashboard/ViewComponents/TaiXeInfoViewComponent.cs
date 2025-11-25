using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.TransportDBContext;

namespace AdminDashboard.ViewComponents
{
    public class TaiXeInfoViewComponent : ViewComponent
    {
        private readonly Db27524Context _context; // Thay ApplicationDbContext bằng tên DbContext thực tế của bạn

        public TaiXeInfoViewComponent(Db27524Context context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            // Tìm thông tin tài xế dựa trên UserId của người dùng đang đăng nhập
            var taiXe = await _context.TaiXes.FirstOrDefaultAsync(x => x.UserId == userId);

            // Nếu không tìm thấy (người này không phải tài xế), View vẫn nhận null để xử lý ẩn đi
            return View(taiXe);
        }
    }
}