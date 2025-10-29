using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AdminDashboard.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        public IActionResult Index(string receiverId = "")
        {
            ViewBag.ReceiverId = receiverId;
            return View();
        }
    }
}
