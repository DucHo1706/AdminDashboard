using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdminDashboard.ViewComponents
{
    public class SliderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "slider"
            );

            var selectedLocalNames = new[] { "banner1.png", "banner1.png" };

            var localFiles = Directory.Exists(folderPath)
                ? Directory.GetFiles(folderPath)
                             .Where(f => selectedLocalNames.Contains(Path.GetFileName(f)))
                             .Select(f => "/images/slider/" + Path.GetFileName(f)) // Chuyển thành URL tương đối
                             .ToList()
                : new List<string>();

            var onlineFiles = new List<string>
            {
                "https://img.freepik.com/premium-vector/modern-web-banner-template-with-bus-riding-from-start-point-towards-tourist-camp-finish-point-touristic-transportation-adventure-travel-transport-service-vector-illustration-linear-style_198278-9680.jpg?w=2000",
                "https://cdn.vectorstock.com/i/preview-1x/40/24/monochrome-banner-template-with-bus-riding-from-vector-24934024.jpg",
            };

            localFiles.AddRange(onlineFiles);

            return View(localFiles);
        }
    }
}