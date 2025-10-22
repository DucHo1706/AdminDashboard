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
            // 1️⃣ Đường dẫn vật lý đến thư mục chứa ảnh local
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "slider" // 👉 bạn nên tạo thư mục này
            );

            // 2️⃣ Danh sách tên ảnh local muốn hiển thị
            var selectedLocalNames = new[] { "banner1.png", "banner1.png" };

            // 3️⃣ Lấy danh sách các ảnh tìm thấy ở local (nếu có)
            var localFiles = Directory.Exists(folderPath)
                ? Directory.GetFiles(folderPath)
                             .Where(f => selectedLocalNames.Contains(Path.GetFileName(f)))
                             .Select(f => "/images/slider/" + Path.GetFileName(f)) // Chuyển thành URL tương đối
                             .ToList()
                : new List<string>();

            // 4️⃣ Khai báo danh sách các ảnh online
            var onlineFiles = new List<string>
            {
                "https://img.freepik.com/premium-vector/modern-web-banner-template-with-bus-riding-from-start-point-towards-tourist-camp-finish-point-touristic-transportation-adventure-travel-transport-service-vector-illustration-linear-style_198278-9680.jpg?w=2000",
                "https://cdn.vectorstock.com/i/preview-1x/40/24/monochrome-banner-template-with-bus-riding-from-vector-24934024.jpg",
            };

            // 5️⃣ Gộp danh sách ảnh local và online lại với nhau
            // Bắt đầu với danh sách localFiles và thêm tất cả các ảnh từ onlineFiles vào.
            localFiles.AddRange(onlineFiles);

            // 6️⃣ Trả danh sách tổng hợp sang View
            return View(localFiles);
        }
    }
}