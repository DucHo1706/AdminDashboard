using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminDashboard.Controllers
{
    public class Home_UserController : Controller
    {
        private readonly Db27524Context _context;

        public Home_UserController(Db27524Context context)
        {
            _context = context;
        }

        public IActionResult Home_User()
        {
            return View();
        }

        

        }
}