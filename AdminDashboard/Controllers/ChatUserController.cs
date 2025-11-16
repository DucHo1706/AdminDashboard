using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Controllers
{
    public class ChatUserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
