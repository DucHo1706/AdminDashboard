using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Authorization;

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IThongKeService _thongKeService;

        public DashboardController(IThongKeService thongKeService)
        {
            _thongKeService = thongKeService;
        }

        public async Task<IActionResult> Thongke()
        {
            try
            {
                var model = await _thongKeService.LayThongKeAsync(User);
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("AccessDenied", "Auth", new { area = "" });
            }
        }
    }
}