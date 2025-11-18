
using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly Db27524Context _context;

        public HomeController(Db27524Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Redirect to Blazor Statistics page
            return Redirect("/Admin/Statistics");
        }

    }
}
