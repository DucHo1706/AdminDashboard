using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.ViewComponents 
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string activeMenu = "")
        {
            ViewBag.ActiveMenu = activeMenu;
            return View();
        }
    }
}